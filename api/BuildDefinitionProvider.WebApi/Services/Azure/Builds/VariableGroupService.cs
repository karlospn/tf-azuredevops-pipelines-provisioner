using System;
using System.Linq;
using System.Threading.Tasks;
using BuildDefinitionProvider.WebApi.DTO.Builds;
using BuildDefinitionProvider.WebApi.Services.Azure.Client;
using Microsoft.TeamFoundation.Build.WebApi;

namespace BuildDefinitionProvider.WebApi.Services.Azure.Builds
{
    public class VariableGroupService : IVariableGroupService
    {
        private readonly IAzureClient _client;

        public VariableGroupService(IAzureClient client)
        {
            _client = client;
        }

        public async Task AddVariableGroups(BuildDefinition definition, 
            CustomBuildDefinitionPayload payload)
        {
            if (payload.VariableGroups == null ||
                payload.VariableGroups.Length == 0)
            {
                return;
            }

            var client = await _client.GetTaskAgentAsync();

            foreach (var variableGroup in payload.VariableGroups)
            {
                var groups = await client.GetVariableGroupsAsync(project: payload.Project, groupName: variableGroup);

                if (groups == null ||
                    !groups.Any())
                {
                    throw new Exception($"Could not find any variablegroup with name: {variableGroup}");
                }

                if (groups.Count() > 1)
                {
                    throw new Exception($"There are more thant one variable group in the same project ({payload.Project}) with the same : {variableGroup}");
                }

                var group = groups.FirstOrDefault();
                
                if (group != null)
                {
                    var g = new VariableGroup
                    {
                        Name = group.Name,
                        Description = group.Description,
                        Id = group.Id,
                        Type = group.Type
                    };

                    foreach (var (key, value) in group.Variables)
                    {
                        g.Variables[key] = new BuildDefinitionVariable
                        {
                            Value = value.Value,
                            IsSecret = value.IsSecret
                        };
                    }
                    definition.VariableGroups.Add(g);

                }

            }


        }
    }
}
