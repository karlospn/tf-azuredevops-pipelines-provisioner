using System.Threading.Tasks;
using BuildDefinitionProvider.WebApi.DTO.Builds;
using Microsoft.TeamFoundation.Build.WebApi;

namespace BuildDefinitionProvider.WebApi.Services.Azure.Builds
{
    public interface IVariableGroupService
    {
        Task AddVariableGroups(BuildDefinition definition, CustomBuildDefinitionPayload payload);
    }
}
