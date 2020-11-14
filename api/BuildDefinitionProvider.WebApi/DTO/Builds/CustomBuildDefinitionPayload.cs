using System.Collections.Generic;

namespace BuildDefinitionProvider.WebApi.DTO.Builds
{
    public class CustomBuildDefinitionPayload
    {
        public string Path { get; set; }
        public string Project { get; set; }
        public string ApplicationName { get; set; }
        public string BuildTemplate { get; set; }
        public string QueuePool { get; set; }
        public string Repository { get; set; }
        public string Branch { get; set; }
        public string[] Tags { get; set; }
        public string[] VariableGroups { get; set; }
        public Dictionary<string, string> Variables { get; set; }
        public List<CITriggers> CITriggers { get; set; }
        public List<ScheduleTriggers> ScheduleTriggers { get; set; }
        public string TaskGroupRevision { get; set; }
        public string BuildRevision { get; set; }
    }

    public class CITriggers
    {
        public List<string> PathFilter { get; set; }
        public List<string> BranchFilter { get; set; }
    }

    public class ScheduleTriggers
    {
        public int DayOfTheWeek { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public string TimeZone { get; set; }
        public List<string> BranchFilter { get; set; }
    }


}