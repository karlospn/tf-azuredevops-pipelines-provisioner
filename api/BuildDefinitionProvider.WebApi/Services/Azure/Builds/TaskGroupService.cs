using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildDefinitionProvider.WebApi.DTO.Builds;
using BuildDefinitionProvider.WebApi.Services.Azure.Client;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.DistributedTask.WebApi;

namespace BuildDefinitionProvider.WebApi.Services.Azure.Builds
{
    public class TaskGroupService : ITaskGroupService
    {
        private readonly IAzureClient _azureClient;
        //Hardcode provisioning project
        private Guid ProvisioningProject = new Guid("e7a50b7f-ad7d-42f8-9c8f-270fe3c68100");

        public TaskGroupService(IAzureClient azureClient )
        {
            _azureClient = azureClient;
        }


        public async Task AddTaskGroupSteps(
            BuildDefinition definition, 
            CustomBuildDefinitionPayload payload)
        {
            var client = await _azureClient.GetTaskAgentAsync();
            var taskGroups = await client.GetTaskGroupsAsync(ProvisioningProject);

            var amount = taskGroups.Count(x =>
                x.Name.Equals(payload.BuildTemplate, StringComparison.CurrentCultureIgnoreCase));

            if (amount > 1)
            {
                throw new Exception($"There are more than one TaskGroup with the name {payload.BuildTemplate} in the provisioning repository");
            }

            if (amount == 0)
            {
                throw new Exception($"There is no TaskGroup with the name {payload.BuildTemplate} in the provisioning repository");
            }

            var tg = taskGroups.FirstOrDefault(x =>
                x.Name.Equals(payload.BuildTemplate, StringComparison.InvariantCultureIgnoreCase));

            if (tg != null)
            {
                definition.Process = new DesignerProcess
                {
                    Phases =
                    {
                        new Phase
                        {
                            Name = "Agent Job 1",
                            RefName = "Job_1",
                            Condition = "succeeded()",
                            Steps = AddTasks(tg)
                        }
                    }
                };

                definition.Properties.Add("tg_name", payload.BuildTemplate);

            }

        }

        private List<BuildDefinitionStep> AddTasks(TaskGroup tg)
        {
            var steps = new List<BuildDefinitionStep>();

            foreach (var t in tg.Tasks)
            {
                var step = new BuildDefinitionStep
                {
                    TimeoutInMinutes = t.TimeoutInMinutes,
                    Condition = t.Condition,
                    DisplayName = t.DisplayName,
                    Environment = t.Environment,
                    AlwaysRun = t.AlwaysRun,
                    ContinueOnError = t.ContinueOnError,
                    Enabled = t.Enabled,
                    Inputs = t.Inputs,
                    TaskDefinition = new Microsoft.TeamFoundation.Build.WebApi.TaskDefinitionReference
                    {
                        DefinitionType = t.Task.DefinitionType,
                        Id = t.Task.Id,
                        VersionSpec = t.Task.VersionSpec
                    }
                    
                };
                steps.Add(step);
            }

            return steps;
        }


        public async Task GetTaskGroup(BuildDefinition definition,
            CustomBuildDefinitionPayload payload)
        {
            var buildClient = await _azureClient.GetBuildClientAsync();
            var tgClient = await _azureClient.GetTaskAgentAsync();

            var taskGroups = await tgClient.GetTaskGroupsAsync(ProvisioningProject);


            var props = await buildClient.GetDefinitionPropertiesAsync(project: payload.Project,  definition.Id );

            if (props == null ||
                !props.ContainsKey("tg_name"))
            {
                throw new Exception("Build definition does not have the metadata necessary");
            }

            var tgName = props["tg_name"] as string;
            var amount = taskGroups.Count(x =>
                x.Name.Equals(tgName, StringComparison.CurrentCultureIgnoreCase));

            if (amount > 1)
            {
                throw new Exception($"There are more than one TaskGroup with the name {tgName} in the provisioning repository");
            }

            if (amount == 0)
            {
                throw new Exception($"There is no TaskGroup with the name {tgName} in the provisioning repository");
            }

            var tg = taskGroups.FirstOrDefault(x =>
                x.Name.Equals(tgName, StringComparison.InvariantCultureIgnoreCase));

            if (tg != null)
            {
                payload.BuildTemplate = tg.Name;
                payload.TaskGroupRevision = tg.Revision.ToString();
            }
        }
    }
}
