package builds

import (
	"crypto/tls"
	"net/http"
	"net/http/cookiejar"
	"time"

	"github.com/hashicorp/terraform-plugin-sdk/helper/schema"
	"github.com/hashicorp/terraform-plugin-sdk/terraform"
)

// Provider returns the terraform.ResourceProvider structure for the generic
// provider.
func Provider() terraform.ResourceProvider {
	return &schema.Provider{
		Schema: map[string]*schema.Schema{
			"uri": &schema.Schema{
				Type:        schema.TypeString,
				Required:    true,
				DefaultFunc: schema.EnvDefaultFunc("REST_API_URI", nil),
				Description: "URI of the REST API endpoint. This serves as the base of all requests.",
			},
			"username": &schema.Schema{
				Type:        schema.TypeString,
				Optional:    true,
				DefaultFunc: schema.EnvDefaultFunc("REST_API_USERNAME", nil),
				Description: "When set, will use this username for BASIC auth to the API.",
			},
			"password": &schema.Schema{
				Type:        schema.TypeString,
				Optional:    true,
				DefaultFunc: schema.EnvDefaultFunc("REST_API_PASSWORD", nil),
				Description: "When set, will use this password for BASIC auth to the API.",
			},
			"timeout": &schema.Schema{
				Type:        schema.TypeInt,
				Optional:    true,
				DefaultFunc: schema.EnvDefaultFunc("REST_API_TIMEOUT", 0),
				Description: "When set, will cause requests taking longer than this time (in seconds) to be aborted.",
			},
			"create_method": &schema.Schema{
				Type:        schema.TypeString,
				DefaultFunc: schema.EnvDefaultFunc("REST_API_CREATE_METHOD", nil),
				Description: "The HTTP route used to CREATE objects of this type on the API server.",
				Required:    true,
			},
			"read_method": &schema.Schema{
				Type:        schema.TypeString,
				Description: "The HTTP route used to READ objects of this type on the API server.",
				Required:    true,
				DefaultFunc: schema.EnvDefaultFunc("REST_API_READ_METHOD", nil),
			},
			"update_method": &schema.Schema{
				Type:        schema.TypeString,
				DefaultFunc: schema.EnvDefaultFunc("REST_API_UPDATE_METHOD", nil),
				Description: "The HTTP route used to UPDATE objects of this type on the API server.",
				Required:    true,
			},
			"destroy_method": &schema.Schema{
				Type:        schema.TypeString,
				DefaultFunc: schema.EnvDefaultFunc("REST_API_DESTROY_METHOD", nil),
				Description: "The HTTP route used to DELETE objects of this type on the API server.",
				Required:    true,
			},
			"insecure": &schema.Schema{
				Type:        schema.TypeBool,
				Optional:    true,
				DefaultFunc: schema.EnvDefaultFunc("REST_API_INSECURE", nil),
				Description: "When using https, this disables TLS verification of the host.",
			},
			"use_cookies": &schema.Schema{
				Type:        schema.TypeBool,
				Optional:    true,
				DefaultFunc: schema.EnvDefaultFunc("REST_API_USE_COOKIES", nil),
				Description: "Enable cookie jar to persist session.",
			},
		},
		ResourcesMap: map[string]*schema.Resource{
			"build_definition_resource": resourceBuildDefinition(),
		},
		ConfigureFunc: configureProvider,
	}
}

func configureProvider(d *schema.ResourceData) (interface{}, error) {

	timeout := d.Get("timeout").(int)
	insecure := d.Get("insecure").(bool)
	use_cookies := d.Get("use_cookies").(bool)

	tr := &http.Transport{
		TLSClientConfig: &tls.Config{InsecureSkipVerify: insecure},
		Proxy:           http.ProxyFromEnvironment,
	}

	var cookieJar http.CookieJar

	if use_cookies {
		cookieJar, _ = cookiejar.New(nil)
	}

	return &api_client{
		http_client: &http.Client{
			Timeout:   time.Second * time.Duration(timeout),
			Transport: tr,
			Jar:       cookieJar,
		},
		uri:            d.Get("uri").(string),
		username:       d.Get("username").(string),
		password:       d.Get("username").(string),
		timeout:        d.Get("timeout").(int),
		create_method:  d.Get("create_method").(string),
		read_method:    d.Get("read_method").(string),
		update_method:  d.Get("update_method").(string),
		destroy_method: d.Get("destroy_method").(string),
	}, nil
}
