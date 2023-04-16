using System;
using System.Collections.Generic;
using System.Linq;

namespace MQuoteApp
{
    public class EstimateDependencyGraph
    {
        private List<EstimateItem> _nodes;
        private Dictionary<EstimateItem, List<EstimateItem>> _edges;
        private Dictionary<EstimateItem, List<EstimateItem>> _dependencies;
        private readonly Dictionary<EstimateItem, HashSet<EstimateItem>> _adjacencyList;
        private readonly Dictionary<EstimateItem, int> _inDegree;
        private List<EstimateItem> Items { get; set; }
        public List<EstimateDependency> Dependencies { get; set; }
        private readonly Dictionary<int, List<EstimateItem>> _dependencies;
        private readonly Dictionary<int, EstimateItem> _items;


        public EstimateDependencyGraph(List<EstimateItem> items, List<EstimateDependency> dependencies)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
            Dependencies = dependencies;
            _items = items.ToDictionary(i => i.Id, i => i);
            _dependencies = dependencies.GroupBy(d => d.ToItemId)
                .ToDictionary(g => g.Key, g => g.Select(d => _items[d.FromItemId]).ToList());
            _adjacencyList = new Dictionary<EstimateItem, HashSet<EstimateItem>>();
            _inDegree = new Dictionary<EstimateItem, int>();
            foreach (var item in items)
            {
                AddNode(item);
            }
        }
        public void EstimateDependency()
        {
            // Calculate start and end dates for all items in the graph
            CalculateEndDate();

            // Calculate dependencies for all items in the graph
            foreach (var item in Items)
            {
                // Get all dependencies for this item
                var dependencies = Dependencies.Where(d => d.ToItemId == item.Id).ToList();

                // Sort dependencies by start date in ascending order
                dependencies.Sort((d1, d2) => GetEstimateItem(d1.FromItemId).StartDate.CompareTo(GetEstimateItem(d2.FromItemId).StartDate));

                // Set the dependencies for the item
                EstimateItem item;
                if (!Items.TryGetValue(id, out item))
                {
                    item = new EstimateItem(id, name);
                    Items.Add(id, item);
                }

                if (item == null) return;

                item.Dependencies = dependencies;

            }
        }

        public void AddNode(EstimateItem node)
        {
            if (!_adjacencyList.ContainsKey(node))
            {
                _adjacencyList[node] = new HashSet<EstimateItem>();
                _inDegree[node] = 0;
            }
        }

        public void AddEdge(EstimateItem from, EstimateItem to)
        {
            if (!_adjacencyList.ContainsKey(from))
            {
                AddNode(from);
            }

            if (!_adjacencyList.ContainsKey(to))
            {
                AddNode(to);
            }

            if (!_adjacencyList[from].Contains(to))
            {
                _adjacencyList[from].Add(to);
                _inDegree[to]++;
            }
        }
        public void AddDependency(EstimateItem item, List<EstimateItem> dependentItems)
        {
            if (!_dependencies.ContainsKey(item))
            {
                _dependencies[item] = new List<EstimateItem>();
            }

            foreach (var dependentItem in dependentItems)
            {
                if (!_dependencies.ContainsKey(dependentItem))
                {
                    _dependencies[dependentItem] = new List<EstimateItem>();
                }

                _dependencies[item].Add(dependentItem);
            }
        }
        // 依存するノードのリストを取得する
        private List<EstimateDependency> GetDependencies(EstimateItem item)
        {
            return Items
                .Where(x => x.Id != item.Id && x.Dependencies.Any(y => y.Id == item.Id))
                .Select(x => new EstimateDependency(x.Id, x.Title))
                .ToList();
        }

        public bool HasCircularDependency()
        {
            var visited = new HashSet<EstimateItem>();
            var inProgress = new HashSet<EstimateItem>();

            foreach (var item in _items.Values)
            {
                if (HasCircularDependency(item, visited, inProgress))
                {
                    return true;
                }
            }

            return false;
        }
        private bool HasCircularDependency(EstimateItem item, HashSet<EstimateItem> visited, HashSet<EstimateItem> inProgress)
        {
            if (inProgress.Contains(item))
            {
                return true;
            }

            if (visited.Contains(item))
            {
                return false;
            }

            visited.Add(item);
            inProgress.Add(item);

            foreach (var dependentItem in _dependencies.GetValueOrDefault(item.Id, Enumerable.Empty<EstimateItem>()))
            {
                if (HasCircularDependency(dependentItem, visited, inProgress))
                {
                    return true;
                }
            }

            inProgress.Remove(item);

            return false;
        }
        public bool CalculateFinishDate()
        {
            var queue = new Queue<EstimateItem>(_inDegree.Where(x => x.Value == 0).Select(x => x.Key));
            var visited = new HashSet<EstimateItem>();
            var finishDates = new Dictionary<EstimateItem, DateTime>();

            foreach (var item in _items.Values)
            {
                if (!visited.Contains(item))
                {
                    Visit(item, visited, finishDates);
                }
            }

            DateTime finishDate = DateTime.MinValue;

            foreach (var date in finishDates.Values)
            {
                if (date > finishDate)
                {
                    finishDate = date;
                }
            }
            return finishDate;
        }

        private bool HasCircularDependencyRecursive(EstimateItem node, HashSet<EstimateItem> visited, HashSet<EstimateItem> recursionStack)
        {
            if (!visited.Contains(node))
            {
                visited.Add(node);
                recursionStack.Add(node);

                foreach (EstimateItem child in _edges[node])
                {
                    if (!visited.Contains(child) && HasCircularDependencyRecursive(child, visited, recursionStack))
                    {
                        return true;
                    }
                    else if (recursionStack.Contains(child))
                    {
                        return true;
                    }
                }
            }

            recursionStack.Remove(node);
            return false;
        }

