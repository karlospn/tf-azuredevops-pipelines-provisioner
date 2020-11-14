package builds

import (
	"encoding/json"
	"fmt"
	"log"
	"strings"

	"github.com/hashicorp/terraform-plugin-sdk/helper/schema"
)

func resourceBuildDefinition() *schema.Resource {
	return &schema.Resource{
		Create: resourceBuildDefinitionCreate,
		Read:   resourceBuildDefinitionRead,
		Update: resourceBuildDefinitionUpdate,
		Delete: resourceBuildDefinitionDelete,
		Schema: map[string]*schema.Schema{
			"create_method": &schema.Schema{
				Type:        schema.TypeString,
				DefaultFunc: schema.EnvDefaultFunc("REST_API_OVERRIDE_CREATE_METHOD", nil),
				Description: "The HTTP route used to CREATE objects of this type on the API server. Overrides  the provider configuration",
				Optional:    true,
			},
			"read_method": &schema.Schema{
				Type:        schema.TypeString,
				Description: "The HTTP route used to READ objects of this type on the API server. Overrides provider configuration",
				DefaultFunc: schema.EnvDefaultFunc("REST_API_OVERRIDE_READ_METHOD", nil),
				Optional:    true,
			},
			"update_method": &schema.Schema{
				Type:        schema.TypeString,
				DefaultFunc: schema.EnvDefaultFunc("REST_API_OVERRIDE_UPDATE_METHOD", nil),
				Description: "The HTTP route used to UPDATE objects of this type on the API server. Overrides provider configuration",
				Optional:    true,
			},
			"destroy_method": &schema.Schema{
				Type:        schema.TypeString,
				DefaultFunc: schema.EnvDefaultFunc("REST_API_OVERRIDE_DESTROY_METHOD", nil),
				Description: "The HTTP route used to DELETE objects of this type on the API server. Overrides provider configuration",
				Optional:    true,
			},
			"path": &schema.Schema{
				Type:        schema.TypeString,
				Description: "Path where the build definition is going to be saved",
				Required:    true,
			},
			"project": &schema.Schema{
				Type:        schema.TypeString,
				Description: "Build definition name",
				Required:    true,
				ForceNew:    true,
			},
			"application_name": &schema.Schema{
				Type:        schema.TypeString,
				Description: "app name",
				Required:    true,
			},
			"build_template": &schema.Schema{
				Type:        schema.TypeString,
				Description: "Task Group name",
				Required:    true,
			},
			"queue_pool": &schema.Schema{
				Type:        schema.TypeString,
				Description: "Build queue pool",
				Required:    true,
			},
			"repository": &schema.Schema{
				Type:        schema.TypeString,
				Description: "Repository name",
				Required:    true,
			},
			"branch": &schema.Schema{
				Type:        schema.TypeString,
				Description: "Branch",
				Required:    true,
			},
			"tags": &schema.Schema{
				Type:        schema.TypeList,
				Description: "Tags",
				Optional:    true,
				Elem: &schema.Schema{
					Type: schema.TypeString,
				},
			},
			"variable_groups": &schema.Schema{
				Type:        schema.TypeList,
				Description: "Variable groups",
				Optional:    true,
				Elem: &schema.Schema{
					Type: schema.TypeString,
				},
			},
			"ci_triggers":       getContinousIntegrationTriggersSchema(),
			"schedule_triggers": getScheduleTriggersSchema(),
			"variables": {
				Type:     schema.TypeMap,
				Optional: true,
				Elem: &schema.Schema{
					Type: schema.TypeString,
				},
			},
			"task_group_revision": {
				Type:     schema.TypeString,
				Computed: true,
			},
			"build_revision": {
				Type:     schema.TypeString,
				Computed: true,
			},
			"revision_taint": {
				Type:     schema.TypeBool,
				Optional: true,
				Default:  false,
			},
		},
	}
}

func getContinousIntegrationTriggersSchema() *schema.Schema {
	return &schema.Schema{
		Type:     schema.TypeList,
		Optional: true,
		MaxItems: 1,
		Elem: &schema.Resource{
			Schema: map[string]*schema.Schema{
				"path_filter": {
					Type:        schema.TypeList,
					Description: "path filter",
					Optional:    true,
					Elem: &schema.Schema{
						Type: schema.TypeString,
					},
				},
				"branch_filter": {
					Type:        schema.TypeList,
					Description: "branch filter",
					Required:    true,
					Elem: &schema.Schema{
						Type: schema.TypeString,
					},
				},
			},
		},
	}
}

