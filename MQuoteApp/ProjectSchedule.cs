using System;
using System.Collections.Generic;

namespace MQuoteApp
{
    public class ProjectSchedule : ProjectItem
    { 
        public string TaskDescription { get; set; }
        public List<string> TaskDependencies { get; set; }
        public bool IsCriticalTask { get; set; }
        public List<ScheduleItem> ScheduleItems { get; set; }

        public ProjectSchedule(DateTime startDate, DateTime finishDate)
            : base(startDate, finishDate)
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
            return (FinishDate - StartDate).Days;
        }

        public void UpdateCriticalTaskStatus(bool isCritical)
        {
            IsCriticalTask = isCritical;
        }
    }
    public class ScheduleItem
    {
        public ScheduleItem(int duration)
        {
            Duration = duration;
        }

        public string ItemName { get; set; }
        public int Duration { get; set; } // 工期（日数）
    }
}

