using System.Collections.Generic;
using BuildDefinitionProvider.WebApi.DTO.Builds;
using Microsoft.TeamFoundation.Build.WebApi;

namespace BuildDefinitionProvider.WebApi.Services.Azure.Builds
{
    public class VariableService : IVariableService
    {

        public void AddVariables(BuildDefinition definition, 
            CustomBuildDefinitionPayload payload)
        {
            if (payload.Variables == null)
            {
                return;
            }

            foreach (var (key, value) in payload.Variables)
            {
                definition.Variables[key] = new BuildDefinitionVariable
                {
                    Value = value,
                    IsSecret = false,
                };
            }
        }

        public Dictionary<string, string> GetVariables(BuildDefinition definition)
        {
            var result = new Dictionary<string, string>();

            foreach(var v in definition.Variables)
            {
                result[v.Key] = v.Value.Value;
            }

            return result;
        }
    }
}
