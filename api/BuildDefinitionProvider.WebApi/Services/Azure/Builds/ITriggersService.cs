using System.Collections.Generic;
using BuildDefinitionProvider.WebApi.DTO.Builds;
using Microsoft.TeamFoundation.Build.WebApi;

namespace BuildDefinitionProvider.WebApi.Services.Azure.Builds
{
    public interface ITriggersService
    {
        void AddTriggers(
            BuildDefinition definition,
            CustomBuildDefinitionPayload payload);
        CITriggers GetCITriggers(BuildDefinition definition);
        List<ScheduleTriggers> GetScheduleTriggers(BuildDefinition definition);
    }
}