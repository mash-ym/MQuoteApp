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
        private readonly Dictionary<int, List<EstimateItem>> _dependencies;
        private readonly Dictionary<int, EstimateItem> _items;


        public EstimateDependencyGraph(List<EstimateItem> items)
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
        public bool EstimateDependency(EstimateItem item1, EstimateItem item2)
        {
            return item2.Dependencies.Contains(item1);
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
            return Items.FirstOrDefault(item => item.Id == id);
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

            // 依存するノードの終了日を再帰的に計算する
            foreach (var dependency in item.Dependencies)
            {
                var dependencyItem = GetEstimateItem(dependency.Id);
                if (dependencyItem == null) continue;

                Traverse(dependencyItem, depth + 1);

                // 依存するノードの中で終了日が最大のものを採用する
                if (dependencyItem.FinishDate.HasValue && (!item.FinishDate.HasValue || item.FinishDate.Value < dependencyItem.EndDate.Value))
                {
                    item.FinishDate = dependencyItem.FinishDate.Value;
                }
            }

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
        }

        public void CalculateEndDates()
        {
            // ルートノードから深さ優先探索をして、各ノードの終了日を計算する
            var rootNodes = GetRootNodes();
            foreach (var rootNode in rootNodes)
            {
                SetDepth(rootNode, 0);
                Traverse(rootNode, 0);
            }

            // すべてのItemに対してCalculateEndDateメソッドを呼び出す
            Items.ForEach(item => CalculateEndDate(item));
        }


    }
}

