using System.Collections.Generic;

namespace MQuoteApp
{
    public class Project
    {
        public string Name { get; set; }
        private List<QuoteData> quoteDataList;

        public Project(string name)
        {
            Name = name;
            quoteDataList = new List<QuoteData>();
        }
        

        public void AddQuoteData(QuoteData quoteData)
        {
            quoteDataList.Add(quoteData);
        }

        public List<QuoteData> GetQuoteData()
        {
            return quoteDataList;
        }
    }

}
