using System;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.WebApi;

namespace BuildDefinitionProvider.WebApi.Services.Azure.Client
{
    public class AzureClient  : IAzureClient
    {
        private readonly Uri _baseUrl;
        private readonly string _personalAccessToken;
        private VssConnection _connection;


        public AzureClient(string baseUrl,
            string personalAccessToken)
        {
            _personalAccessToken = personalAccessToken;
            _baseUrl = new Uri(baseUrl);
        }

        public async Task<ReleaseHttpClient2> GetReleaseClientAsync()
        {
            return await GetConnection()
                .GetClientAsync<ReleaseHttpClient2>();

        }



        public async Task<BuildHttpClient> GetBuildClientAsync()
        {
            return await GetConnection()
                .GetClientAsync<BuildHttpClient>();
        }


        public async Task<TaskAgentHttpClient> GetTaskAgentAsync()
        {
            return await GetConnection()
                .GetClientAsync<TaskAgentHttpClient>();
        }



        public VssConnection GetConnection()
        {
            if (_connection == null)
            {
                var credentials = new VssBasicCredential(string.Empty, _personalAccessToken);
                _connection = new VssConnection(_baseUrl, credentials);
            }
            return _connection;
        }


    }
}