        public bool RemoveEdge(EstimateItem from, EstimateItem to)
        {
            if (_edges.ContainsKey(from))
            {
                _edges[from].Remove(to);
            }
            if (!_adjacencyList.ContainsKey(from) || !_adjacencyList.ContainsKey(to))
            {
                return false;
            }

            if (!_adjacencyList[from].Contains(to))
            {
                return false;
            }

            _adjacencyList[from].Remove(to);
            _inDegree[to]--;

            return true;
        }

        public List<EstimateItem> GetNodes()
        {
            return _nodes;
        }

        public List<EstimateItem> GetChildren(EstimateItem node)
        {
            if (_edges.ContainsKey(node))
            {
                return _edges[node];
            }
            else
            {
                return new List<EstimateItem>();
            }
        }
        private EstimateItem GetEstimateItem(int id)
        {
            var result = _items.FirstOrDefault(x => x.Id.ToString() == id);
            return result;
        }


        private void Visit(EstimateItem item, HashSet<EstimateItem> visited, Dictionary<EstimateItem, DateTime> finishDates)
        {
            visited.Add(item);

            DateTime startDate = item.StartDate;
            DateTime finishDate = item.FinishDate;

            foreach (var dependentItem in _adjacencyList[item])
            {
                finishDates[dependentItem] = finishDates.ContainsKey(dependentItem) ?
                    DateTime.MaxValue : item.FinishDate;
                if (--_inDegree[dependentItem] == 0)
                {
                    Visit(dependentItem, visited, finishDates);
                }
            }
        }
        public void RemoveDependency(EstimateItem item)
        {
            _dependencies.Remove(item);

            foreach (var dependentItems in _dependencies.Values)
            {
                dependentItems.Remove(item);
            }
        }
        private void VisitRecursive(EstimateItem node, HashSet<EstimateItem> visited, Action<EstimateItem> visitor)
        {
            if (!visited.Contains(node))
            {
                visited.Add(node);

                foreach (EstimateItem child in _edges[node])
                {
                    VisitRecursive(child, visited, visitor);
                }

                visitor(node);
            }
        }

        public void DrawGraphWithPeriods()
        {
            // Create a new Graphviz graph
            var graph = new Graphviz.Graph("Estimate Dependency Graph");

            // Add nodes to the graph
            foreach (var item in Items)
            {
                var node = graph.Nodes.Add(item.Name);
                node.Label = $"{item.Name} ({item.Duration} days)";

                // Set the node style based on the item status
                if (item.Status == ItemStatus.InProgress)
                {
                    node.Style.FillColor = Graphviz.Color.LightBlue;
                }
                else if (item.Status == ItemStatus.Completed)
                {
                    node.Style.FillColor = Graphviz.Color.LightGreen;
                }
                else
                {
                    node.Style.FillColor = Graphviz.Color.White;
                }
            }

            // Add edges to the graph
            foreach (var item in Items)
            {
                foreach (var dep in item.Dependencies)
                {
                    var edge = graph.Edges.Add(item.Name, dep.Name);
                    edge.Label = $"{dep.Name} -> {item.Name}";
                }
            }

            // Output the graph in DOT format
            var dot = graph.Compile();
            Console.WriteLine(dot);

            // Output the graph in PNG format
            graph.RenderToFile("EstimateDependencyGraph.png");
        }

        // DFSでノードを走査する
        private void Traverse(EstimateItem item, int depth)
        {
            // 計算済みの場合は何もしない
            if (item.FinishDate.HasValue) return;

            // 依存するノードの終了日を再帰的に計算する
            foreach (var dependency in item.Dependencies)
            {
                var dependencyItem = GetEstimateItem(dependency.Id);
                if (dependencyItem == null) continue;
                Traverse(dependencyItem, depth + 1);
                // 依存するノードの中で終了日が最大のものを採用する
                if (dependencyItem.EndDate.HasValue && (!item.FinishDate.HasValue || item.FinishDate.Value < dependencyItem.EndDate.Value))
                {
                    item.FinishDate = dependencyItem.FinishDate.Value;
                }
            }

            // 依存するノードがない場合は開始日から期間を加算したものを終了日とする
            if (!item.FinishDate.HasValue)
            {
                item.FinishDate = item.StartDate.AddDays((double)item.Duration);
            }

            // ノードの深さを設定する
            SetDepth(item, depth);
        }
        private List<EstimateItem> GetRootNodes()
        {
            List<EstimateItem> rootNodes = new List<EstimateItem>();
            foreach (var item in Items)
            {
                bool isRoot = true;
                foreach (var dependency in Dependencies)
                {
                    if (dependency.ToId == item.Id)
                    {
                        isRoot = false;
                        break;
                    }
                }
                if (isRoot)
                {
                    rootNodes.Add(item);
                }
            }
            return rootNodes;
        }
        // 深さを設定する
        private void SetDepth(EstimateItem item, int depth)
        {
            item.Depth = depth;
            if (item.Parent == null)
            {
                return;
            }

            SetDepth(item.Parent, depth + 1);
        }
        private void CalculateEndDates()
        {
            // ルートノードを取得する
            var rootNodes = GetRootNodes();
            foreach (var rootNode in rootNodes)
            {
                // ルートノードからの深さを0に設定する
                SetDepth(rootNode, 0);
                // DFSでノードを走査する
                Traverse(rootNode, 0);
            }
        }

    }
}

