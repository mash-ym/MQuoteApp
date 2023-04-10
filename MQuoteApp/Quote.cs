using System;
using System.Collections.Generic;

namespace MQuoteApp
{
    public class Quote
    {
        public string ClientName { get; set; } // 見積書の顧客名を表すstring型のプロパティ
        public DateTime QuoteDate { get; set; } // 見積書の発行日を表すDateTime型のプロパティ
        public decimal TaxRate { get; set; } //見積書の消費税率を表すdecimal型のプロパティ
        public List<EstimateItem> Quotes { get; set; }

        public Quote()
        {
            Quotes = new List<EstimateItem>();
        }
        public List<Option> Options { get; set; } // 見積書のオプションを表すOption型のリスト
        public decimal Discount { get; set; } // 値引きの割合（％）
        public class Option
        {
            public string Name { get; set; } // オプションの名前を表すstring型のプロパティ
            public decimal Cost { get; set; } // オプションの金額を表すdecimal型のプロパティ
        }

        public Quote(string clientName, DateTime quoteDate, decimal taxRate)
        {
            ClientName = clientName;
            QuoteDate = quoteDate;
            TaxRate = taxRate;
            Quotes = new List<EstimateItem>();
            Options = new List<Option>();
            Discount = 0;
        }

        // 見積書の小計を計算するdecimal型のメソッド
        public virtual decimal CalculateTotal()
        {
            decimal subtotal = 0;
            foreach (var item in Quotes)
            {
                subtotal += item.GetTotalCost();
            }
            return subtotal;
        }
        // 見積書の消費税額を計算するdecimal型のメソッド
        public virtual decimal CalculateTax()
        {
            return CalculateTotal() * TaxRate;
        }
        // 見積書の合計金額を計算するdecimal型のメソッド
        public virtual decimal CalculateGrandTotal()
        {
            return CalculateTotal() + CalculateTax();
        }
        // 見積書に項目を追加するvoid型のメソッド
        public void AddItem(EstimateItem item)
        {
            Quotes.Add(item);
        }
        public decimal GetSubtotal()
        {
            decimal subtotal = 0;

            // 見積もりの項目の金額を加算
            foreach (EstimateItem item in Quotes)
            {
                subtotal += item.GetTotalCost();
            }

            // オプションの金額を加算
            foreach (var option in Options)
            {
                subtotal += option.Cost;
            }

            // 値引きを適用
            if (Discount != 0)
            {
                subtotal *= (1 - Discount / 100);
            }

            return subtotal;
        }
        public decimal CalculateTotalWithDiscount()
        {
            decimal total = CalculateGrandTotal();

            if (Discount > 0)
            {
                decimal discountAmount = total * (Discount / 100);
                total -= discountAmount;
            }

            return total;
        }



    }


}
