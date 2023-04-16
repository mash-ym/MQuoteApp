using System;
using System.Collections.Generic;

namespace MQuoteApp
{
    public class EstimateDependencyGraph
    {
        private List<EstimateItem> _nodes;
        private Dictionary<EstimateItem, List<EstimateItem>> _edges;
        private Dictionary<EstimateItem, List<EstimateItem>> _dependencies;

        public EstimateDependencyGraph()
        {
            _nodes = new List<EstimateItem>();
            _edges = new Dictionary<EstimateItem, List<EstimateItem>>();
            _dependencies = new Dictionary<EstimateItem, List<EstimateItem>>();
        }

        public void AddNode(EstimateItem node)
        {
            if (!_nodes.Contains(node))
            {
                _nodes.Add(node);
                _edges[node] = new List<EstimateItem>();
            }
        }

        public void AddEdge(EstimateItem from, EstimateItem to)
        {
            if (!_nodes.Contains(from))
            {
                AddNode(from);
            }
            if (!_nodes.Contains(to))
            {
                AddNode(to);
            }
            if (!_edges[from].Contains(to))
            {
                _edges[from].Add(to);
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
        public Dictionary<EstimateItem, List<EstimateItem>> GetDependencies()
        {
            return _dependencies;
        }

        public bool HasCircularDependency()
        {
            var visited = new HashSet<EstimateItem>();
            var inProgress = new HashSet<EstimateItem>();

            foreach (var item in _dependencies.Keys)
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

            foreach (var dependentItem in _dependencies[item])
            {
                if (HasCircularDependency(dependentItem, visited, inProgress))
                {
                    return true;
                }
            }

            inProgress.Remove(item);

            return false;
        }
        public DateTime CalculateFinishDate()
        {
            var visited = new HashSet<EstimateItem>();
            var finishDates = new Dictionary<EstimateItem, DateTime>();

            foreach (var item in _dependencies.Keys)
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

        public void RemoveEdge(EstimateItem from, EstimateItem to)
        {
            if (_edges.ContainsKey(from))
            {
                _edges[from].Remove(to);
            }
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

        private void Visit(EstimateItem item, HashSet<EstimateItem> visited, Dictionary<EstimateItem, DateTime> finishDates)
        {
            visited.Add(item);

            DateTime maxFinishDate = DateTime.MinValue;

            foreach (var dependentItem in _dependencies[item])
            {
                if (!visited.Contains(dependentItem))
                {
                    Visit(dependentItem, visited, finishDates);
                }

                if (finishDates.ContainsKey(dependentItem) && finishDates[dependentItem] > maxFinishDate)
                {
                    maxFinishDate = finishDates[dependentItem];
                }
            }

            finishDates[item] = item.CalculateEndDate(maxFinishDate);
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
    }
}