func getScheduleTriggersSchema() *schema.Schema {
	return &schema.Schema{
		Type:     schema.TypeList,
		Optional: true,
		Elem: &schema.Resource{
			Schema: map[string]*schema.Schema{
				"day_of_the_week": {
					Type:        schema.TypeInt,
					Description: "Day of the week",
					Required:    true,
				},
				"hours": {
					Type:        schema.TypeInt,
					Description: "Hours",
					Required:    true,
				},
				"minutes": {
					Type:        schema.TypeInt,
					Description: "Minutes",
					Required:    true,
				},
				"time_zone": {
					Type:        schema.TypeString,
					Description: "TimeZone",
					Required:    true,
				},
				"branch_filter": {
					Type:        schema.TypeList,
					Description: "branch filter",
					Required:    true,
					Elem: &schema.Schema{
						Type: schema.TypeString,
					},
				},
			},
		},
	}
}

func resourceBuildDefinitionCreate(d *schema.ResourceData, meta interface{}) error {
	client := meta.(*api_client)

	route := d.Get("create_method").(string)
	if route == "" {
		route = client.create_method
	}

	build := &BuildDefinition{
		Path:              d.Get("path").(string),
		Project:           d.Get("project").(string),
		ApplicationName:   d.Get("application_name").(string),
		BuildTemplate:     d.Get("build_template").(string),
		QueuePool:         d.Get("queue_pool").(string),
		Repository:        d.Get("repository").(string),
		Branch:            d.Get("branch").(string),
		TaskGroupRevision: d.Get("task_group_revision").(string),
		BuildRevision:     d.Get("build_revision").(string),
		Tags:              interface2StringList(d.Get("tags").([]interface{})),
		VariableGroups:    interface2StringList(d.Get("variable_groups").([]interface{})),
		CITriggers:        getCITriggersFromSchema(d),
		ScheduleTriggers:  getScheduleTriggersFromSchema(d),
	}

	if value, ok := d.GetOk("variables"); ok {
		vars := make(map[string]string)
		variables := value.(map[string]interface{})
		for k, v := range variables {
			vars[k] = v.(string)
		}
		build.Variables = vars
	}

	req, _ := json.Marshal(build)
	result, _, err := send(client, "POST", route, req)

	if err != nil {
		return fmt.Errorf("Failed to create record: %s", err)
	}

	var task BuildDefinition
	err = json.Unmarshal(result, &task)

	if err != nil {
		return fmt.Errorf("Failed to create record: %s", err)
	}

	if task.Id == "" {
		return fmt.Errorf("Something went wrong. The api did not return an Id")
	}

	d.SetId(task.Id)
	return resourceBuildDefinitionRead(d, meta)
}

func resourceBuildDefinitionRead(d *schema.ResourceData, meta interface{}) error {
	client := meta.(*api_client)
	id := d.Id()

	route := d.Get("read_method").(string)
	if route == "" {
		route = client.read_method
	}

	result, statusCode, err := send(client, "GET", strings.Replace(route, "{id}", id, -1), nil)

	if err != nil {

		if statusCode == 404 {
			log.Printf("resource not found with ID:\n%s\n", id)
			d.SetId("")
			return nil
		}

		return fmt.Errorf(
			"There was a problem when trying to find object with ID: %s", d.Id())
	}

	var task BuildDefinition
	err = json.Unmarshal(result, &task)

	if err != nil {
		return fmt.Errorf("Failed to create record: %s", err)
	}

	checkRevisionTaint(d, task)

	d.Set("path", task.Path)
	d.Set("project", task.Project)
	d.Set("application_name", task.ApplicationName)
	d.Set("build_template", task.BuildTemplate)
	d.Set("queue_pool", task.QueuePool)
	d.Set("repository", task.Repository)
	d.Set("branch", task.Branch)
	d.Set("task_group_revision", task.TaskGroupRevision)
	d.Set("build_revision", task.BuildRevision)
	d.Set("tags", task.Tags)
	d.Set("variable_groups", task.VariableGroups)
	if task.Variables != nil {
		d.Set("variables", task.Variables)
	}
	setCITriggersInSchema(d, task.CITriggers)
	setScheduleTriggersInSchema(d, task.ScheduleTriggers)

	return nil
}

