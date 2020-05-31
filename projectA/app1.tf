data "azuredevops_project" "projectA" {
  project_name = "projectA"
}

resource "azuredevops_git_repository" "App1" {
  project_id = azuredevops_project.projectA.id
  name       = "App1"
  initialization {
    init_type = "Clean"
  }
}

# Doesn't work yet with provider version 0.12
# resource "azuredevops_branch_policy_min_reviewers" "branchPolicyReviewers" {
#   project_id = data.azuredevops_project.projectA.id

#   enabled  = true
#   blocking = true

#   settings {
#     reviewer_count     = 2
#     submitter_can_vote = false

#     scope {
#       repository_id  = azuredevops_git_repository.App1.id
#       repository_ref = azuredevops_git_repository.App1.default_branch
#       match_type     = "Exact"
#     }

#     scope {
#       repository_id  = azuredevops_git_repository.App1.id
#       repository_ref = "refs/heads/releases"
#       match_type     = "Prefix"
#     }
#   }
# }

resource "azuredevops_build_definition" "build" {
  project_id = azuredevops_project.projectA.id
  agent_pool_name = "Hosted Ubuntu 1604"
  name = "Automatic Build Definition"
  path = "\\App1"

  repository {
    repo_name   = "App1"
    repo_type   = "TfsGit"
    repo_id     = azuredevops_git_repository.App1.id
    branch_name = azuredevops_git_repository.App1.default_branch
    yml_path    = "azure-pipelines.yml"
  }
}