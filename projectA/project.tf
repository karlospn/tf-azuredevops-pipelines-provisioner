resource "azuredevops_project" "projectA" {
  project_name       = "projectA"
  description        = "projectA Description"
  visibility         = "private"
  version_control    = "Git"
  work_item_template = "Agile"
}
