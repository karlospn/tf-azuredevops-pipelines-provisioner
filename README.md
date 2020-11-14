## Terraform Azure DevOps classic builds custom provider

This repository contains a provider to create builds on classic mode with Azure DevOps. It creates **opinionated builds** based on a few parameters.   
The provider does not interact directly with Azure DevOps API, it uses a tailor-made NET Core 3.1 WebAPI to create the builds.    

![Diagram](https://github.com/karlospn/tf-azuredevops-pipelines-provisioner/blob/master/docs/diagram.png)


You might be asking why I'm creating an API that acts like a men-in-the-middle when I could be calling the Azure DevOps API directly from the Terraform provider. And that's because some people are afraid of Golang and that's a neat trick to minimize the Go codebase.   

The custom provider delegates almost all the ETL work into the custom WebApi.
