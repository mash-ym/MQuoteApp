using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace MQuoteApp
{

    public class EstimateItem : ProjectItem
    {
        public string ItemName { get; internal set; } // 項目名
        public decimal Amount { get; set; } // 数量
        public string Unit { get; set; } // 単位
        public decimal UnitPrice { get; set; } // 単価 
        public string Remarks { get; set; } // 備考
        public decimal SubcontractorAmount { get; set; } // 下請け業者金額
        public decimal EstimatedAmount { get; set; } // 見積金額
        public List<EstimateItem> SubItems { get; set; } // 下位アイテムリスト
        public List<EstimateSubcontractor> Subcontractors { get; set; } // 下請け業者リスト
        public ConstructionProject Project { get; set; } // ConstructionProjectオブジェクトを保持するプロパティ
        public string Description { get; set; } // 説明

        public EstimateItem(string name, DateTime startDate, DateTime endDate, string itemName, decimal unitPrice, decimal amount, decimal subcontractorAmount, decimal estimatedAmount)
            : base(name, startDate, endDate)
        {
            ItemName = itemName;
            UnitPrice = unitPrice;
            Amount = amount;
            SubcontractorAmount = subcontractorAmount;
            EstimatedAmount = estimatedAmount;
            SubItems = new List<EstimateItem>();
            Subcontractors = new List<EstimateSubcontractor>();
        }
        public void AddSubItem(EstimateItem subItem)
        {
            SubItems.Add(subItem);
        }

        public void AddSubcontractor(EstimateSubcontractor subcontractor)
        {
            Subcontractors.Add(subcontractor);
        }

        public decimal GetTotalCost()
        {
            decimal total = SubcontractorAmount;
            foreach (var subcontractor in Subcontractors)
            {
                total += subcontractor.GetTotalCost();
            }
            foreach (var item in SubItems)
            {
                total += item.GetTotalCost();
            }
            return total;
        }
        public decimal GetSubtotal()
        {
            decimal subtotal = UnitPrice * Amount;
            foreach (var item in SubItems)
            {
                subtotal += item.GetSubtotal();
            }
            return subtotal;
        }

        public class EstimateSubcontractor
        {
            public string ItemName { get; set; } // 項目名
            public decimal Amount { get; set; } // 数量
            public string Unit { get; set; } // 単位
            public decimal UnitPrice { get; set; } // 単価 
            public decimal SubcontractorAmount { get; set; } // 下請け業者金額
            public decimal EstimatedAmount { get; set; } // 見積金額
            public List<EstimateSubcontractor> SubItems { get; set; } // 下位アイテムリスト


            public EstimateSubcontractor(string itemName, decimal unitPrice, decimal amount, decimal subcontractorAmount, decimal estimatedAmount)
            {
                ItemName = itemName;
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
        public void SaveToDatabase(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.CommandText = "INSERT INTO EstimateItems (ItemName, Amount, Unit, UnitPrice, Remarks, SubcontractorAmount, EstimatedAmount, Project, Description) VALUES (@ItemName, @Amount, @Unit, @UnitPrice, @Remarks, @SubcontractorAmount, @EstimatedAmount, @Project, @Description); SELECT SCOPE_IDENTITY()";
                command.Parameters.AddWithValue("@ItemName", this.ItemName);
                command.Parameters.AddWithValue("@Amount", this.Amount);
                command.Parameters.AddWithValue("@Unit", this.Unit);
                command.Parameters.AddWithValue("@UnitPrice", this.UnitPrice);
                command.Parameters.AddWithValue("@Remarks", this.Remarks);
                command.Parameters.AddWithValue("@SubcontractorAmount", this.SubcontractorAmount);
                command.Parameters.AddWithValue("@EstimatedAmount", this.EstimatedAmount);
                command.Parameters.AddWithValue("@Project", this.Project);
                command.Parameters.AddWithValue("@Description", this.Description);
                connection.Open();
                command.ExecuteNonQuery();

                if (this.SubItems != null && this.SubItems.Count > 0)
                {
                    foreach (var subItem in this.SubItems)
                    {
                        var subCommand = new SqlCommand();
                        subCommand.Connection = connection;
                        subCommand.CommandType = CommandType.Text;
                        subCommand.CommandText = "INSERT INTO EstimateSubcontractors (ItemName, Amount, Unit, UnitPrice, SubcontractorAmount, EstimatedAmount) VALUES (@ItemName, @Amount, @Unit, @UnitPrice, @SubcontractorAmount, @EstimatedAmount)";
                        subCommand.Parameters.AddWithValue("@ItemName", subItem.ItemName);
                        subCommand.Parameters.AddWithValue("@Amount", subItem.Amount);
                        subCommand.Parameters.AddWithValue("@Unit", subItem.Unit);
                        subCommand.Parameters.AddWithValue("@UnitPrice", subItem.UnitPrice);
                        subCommand.Parameters.AddWithValue("@SubcontractorAmount", subItem.SubcontractorAmount);
                        subCommand.Parameters.AddWithValue("@EstimatedAmount", subItem.EstimatedAmount);
                        subCommand.ExecuteNonQuery();
                    }
                }
            }
        }




        public decimal CalculateSubcontractorAmount()
        {
            decimal subAmount = 0;
            foreach (var item in SubItems)
            {
                subAmount += item.CalculateSubcontractorAmount();
            }
            subAmount += SubcontractorAmount;
            return subAmount;
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
            Items = new List<EstimateItem>();
            foreach (var item in items)
            {
                // EstimateItemオブジェクトを生成する際に、関連するConstructionProjectオブジェクトを指定して生成する
                Items.Add(new EstimateItem(item.ItemName, item.Amount, item.UnitPrice, item.StartDate, item.SubItems, this));
            }
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
