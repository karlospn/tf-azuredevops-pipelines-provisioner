using System.Threading.Tasks;
using BuildDefinitionProvider.WebApi.DTO.Builds;
using BuildDefinitionProvider.WebApi.Services.Azure.Client;
using Microsoft.TeamFoundation.Build.WebApi;

namespace BuildDefinitionProvider.WebApi.Services.Azure.Builds
{
    public class TagsService : ITagsService
    {
        private readonly IAzureClient _client;

        public TagsService(IAzureClient client)
        {
            _client = client;
        }

        public async Task AddTags(BuildDefinition definition,
            CustomBuildDefinitionPayload payload)
        {
            var client = await _client.GetBuildClientAsync();
            await client.AddDefinitionTagsAsync(payload.Tags, payload.Project, definition.Id);
        }


        public async Task UpdateTags(BuildDefinition definition,
            CustomBuildDefinitionPayload payload)
        {
            var client = await _client.GetBuildClientAsync();
            var tags = await client.GetDefinitionTagsAsync(payload.Project, definition.Id);
            foreach (var tag in tags)
            {
                await client.DeleteDefinitionTagAsync(payload.Project, definition.Id, tag);
            }
            await client.AddDefinitionTagsAsync(payload.Tags, payload.Project, definition.Id);
        }



    }
}
