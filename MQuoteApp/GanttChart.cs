using System;
using System.Collections.Generic;

namespace MQuoteApp
{
    // ガントチャートを表すクラス
    public class GanttChart
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<GanttChartItem> Items { get; set; }

        public GanttChart(DateTime startDate, DateTime endDate, List<GanttChartItem> items)
        {
            StartDate = startDate;
            EndDate = endDate;
            Items = items;
        }
        public class GanttChartItem
        {
            public string ProjectName { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }

            public GanttChartItem(string projectName, DateTime startDate, DateTime endDate)
            {
                ProjectName = projectName;
                StartDate = startDate;
                EndDate = endDate;
            }
        }public class GanttChartItem
    {
        public string ProjectName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public GanttChartItem(string projectName, DateTime startDate, DateTime endDate)
        {
            ProjectName = projectName;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
        // タスクリスト
        private List<Task> tasks = new List<Task>();

        // タスクの追加
        public void AddTask(Task task)
        {
            tasks.Add(task);
        }

        // タスクの削除
        public void RemoveTask(Task task)
        {
            tasks.Remove(task);
        }

        // タスクの変更
        public void UpdateTask(Task task)
        {
            int index = tasks.FindIndex(t => t.Id == task.Id);
            if (index >= 0)
            {
                tasks[index] = task;
            }
        }

        // タスクリストをソートする
        public void SortTasks()
        {
            tasks.Sort((t1, t2) => t1.StartDate.CompareTo(t2.StartDate));
        }

        // タスクの期間を計算する
        public TimeSpan CalculateDuration()
        {
            if (tasks.Count == 0)
            {
                return TimeSpan.Zero;
            }

            DateTime startDate = tasks[0].StartDate;
            DateTime endDate = tasks[0].EndDate;
            foreach (var task in tasks)
            {
                if (task.StartDate < startDate)
                {
                    startDate = task.StartDate;
                }
                if (task.EndDate > endDate)
                {
                    endDate = task.EndDate;
                }
            }

            return endDate - startDate;
        }

        // タスククラス
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
}
