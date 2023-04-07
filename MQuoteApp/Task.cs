using System;

namespace MQuoteApp
{
    public class Task : ProjectItem
    {
        private static int nextId = 1;

        public int Id { get; }
        public string SubcontractorName { get; set; }

        public Task(string name, DateTime startDate, DateTime endDate, string subcontractorName)
            :base(name, startDate, endDate)
        {
            Id = nextId++;
            SubcontractorName = subcontractorName;
        }

        public override string ToString()
        {
            return $"{Name} ({StartDate.ToShortDateString()} - {EndDate.ToShortDateString()})";
        }
    }
}
