using System;
using System.Collections.Generic;

namespace MQuoteApp
{
    public abstract class ProjectItem
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<ProjectItem> SubItems { get; set; }
        public int WorkingDays => GetWorkingDays();

        protected ProjectItem(string name, DateTime startDate, DateTime endDate)
        {
            Name = name;
            StartDate = startDate;
            EndDate = endDate;
        }

        protected ProjectItem(DateTime startDate, DateTime endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
        }

        protected int GetWorkingDays()
        {
            int days = 0;
            DateTime currentDay = StartDate;
            while (currentDay <= EndDate)
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
