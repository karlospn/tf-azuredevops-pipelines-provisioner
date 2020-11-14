package builds

import "net/http"

type api_client struct {
	http_client    *http.Client
	insecure       bool
	uri            string
	username       string
	password       string
	timeout        int
	create_method  string
	read_method    string
	update_method  string
	destroy_method string
	use_cookies    string
}

type ContinousIntegrationTrigger struct {
	PathFilter   []string
	BranchFilter []string
}

type ScheduleTrigger struct {
	DayOfTheWeek int
	Hours        int
	Minutes      int
	TimeZone     string
	BranchFilter []string
}

type BuildDefinition struct {
	Id                string
	Path              string
	Project           string
	ApplicationName   string
	BuildTemplate     string
	QueuePool         string
	Repository        string
	Branch            string
	Tags              []string
	VariableGroups    []string
	Variables         map[string]string
	TaskGroupRevision string
	BuildRevision     string
	CITriggers        []ContinousIntegrationTrigger
	ScheduleTriggers  []ScheduleTrigger
}
