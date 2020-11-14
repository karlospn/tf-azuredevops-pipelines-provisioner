using System.Threading.Tasks;
using BuildDefinitionProvider.WebApi.DTO.Builds;
using Microsoft.TeamFoundation.Build.WebApi;

namespace BuildDefinitionProvider.WebApi.Services.Azure.Builds
{
    public interface ITaskGroupService
    {
        Task AddTaskGroupSteps(BuildDefinition definition, 
            CustomBuildDefinitionPayload payload);

        Task GetTaskGroup(BuildDefinition definition,
            CustomBuildDefinitionPayload payload);
    }
}
