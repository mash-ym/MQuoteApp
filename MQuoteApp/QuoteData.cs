using System;
using System.Collections.Generic;

namespace MQuoteApp
{
    public class QuoteData
    {
        public string ProjectName { get; set; }
        public string SubcontractorName { get; set; }
        public List<Estimate> Estimates { get; set; }

        public QuoteData(string projectName, string subcontractorName)
        {
            ProjectName = projectName;
            SubcontractorName = subcontractorName;
            Estimates = new List<Estimate>();
        }

        public void AddEstimate(Estimate estimate)
        {
            Estimates.Add(estimate);
        }
    }
}
