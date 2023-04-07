using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQuoteApp
{
    public class Quote
    {
        public decimal TotalAmount { get; set; }
        public decimal TotalEstimatedAmount { get; set; }

        public decimal CalculateAmount(IEnumerable<EstimateItem> items)
        {
            decimal totalAmount = 0;
            decimal totalEstimatedAmount = 0;

            foreach (var item in items)
            {
                totalAmount += item.GetTotalCost();
                totalEstimatedAmount += item.EstimatedAmount;
            }

            TotalAmount = totalAmount;
            TotalEstimatedAmount = totalEstimatedAmount;

            return totalAmount;
        }
    }

}
