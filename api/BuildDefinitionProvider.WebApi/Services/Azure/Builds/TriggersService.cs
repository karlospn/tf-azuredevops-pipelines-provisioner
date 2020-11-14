using System;
using System.Collections.Generic;
using BuildDefinitionProvider.WebApi.DTO.Builds;
using Microsoft.TeamFoundation.Build.WebApi;

namespace BuildDefinitionProvider.WebApi.Services.Azure.Builds
{
    public class TriggersService : ITriggersService
    {
        public void  AddTriggers(
            BuildDefinition definition, 
            CustomBuildDefinitionPayload payload)
        {
            if(payload.CITriggers != null)
            {
                foreach (var trg in payload.CITriggers)
                {
                    var trigger = new ContinuousIntegrationTrigger();
                    trigger.BranchFilters.AddRange(trg.BranchFilter);
                    trigger.PathFilters.AddRange(trg.PathFilter);
                    trigger.SettingsSourceType = 1;
                    trigger.BatchChanges = false;
                    trigger.MaxConcurrentBuildsPerBranch = 1;
                    trigger.PollingInterval = 0;
                    
                    definition.Triggers.Add(trigger);
                }

            }


            if (payload.ScheduleTriggers != null)
            {
                var schTriggers = new ScheduleTrigger { Schedules = new List<Schedule>() };

                foreach (var trg in payload.ScheduleTriggers)
                {

                    var sch = new Schedule();

                    sch.BranchFilters.AddRange(trg.BranchFilter);
                    sch.StartHours = trg.Hours;
                    sch.StartMinutes = trg.Minutes;
                    sch.TimeZoneId = trg.TimeZone;
                    sch.DaysToBuild = GetScheduleDay(trg.DayOfTheWeek);
                    schTriggers.Schedules.Add(sch);

                }

                definition.Triggers.Add(schTriggers);
            }
           

        }

        public CITriggers GetCITriggers(BuildDefinition definition)
        {
            var result = new List<CITriggers>();

            foreach (var trigger in definition.Triggers)
            {
                if (trigger is ContinuousIntegrationTrigger continuousIntegrationTrigger)
                {
                    var t = new CITriggers
                    {
                        BranchFilter = continuousIntegrationTrigger.BranchFilters,
                        PathFilter = continuousIntegrationTrigger.PathFilters
                    };

                    return t;
                }
            }

            return null;
        }

        public List<ScheduleTriggers> GetScheduleTriggers(BuildDefinition definition)
        {
            var result = new List<ScheduleTriggers>();

            foreach (var trigger in definition.Triggers)
            {
                if (trigger is ScheduleTrigger scheduleTrigger)
                {
                    foreach (var schedule in scheduleTrigger.Schedules)
                    {
                        var t = new ScheduleTriggers
                        {
                            BranchFilter = schedule.BranchFilters,
                            Hours = schedule.StartHours,
                            Minutes = schedule.StartMinutes,
                            TimeZone = schedule.TimeZoneId,
                            DayOfTheWeek = GetStringFromScheduleDay(schedule.DaysToBuild)
                        };

                        result.Add(t);
                    }

                }
            }

            return result;
        }

        private int GetStringFromScheduleDay(ScheduleDays day)
        {
            if (day == ScheduleDays.Monday) return 1;
            if (day == ScheduleDays.Tuesday) return 2;
            if (day == ScheduleDays.Wednesday) return 3;
            if (day == ScheduleDays.Thursday) return 4;
            if (day == ScheduleDays.Friday) return 5;
            if (day == ScheduleDays.Saturday) return 6;
            if (day == ScheduleDays.Sunday) return 7;

            throw new Exception("Day on schedule trigger out of range");
        }

        private ScheduleDays GetScheduleDay(int day)
        {
            if (day == 1) return ScheduleDays.Monday;
            if (day == 2) return ScheduleDays.Tuesday;
            if (day == 3) return ScheduleDays.Wednesday;
            if (day == 4) return ScheduleDays.Thursday;
            if (day == 5) return ScheduleDays.Friday;
            if (day == 6) return ScheduleDays.Saturday;
            if (day == 7) return ScheduleDays.Saturday;

            throw new Exception("Day index on schedule trigger out of range");

        }
    }
}
