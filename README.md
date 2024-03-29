## Terraform Azure DevOps classic builds custom provider

This repository contains a provider to create builds on classic mode with Azure DevOps. It creates **opinionated builds** based on a few parameters.   
The provider does not interact directly with Azure DevOps API, it uses a tailor-made NET Core 3.1 WebAPI to create the builds.    

![Diagram](https://github.com/karlospn/tf-azuredevops-pipelines-provisioner/blob/master/docs/diagram.png)


You might be asking why I'm creating an API that acts like a men-in-the-middle when I could be calling the Azure DevOps API directly from the Terraform provider. And that's because some people are afraid of Golang and that's a neat trick to minimize the Go codebase.   

The custom provider delegates almost all the ETL work into the custom WebApi.


### How to create a build?

Here is an example about how to create a build:

```javascript
resource "build_definition_resource" "test" {
	path = "\\Autogenerated"
	project = "projectA"
  	application_name = "its_my_build"
	build_template = "mytg"
	build_template_inputs = {
		"xxx" : "yyyy"
		"zzz" : "ppp"
		"myvar" : "somevar"
	}
	queue_pool = "MyHostedPool"
	repository = "App1"
	branch = "master"
	tags = ["tag1"]
	variable_groups = ["MyGroup"]
	ci_triggers {
		branch_filter = ["+refs/heads/master"]
	}	
}
```

The attributes are the following ones:

- **path**: The folder inside Azure DevOps pipelines where the buid is going to be saved.
- **project**: The Azure DevOps team project.
- **application_project**: Name of the app. It uses the name to create the name of the build.
- **build_template**: You cannot add Tasks directly to the build, you can only reference an existing Task Group. The build_template parameter is the name of the Task Group. The build isn't going to reference the task group directly, instead it will take all the tasks that the task group contains an copy them into our build.
- **build_template_inputs**: Inputs that the task group need to run.
- **queue_pool**: The agent pool where our build is going to run.
- **repository**: Repository name.
- **branch**: Branch name.
- **tags**: Tags to assign to our build
- **variable_groups**: Name of the variable_group that you want to use. It needs to exist previously.
- **ci_triggers**: Continous Integration Triggers.
- **schedule_triggers**: Scheduled Triggers.
- **variables**: Variables that the build might need.