func checkRevisionTaint(d *schema.ResourceData, task BuildDefinition) {

	br := d.Get("build_revision").(string)
	tr := d.Get("task_group_revision").(string)
	taint := false
	if br != "" && br != task.BuildRevision {
		taint = true
	}

	if tr != "" && tr != task.TaskGroupRevision {
		taint = true
	}

	d.Set("revision_taint", taint)

}

func resourceBuildDefinitionUpdate(d *schema.ResourceData, meta interface{}) error {
	client := meta.(*api_client)
	id := d.Id()

	route := d.Get("update_method").(string)
	if route == "" {
		route = client.update_method
	}

	build := &BuildDefinition{
		Path:              d.Get("path").(string),
		Project:           d.Get("project").(string),
		ApplicationName:   d.Get("application_name").(string),
		BuildTemplate:     d.Get("build_template").(string),
		QueuePool:         d.Get("queue_pool").(string),
		Repository:        d.Get("repository").(string),
		Branch:            d.Get("branch").(string),
		TaskGroupRevision: d.Get("task_group_revision").(string),
		BuildRevision:     d.Get("build_revision").(string),
		Tags:              interface2StringList(d.Get("tags").([]interface{})),
		VariableGroups:    interface2StringList(d.Get("variable_groups").([]interface{})),
		CITriggers:        getCITriggersFromSchema(d),
		ScheduleTriggers:  getScheduleTriggersFromSchema(d),
	}

	if value, ok := d.GetOk("variables"); ok {
		vars := make(map[string]string)
		variables := value.(map[string]interface{})
		for k, v := range variables {
			vars[k] = v.(string)
		}
		build.Variables = vars
	}

	req, _ := json.Marshal(build)
	_, _, err := send(client, "PUT", strings.Replace(route, "{id}", id, -1), req)

	if err != nil {
		return fmt.Errorf("Failed to update record: %s", err)
	}
	return resourceBuildDefinitionRead(d, meta)

}

func resourceBuildDefinitionDelete(d *schema.ResourceData, meta interface{}) error {
	client := meta.(*api_client)
	id := d.Id()

	route := d.Get("destroy_method").(string)
	if route == "" {
		route = client.destroy_method
	}
	_, _, err := send(client, "DELETE", strings.Replace(route, "{id}", id, -1), nil)

	if err != nil {
		return err
	}

	return nil
}

func getCITriggersFromSchema(d *schema.ResourceData) []ContinousIntegrationTrigger {
	var options []ContinousIntegrationTrigger
	configuredOptions := d.Get("ci_triggers").([]interface{})
	for _, opt := range configuredOptions {
		data := opt.(map[string]interface{})
		elem := ContinousIntegrationTrigger{
			PathFilter:   interface2StringList(data["path_filter"].([]interface{})),
			BranchFilter: interface2StringList(data["branch_filter"].([]interface{})),
		}
		options = append(options, elem)
	}
	return options
}

func setCITriggersInSchema(d *schema.ResourceData, triggers []ContinousIntegrationTrigger) error {
	var ciList []map[string]interface{}
	for _, trigger := range triggers {
		elem := make(map[string]interface{})
		elem["path_filter"] = trigger.PathFilter
		elem["branch_filter"] = trigger.BranchFilter
		ciList = append(ciList, elem)
	}
	err := d.Set("ci_triggers", ciList)
	return err
}

func getScheduleTriggersFromSchema(d *schema.ResourceData) []ScheduleTrigger {
	triggers := d.Get("schedule_triggers").([]interface{})
	var SList []ScheduleTrigger
	for _, trigger := range triggers {
		data := trigger.(map[string]interface{})
		elem := ScheduleTrigger{
			DayOfTheWeek: data["day_of_the_week"].(int),
			Hours:        data["hours"].(int),
			Minutes:      data["minutes"].(int),
			TimeZone:     data["time_zone"].(string),
			BranchFilter: interface2StringList(data["branch_filter"].([]interface{})),
		}
		SList = append(SList, elem)
	}
	return SList
}

func setScheduleTriggersInSchema(d *schema.ResourceData, triggers []ScheduleTrigger) error {
	var ciList []map[string]interface{}
	for _, trigger := range triggers {
		elem := make(map[string]interface{})
		elem["day_of_the_week"] = trigger.DayOfTheWeek
		elem["hours"] = trigger.Hours
		elem["minutes"] = trigger.Minutes
		elem["time_zone"] = trigger.TimeZone
		elem["branch_filter"] = trigger.BranchFilter
		ciList = append(ciList, elem)
	}
	err := d.Set("schedule_triggers", ciList)
	return err
}
