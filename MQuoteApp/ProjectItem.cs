using System;
using System.Collections.Generic;

namespace MQuoteApp
{
    public abstract class ProjectItem
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
        public List<ProjectItem> SubItems { get; set; }
        public int WorkingDays => GetWorkingDays();
        public int? Duration { get; set; } // 工事期間（日数

        protected ProjectItem(string name, DateTime startDate, DateTime finishDate, int? duration)
        {
            Name = name;
            StartDate = startDate;
            FinishDate = finishDate;
            Duration = duration;
        }

        protected ProjectItem(DateTime startDate, DateTime finishDate)
        {
            StartDate = startDate;
            FinishDate = finishDate;
        }

        protected int GetWorkingDays()
        {
            int days = 0;
            DateTime currentDay = (DateTime)StartDate;
            while (currentDay <= FinishDate)
            {
                if (currentDay.DayOfWeek != DayOfWeek.Saturday && currentDay.DayOfWeek != DayOfWeek.Sunday)
                {
                    days++;
                }
                currentDay = currentDay.AddDays(1);
            }
            return days;
        }

        public virtual decimal GetTotalCost()
        {
            return 0m;
        }
    }
}
