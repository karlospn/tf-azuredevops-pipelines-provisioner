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
# resource "azuredevops_branch_policy_min_reviewers" "branchPolicy" {
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