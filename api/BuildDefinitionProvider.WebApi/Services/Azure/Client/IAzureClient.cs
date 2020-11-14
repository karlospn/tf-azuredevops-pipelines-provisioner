using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;

namespace BuildDefinitionProvider.WebApi.Services.Azure.Client
{
    public interface IAzureClient
    {
        Task<ReleaseHttpClient2> GetReleaseClientAsync();
        Task<BuildHttpClient> GetBuildClientAsync();
        Task<TaskAgentHttpClient> GetTaskAgentAsync();
    }
}
