using System;
using System.Collections.Generic;

namespace MQuoteApp
{
    public class ProjectSchedule : ProjectItem
    {
        public string TaskName { get; set; }
        public string TaskDescription { get; set; }
        public List<string> TaskDependencies { get; set; }
        public bool IsCriticalTask { get; set; }

        public ProjectSchedule(DateTime startDate, DateTime endDate)
            : base(startDate, endDate)
        {
            TaskDependencies = new List<string>();
            IsCriticalTask = false;
        }

        public void AddTaskDependency(string taskName)
        {
            if (!TaskDependencies.Contains(taskName))
            {
                TaskDependencies.Add(taskName);
            }
        }

        public void RemoveTaskDependency(string taskName)
        {
            if (TaskDependencies.Contains(taskName))
            {
                TaskDependencies.Remove(taskName);
            }
        }

        public int GetDurationInDays()
        {
            return (EndDate - StartDate).Days;
        }

        public void UpdateCriticalTaskStatus(bool isCritical)
        {
            IsCriticalTask = isCritical;
        }
    }
}

