using System.Threading.Tasks;
using BuildDefinitionProvider.WebApi.DTO.Builds;
using Microsoft.TeamFoundation.Build.WebApi;

namespace BuildDefinitionProvider.WebApi.Services.Azure.Builds
{
    public interface ITagsService
    {
        Task AddTags(BuildDefinition definition,
            CustomBuildDefinitionPayload payload);

        Task UpdateTags(BuildDefinition definition,
            CustomBuildDefinitionPayload payload);
    }
}