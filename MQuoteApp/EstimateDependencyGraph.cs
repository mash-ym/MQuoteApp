using System.Collections.Generic;

namespace MQuoteApp
{
    public class EstimateDependencyGraph
    {
        private Dictionary<EstimateItem, List<EstimateItem>> _graph = new Dictionary<EstimateItem, List<EstimateItem>>();
        private void CalculateFinishDate(EstimateItem item)
        {
            if (item.StartDate.HasValue && item.Duration.HasValue)
            {
                item.FinishDate = item.StartDate.Value.AddDays(item.Duration.Value);
                // 子ノードの終了日を再帰的に計算
                foreach (var childItem in item.SubItems
                    )
                {
                    CalculateFinishDate(childItem);
                }
            }
        }



        public void AddDependency(EstimateItem from, EstimateItem to)
        {
            if (!_graph.ContainsKey(from))
            {
                _graph[from] = new List<EstimateItem>();
            }
            _graph[from].Add(to);
        }

        public void RemoveDependency(EstimateItem from, EstimateItem to)
        {
            if (_graph.ContainsKey(from))
            {
                _graph[from].Remove(to);
            }
        }

        public List<EstimateItem> GetDependencies(EstimateItem from)
        {
            if (_graph.ContainsKey(from))
            {
                return _graph[from];
            }
            return new List<EstimateItem>();
        }

        public List<EstimateItem> GetDependents(EstimateItem to)
        {
            List<EstimateItem> dependents = new List<EstimateItem>();
            foreach (EstimateItem from in _graph.Keys)
            {
                if (_graph[from].Contains(to))
                {
                    dependents.Add(from);
                }
            }
            return dependents;
        }

        public void Clear()
        {
            _graph.Clear();
        }

        public List<EstimateItem> TopologicalSort()
        {
            Dictionary<EstimateItem, int> indegrees = new Dictionary<EstimateItem, int>();
            foreach (EstimateItem from in _graph.Keys)
            {
                indegrees[from] = 0;
            }
            foreach (EstimateItem from in _graph.Keys)
            {
                foreach (EstimateItem to in _graph[from])
                {
                    if (!indegrees.ContainsKey(to))
                    {
                        indegrees[to] = 0;
                    }
                    indegrees[to]++;
                }
            }

            List<EstimateItem> sortedItems = new List<EstimateItem>();
            Queue<EstimateItem> queue = new Queue<EstimateItem>();
            foreach (EstimateItem item in indegrees.Keys)
            {
                if (indegrees[item] == 0)
                {
                    queue.Enqueue(item);
                }
            }
            while (queue.Count > 0)
            {
                EstimateItem item = queue.Dequeue();
                sortedItems.Add(item);
                foreach (EstimateItem to in _graph[item])
                {
                    indegrees[to]--;
                    if (indegrees[to] == 0)
                    {
                        queue.Enqueue(to);
                    }
                }
            }

            return sortedItems;
        }
    } 
}




