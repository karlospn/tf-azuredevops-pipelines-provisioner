using System.Collections.Generic;
using BuildDefinitionProvider.WebApi.DTO.Builds;
using Microsoft.TeamFoundation.Build.WebApi;

namespace BuildDefinitionProvider.WebApi.Services.Azure.Builds
{
    public interface IVariableService
    {
        void AddVariables(BuildDefinition definition, CustomBuildDefinitionPayload payload);
        Dictionary<string, string> GetVariables(BuildDefinition definition);
    }
}
