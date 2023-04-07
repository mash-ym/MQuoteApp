using System;

namespace MQuoteApp
{
    public class Task
    {
        private static int nextId = 1;

        public int Id { get; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SubcontractorName { get; set; }

        public Task(string name, DateTime startDate, DateTime endDate, string subcontractorName)
        {
            Id = nextId++;
            Name = name;
            StartDate = startDate;
            EndDate = endDate;
            SubcontractorName = subcontractorName;
        }

        public override string ToString()
        {
            return $"{Name} ({StartDate.ToShortDateString()} - {EndDate.ToShortDateString()})";
        }
    }
}
