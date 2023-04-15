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
        public string Id { get; set; }// ID
        public string ItemName { get; internal set; } // 項目名
        public decimal Amount { get; set; } // 数量
        public string Unit { get; set; } // 単位
        public decimal UnitPrice { get; set; } // 単価 
        public string Remarks { get; set; } // 備考
        public decimal SubcontractorAmount { get; set; } // 下請け業者金額
        public decimal EstimatedAmount { get; set; } // 見積金額
        public List<EstimateItem> SubItems { get; set; } // 下位アイテムリスト
        public List<EstimateSubcontractor> Subcontractors { get; set; } // 下請け業者リスト
        public List<EstimateItem> Dependencies { get; set; }
        public string Description { get; set; } // 説明
        public EstimateItem(string id, string name, DateTime startDate, DateTime finishDate, string itemName, decimal unitPrice, decimal amount, decimal subcontractorAmount, decimal estimatedAmount, int? duration, TimeSpan period, List<EstimateItem> dependencies)
            : base(name, startDate, finishDate, duration, period)
        {
            Id = id;
            ItemName = itemName;
            UnitPrice = unitPrice;
            Amount = amount;
            SubcontractorAmount = subcontractorAmount;
            EstimatedAmount = estimatedAmount;
            SubItems = new List<EstimateItem>();
            Subcontractors = new List<EstimateSubcontractor>();
            StartDate = startDate;
            FinishDate = finishDate;
            Duration = duration;
            period = period;
            Dependencies = dependencies;
        }
        public DateTime CalculateEndDate()
        {
            if (Dependencies.Count == 0)
            {
                // 依存関係がない場合は自身の開始日を終了日とする
                return StartDate;
            }

            // 依存関係がある場合は、最大の終了日を計算する
            DateTime maxEndDate = DateTime.MinValue;
            foreach (var dependency in Dependencies)
            {
                var dependencyEndDate = dependency.CalculateEndDate();
                if (dependencyEndDate > maxEndDate)
                {
                    maxEndDate = dependencyEndDate;
                }
            }

            // 自身の終了日を計算する
            var duration = Duration ?? 0; // 見積もり工数が設定されていない場合は0とする
            var endDate = maxEndDate.AddDays(duration);

            // 終了日を更新する
            FinishDate = endDate;

            return endDate;
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

            if (FinishDate > StartDate)
            {
                int itemDuration = (FinishDate - StartDate).Days;
                if (itemDuration > duration)
                {
                    duration = itemDuration;
                }
            }

            return duration;
        }
        public DateTime GetEndDate(DateTime startDate, int duration)
        {
            return startDate.AddDays(duration);
        }
    }
}
