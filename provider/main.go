package main

import (
	"github.com/hashicorp/terraform-plugin-sdk/plugin"
	"vueling.com/devops/builds"
)

func main() {

	plugin.Serve(&plugin.ServeOpts{
		ProviderFunc: builds.Provider})

}
