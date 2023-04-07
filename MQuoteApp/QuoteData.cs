using System;

namespace MQuoteApp
{
    public class QuoteData
    {
        public string Item { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total
        {
            get { return Quantity * Price; }
        }
        
        public DateTime Date { get; set; }

        public QuoteData(string item, decimal quantity, decimal price, DateTime date)
        {
            Item = item;
            Quantity = quantity;
            Price = price;
            Date = date;
        }
    }
}

