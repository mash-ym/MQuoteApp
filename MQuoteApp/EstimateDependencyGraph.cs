using System;
using System.Collections.Generic;

namespace MQuoteApp
{
    public class EstimateDependencyGraph
    {
        private List<EstimateItem> _nodes;
        private Dictionary<EstimateItem, List<EstimateItem>> _edges;

        public EstimateDependencyGraph()
        {
            _nodes = new List<EstimateItem>();
            _edges = new Dictionary<EstimateItem, List<EstimateItem>>();
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

        public bool HasCircularDependency()
        {
            HashSet<EstimateItem> visited = new HashSet<EstimateItem>();
            HashSet<EstimateItem> recursionStack = new HashSet<EstimateItem>();

            foreach (EstimateItem node in _nodes)
            {
                if (HasCircularDependencyRecursive(node, visited, recursionStack))
                {
                    return true;
                }
            }

            return false;
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

        public void Visit(Action<EstimateItem> visitor)
        {
            HashSet<EstimateItem> visited = new HashSet<EstimateItem>();
            foreach (EstimateItem node in _nodes)
            {
                VisitRecursive(node, visited, visitor);
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
