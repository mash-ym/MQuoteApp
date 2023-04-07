using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MQuoteApp
{
    public class Project
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<EstimateItem> EstimateItems { get; set; }

        public Project(string name, DateTime startDate, DateTime endDate, List<EstimateItem> estimateItems)
        {
            Name = name;
            StartDate = startDate;
            EndDate = endDate;
            EstimateItems = estimateItems;
        }

        public decimal GetTotalCost()
        {
            decimal total = 0;
            foreach (var item in EstimateItems)
            {
                total += item.GetTotalCost() * (decimal)GetWorkingDays() / (decimal)item.WorkingDays;
            }
            return total;
        }

        public int GetWorkingDays()
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

        public string GanttChart()
        {
            StringBuilder chart = new StringBuilder();

            int projectDuration = GetDuration();

            chart.Append("Name\tStart Date\tEnd Date\tDuration (days)\n");

            foreach (var item in EstimateItems)
            {
                int itemDuration = (int)Math.Ceiling((double)item.EstimatedAmount / (double)item.Amount);
                int itemStartDateOffset = GetWorkingDays(item.StartDate, StartDate);

                chart.AppendFormat("{0}\t{1}\t{2}\t{3}\n", item.Name, item.StartDate.ToShortDateString(),
                                     item.StartDate.AddDays(itemDuration).ToShortDateString(), itemDuration);

                if (item.SubItems != null && item.SubItems.Any())
                {
                    foreach (var subItem in item.SubItems)
                    {
                        int subItemDuration = (int)Math.Ceiling((double)subItem.EstimatedAmount / (double)subItem.Amount);
                        int subItemStartDateOffset = GetWorkingDays(subItem.StartDate, StartDate);

                        chart.AppendFormat("- {0}\t{1}\t{2}\t{3}\n", subItem.Name,
                                             subItem.StartDate.ToShortDateString(),
                                             subItem.StartDate.AddDays(subItemDuration).ToShortDateString(), subItemDuration);
                    }
                }
            }

            return chart.ToString();
        }

        private int GetDuration()
        {
            return GetWorkingDays(StartDate, EndDate);
        }

        private int GetWorkingDays(DateTime startDate, DateTime endDate)
        {
            int days = 0;
            DateTime currentDay = startDate;
            while (currentDay <= endDate)
            {
                if (currentDay.DayOfWeek != DayOfWeek.Saturday && currentDay.DayOfWeek != DayOfWeek.Sunday)
                {
                    days++;
                }
                currentDay = currentDay.AddDays(1);
            }
            return days;
        }
    }

}

