using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildDefinitionProvider.WebApi.DTO.Builds;
using BuildDefinitionProvider.WebApi.Exceptions;
using BuildDefinitionProvider.WebApi.Services.Azure.Client;
using Microsoft.TeamFoundation.Build.WebApi;
using TaskAgentPoolReference = Microsoft.TeamFoundation.Build.WebApi.TaskAgentPoolReference;

namespace BuildDefinitionProvider.WebApi.Services.Azure.Builds
{
    public class BuildDefinitionService : IBuildDefinitionService
    {
        private readonly IAzureClient _azureClient;
        private readonly IVariableGroupService _variableGroupService;
        private readonly ITaskGroupService _taskGroupService;
        private readonly IVariableService _variableService;
        private readonly ITriggersService _triggersService;
        private readonly ITagsService _tagsService;

        public BuildDefinitionService(IAzureClient azureClient, 
            IVariableGroupService variableGroupService,
            ITaskGroupService taskGroupService,
            IVariableService variableService, 
            ITriggersService triggersService, 
            ITagsService tagsService)
        {
            _azureClient = azureClient;
            _variableGroupService = variableGroupService;
            _taskGroupService = taskGroupService;
            _variableService = variableService;
            _triggersService = triggersService;
            _tagsService = tagsService;
        }


        public async Task<CustomBuildDefinitionPayload> GetAsync(
            string id)
        {
            try
            {
                var project = getProject(id);
                var buildId = getAdosId(id);

                var client = await _azureClient.GetBuildClientAsync();

                var definitions = await client
                    .GetFullDefinitionsAsync(project: project,
                        definitionIds: new List<int> { Convert.ToInt32(buildId) });



                if (definitions.Count > 1)
                {
                    throw new Exception("Error: Found more thant one build with the same id");
                }

                if (definitions.Count == 0)
                {
                    throw new NotFoundException($"Error: Build wit ID: {buildId} not found");
                }

                var result = definitions.FirstOrDefault();

                if (result != null)
                {
                    var appName = result.Name.Split('-');
                    var name = appName.Length == 0 ? $"Modified+{id}" : appName[0];

                    var definition = new CustomBuildDefinitionPayload
                    {
                        ApplicationName = name,
                        Branch = result.Repository.DefaultBranch,
                        Repository = result.Repository.Name,
                        QueuePool = result.Queue.Name,
                        BuildRevision = result.Revision.ToString(),
                        Path = result.Path,
                        Project = result.Project.Name,
                        Tags = result.Tags.ToArray(),
                        VariableGroups = result.VariableGroups.Select(x => x.Name).ToArray(),
                        Variables = _variableService.GetVariables(result),
                        CITriggers = new List<CITriggers>{_triggersService.GetCITriggers(result)},
                        ScheduleTriggers = _triggersService.GetScheduleTriggers(result)
                    };
                    await _taskGroupService.GetTaskGroup(result, definition);
                    return definition;

                }
                return null;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new Exception("Error: Something went wrong when calling the AzureDevOps API", e);
            }
          
        }


        public async Task<string> PostAsync(
            CustomBuildDefinitionPayload payload)
        {
            try
            {

                var client = await _azureClient.GetBuildClientAsync();

                var definition = new BuildDefinition
                {
                    Name = $"{payload.ApplicationName}-{payload.BuildTemplate}-{payload.Branch}" ,
                    Path = payload.Path,
                    Repository = new BuildRepository
                    {
                        DefaultBranch = payload.Branch,
                        Name = payload.Repository,
                        Type = "TfsGit",
                    },
                    Queue = new AgentPoolQueue
                    {
                        Name = payload.QueuePool,
                        Pool = new TaskAgentPoolReference
                        {
                            Name = payload.QueuePool
                        }
                    }

                };
                
                await _variableGroupService.AddVariableGroups(definition, payload);
                await _taskGroupService.AddTaskGroupSteps(definition, payload);
                _variableService.AddVariables(definition, payload);
                _triggersService.AddTriggers(definition, payload);

                var result = await client
                    .CreateDefinitionAsync(definition, payload.Project);

                await _tagsService.AddTags(result, payload);

                return $"{payload.Project}@@@{result.Id.ToString()}";
            }
            catch (Exception e)
            {
                throw new Exception(
                    "Error: Something went wrong when calling the AzureDevOps API", e);
            }

        }

        public async Task PutAsync(
            string id,
            CustomBuildDefinitionPayload payload)
        {
            try
            {
                var project = getProject(id);
                var buildId = getAdosId(id);
                var client = await _azureClient.GetBuildClientAsync();

                var definitions = await client
                    .GetFullDefinitionsAsync(project: project, definitionIds: new List<int> { Convert.ToInt32(buildId) });

                if (definitions is null ||
                    definitions.Count > 1 ||
                    definitions.Count == 0)
                {
                    throw new Exception("Error: Cannot have more than 1 build with the same name on the same project");
                }

                var df = definitions.FirstOrDefault();

                var definition = new BuildDefinition
                {
                    Id = Convert.ToInt32(buildId),
                    Revision = Convert.ToInt32(payload.BuildRevision),
                    Name = $"{ payload.ApplicationName }-{ payload.BuildTemplate }-{ payload.Branch }",
                    Path = payload.Path,
                    Repository = new BuildRepository
                    {
                        DefaultBranch = payload.Branch,
                        Name = payload.Repository,
                        Type = "TfsGit",
                    },
                    Queue = new AgentPoolQueue
                    {
                        Name = payload.QueuePool,
                        Pool = new TaskAgentPoolReference
                        {
                            Name = payload.QueuePool
                        }
                    },
                };

                await _tagsService.UpdateTags(df, payload);
                await _variableGroupService.AddVariableGroups(definition, payload);
                _variableService.AddVariables(definition, payload);
                await _taskGroupService.AddTaskGroupSteps(definition, payload);
                _triggersService.AddTriggers(definition, payload);

                await client.UpdateDefinitionAsync(
                    definition: definition, 
                    project: project, 
                    definitionId: definition.Id);

            }
            catch (Exception e)
            {
                throw new Exception(
                    "Error: Something went wrong when calling the AzureDevOps API", e);
            }

        }

        public async Task DeleteAsync(
            string id)
        {
            try
            {
                var project = getProject(id);
                var buildId = getAdosId(id);
                var client = await _azureClient.GetBuildClientAsync();

                var definitions = await client
                    .GetFullDefinitionsAsync(project: project,
                        definitionIds: new List<int> { Convert.ToInt32(buildId) });


                if (definitions is null ||
                    definitions.Count > 1 ||
                    definitions.Count == 0)
                {
                    throw new Exception("Error: Cannot have more than 1 build with the same name on the same project");
                }

                var result = definitions.FirstOrDefault();

                if (result != null)
                {
                    await client.DeleteDefinitionAsync(project, result.Id);
                }
            }
            catch (Exception e)
            {
                throw new Exception(
                    "Error: Something went wrong when calling the AzureDevOps API", e);
            }
        
        }


        private string getProject(string id)
        {
            return id.Split("@@@")[0];
        }

        private string getAdosId(string id)
        {
            return id.Split("@@@")[1];
        }
    }


}
