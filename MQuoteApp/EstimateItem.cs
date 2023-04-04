using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MQuoteApp
{

    public class EstimateItem
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public string Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public string Remarks { get; set; }
        public decimal SubcontractorAmount { get; set; }
        public decimal EstimatedAmount { get; set; }
        public List<EstimateItem> SubItems { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public EstimateItem(string name, decimal unitPrice, decimal amount, decimal subcontractorAmount, decimal estimatedAmount)
        {
            Name = name;
            UnitPrice = unitPrice;
            Amount = amount;
            SubcontractorAmount = subcontractorAmount;
            EstimatedAmount = estimatedAmount;
            SubItems = new List<EstimateItem>();
        }

        public class EstimateSubcontractor
        {
            public string Name { get; set; }
            public decimal Amount { get; set; }
            public string Unit { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal SubcontractorAmount { get; set; }
            public decimal EstimatedAmount { get; set; }
            public List<EstimateSubcontractor> SubItems { get; set; }

            public EstimateSubcontractor(string name, decimal unitPrice, decimal amount, decimal subcontractorAmount, decimal estimatedAmount)
            {
                Name = name;
                UnitPrice = unitPrice;
                Amount = amount;
                SubcontractorAmount = subcontractorAmount;
                EstimatedAmount = estimatedAmount;
                SubItems = new List<EstimateSubcontractor>();
            }

            public void AddSubItem(EstimateSubcontractor subItem)
            {
                SubItems.Add(subItem);
            }

            public decimal GetTotalCost()
            {
                decimal total = SubcontractorAmount;
                foreach (var item in SubItems)
                {
                    total += item.GetTotalCost();
                }
                return total;
            }
        }

        public void AddSubItem(EstimateItem subItem)
        {
            SubItems.Add(subItem);
        }

        public decimal GetTotalCost()
        {
            decimal total = EstimatedAmount;
            foreach (var item in SubItems)
            {
                total += item.GetTotalCost();
            }
            return total;
        }

        public int GetDuration()
        {
            int duration = 0;
            foreach (var item in SubItems)
            {
                int itemDuration = item.GetDuration();
                if (itemDuration > duration)
                {
                    duration = itemDuration;
                }
            }

            if (EndDate > StartDate)
            {
                int itemDuration = (EndDate - StartDate).Days;
                if (itemDuration > duration)
                {
                    duration = itemDuration;
                }
            }

            return duration;
        }
    }

    public class ConstructionProject
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<EstimateItem> Items { get; set; }

        public ConstructionProject(string name, DateTime startDate, DateTime endDate, List<EstimateItem> items)
        {
            Name = name;
            StartDate = startDate;
            EndDate = endDate;
            Items = items;
        }

        public decimal GetTotalCost()
        {
            decimal total = 0;
            foreach (var item in Items)
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

            foreach (var item in Items)
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
