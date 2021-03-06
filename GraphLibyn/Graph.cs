﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GraphLibyn.Exceptions;
using Newtonsoft.Json;

namespace GraphLibyn
{
    public class Graph
    {
        private bool cacheIsEmpty = true;
        // Internally the class will always work with this node that exposes the AddNeighbor method
        // Externally everything should be done with string IDs. So all nodes in a Graph should be
        // GraphNodes and it will always be safe to cast them, but externally they will be Nodes
        protected class GraphNode : Node
        {
            // Make ctor private, only give access through method 
            private GraphNode(string id, Graph graph) : base(id, graph) { }

            // ensure that any GraphNode that is created is part of a graph
            public static GraphNode NewGraphNode(string id, Graph graph)
            {
                if (graph._nodesDictionary.ContainsKey(id))
                    throw new GraphCreationException("Id is already present, perhaps with 0 degree.");
                GraphNode graphNode = new GraphNode(id, graph);
                graph._nodesDictionary[id] = graphNode;
                return graphNode;
            }

            // Class basically exists for these methods, the publicly accessible Node can't be modified, here in the Graph this one can be
            public bool AddNeighbor(GraphNode n) => base.AddNeighbor(n);
            public bool RemoveNeighbor(GraphNode n) => base.RemoveNeighbor(n);

            // When a graph is dirty, the nodes also have to be set to dirty because a change in one node could affect
            // a different one that isn't automatically made dirty by the change. So the graph needs to reset nodes.
            public new void ResetCache()
            {
                base.EmptyCache();
            }

            public void SetGraph(Graph graph)
            {
                _graph = graph;
            }
        }

        // Simple wrapper, easier to call as method of the graph than it is to always pass in "this"
        protected GraphNode NewGraphNode(string id)
        {
            return GraphNode.NewGraphNode(id, this);
        }

        protected void EmptyCache()
        {
            if (!cacheIsEmpty)
            {
                _nodes = null;
                _allEdgesAsNodes = null;
                _degreeVector = null;
                _fiVector = null;
                _degreeCountDictionary = null;
                _nodeDegreeProbabilityDictionary = null;
                _edgeNodeDegreeProbabilityDictionary = null;
                _jointEdgeNodeExcessDegreeProbabilityDictionary = null;
                _excessDegreeVariance = Double.NegativeInfinity;
                _graphAssortativity = Double.NegativeInfinity;
                _graphAssortativity2 = Double.NegativeInfinity;
                _sumOfAllNodesAvgDiffs = Double.NegativeInfinity;
                _thedchansLambda = Double.NegativeInfinity;
                _sumOfAllFiMaxOverMin = Double.NegativeInfinity;
                _sumOfAllFiAbsoluteOfLn = Double.NegativeInfinity;
                // Design decision, it's too hard to differntiate between the node values that affect only themselves
                // and those that affect other nodes (and how many hops away, etc.) so any time the graph is dirty we
                // will force all node values to be reset as well
                _nodesDictionary.Values.ToList().ForEach(n => ((GraphNode) n).ResetCache());
            }
            cacheIsEmpty = true;
        }

        protected Dictionary<string, GraphNode> _nodesDictionary = new Dictionary<string, GraphNode>();
        private IEnumerable<GraphNode> _nodes = null;
        /// <summary>
        /// Collection of all nodes except zero degree
        /// </summary>
        public IEnumerable<Node> Nodes
        {
            get
            {
                cacheIsEmpty = false;
                if (_nodes == null)
                {
                    if(!_nodesDictionary.Any(kvp => kvp.Value.Degree > 0))
                        throw new EmptyGraphException("Graph is empty, possibly contains only 0 degree nodes");
                    _nodes = _nodesDictionary.Values.Where(n => n.Degree > 0);
                }
                return _nodes;
            }
        }


        // For internal use, can get all edges as Tuples of Node,Node
        private List<Tuple<Node, Node>> _allEdgesAsNodes = null;
        protected IEnumerable<Tuple<Node, Node>> EdgesAsNodes
        {
            get
            {
                cacheIsEmpty = false;
                return _allEdgesAsNodes ?? (_allEdgesAsNodes = Nodes.SelectMany(n => n.Neighbors.Select(n2 =>
                               n.Id.CompareTo(n2.Id) < 0
                                   ? new Tuple<Node, Node>(n, n2)
                                   : new Tuple<Node, Node>(n2, n)))
                           .Distinct()
                           .OrderBy(t => t.Item1.Id)
                           .ThenBy(t => t.Item2.Id)
                           .ToList());
            }
        }

        // For external use we'll give Edges as Tuples of Id,Id
        public IEnumerable<Tuple<string, string>> Edges => EdgesAsNodes.Select(t => new Tuple<string, string>(t.Item1.Id, t.Item2.Id));

        public bool AddNode(string Id)
        {
            if (_nodesDictionary.ContainsKey(Id))
                return false;
            NewGraphNode(Id);
            
            // NOTE: not resetting the cache because a new node is degree 0 and won't be included so this change doesn't affect anything

            return true;
        }

        // For internal use only, external code will only work with IDs, but interallly it
        // will sometimes be useful to create the nodes first and add them later, or use references to
        // existing nodes in order to add an edge between them
        protected bool AddEdge(Node node1, Node node2, bool addNodesToGraphIfAbsent = false)
        {
            var n1 = (GraphNode) node1;
            var n2 = (GraphNode) node2;

            if(!addNodesToGraphIfAbsent && (!_nodesDictionary.Values.Contains(n1) || !_nodesDictionary.Values.Contains(n2)))
                throw new GraphCreationException("Call to protected AddEdge(Node n1, Node n2) with nodes absent from graph");

            bool changeMade = false;
            if (!_nodesDictionary.ContainsKey(n1.Id))
            {
                _nodesDictionary[n1.Id] = n1;
                changeMade = true;
            }
            if (!_nodesDictionary.ContainsKey(n2.Id))
            {
                _nodesDictionary[n2.Id] = n2;
                changeMade = true;
            }
            if (n1.AddNeighbor(n2))
                changeMade = true;

            if (changeMade)
                EmptyCache();

            return changeMade;
        }

        public bool AddEdge(string id1, string id2, bool addNodeIfNotFound = true)
        {
            if (_nodesDictionary.ContainsKey(id1) && _nodesDictionary.ContainsKey(id2) && _nodesDictionary[id1].Neighbors.Any(n => n.Id == id2))
                return false;
            if (!addNodeIfNotFound && (!_nodesDictionary.ContainsKey(id1) || !_nodesDictionary.ContainsKey(id2)))
                throw new GraphCreationException("One or both nodes specified in an edge is not present in the graph");

            GraphNode n1 = _nodesDictionary.ContainsKey(id1) ? _nodesDictionary[id1] : NewGraphNode(id1);
            GraphNode n2 = _nodesDictionary.ContainsKey(id2) ? _nodesDictionary[id2] : NewGraphNode(id2);

            n1.AddNeighbor(n2);
            EmptyCache();
            return true;
        }

        protected bool RemoveEdge(Node node1, Node node2)
        {
            if (((GraphNode) node1).RemoveNeighbor((GraphNode) node2))
            {
                EmptyCache();
                return true;
            }
            return false;

        }

        public bool RemoveEdge(string id1, string id2)
        {
            if (!_nodesDictionary.ContainsKey(id1) || !_nodesDictionary.ContainsKey(id2))
                throw new GraphException($"Remove edge failed, {id1} or {id2} is not in the graph.");
            return RemoveEdge(_nodesDictionary[id1], _nodesDictionary[id2]);
        }

        private List<int> _degreeVector = null;

        public IEnumerable<int> DegreeVector
        {
            get
            {
                cacheIsEmpty = false;
                return _degreeVector ?? (_degreeVector = Nodes.Select(n => n.Degree).OrderBy(i => i).ToList());
            }
        }

        // useful function to give back the histogram as a string
        public string DegreeHistogram(Func<int, int> grpByFunc = null, string kvDelim = ":\t\t", string binDelim = "\n", bool ascendingOrder = true)
            => ascendingOrder
                ? string.Join(binDelim,
                    DegreeVector.GroupBy(grpByFunc ?? (i => i)).OrderBy(g => g.Key).Select(g => g.Key + kvDelim + g.Count()))
                : string.Join(binDelim,
                    DegreeVector.GroupBy(grpByFunc ?? (i => i)).OrderByDescending(g => g.Key).Select(g => g.Key + kvDelim + g.Count()));

        private List<double> _fiVector = null;

        public IEnumerable<double> FiVector
        {
            get
            {
                cacheIsEmpty = false;
                return _fiVector ?? (_fiVector = Nodes.Select(n => n.FIndex).OrderBy(d => d).ToList());
            }
        }

        // useful function to give back the histogram as a string
        public string FiVectorHistogram(Func<double, double> grpByFunc = null, string kvDelim = ":\t\t", string binDelim = "\n", bool ascendingOrder = true)
            => ascendingOrder
                ? string.Join(binDelim,
                    FiVector.GroupBy(grpByFunc ?? (d => d)).OrderBy(g => g.Key).Select(g => g.Key + kvDelim + g.Count()))
                : string.Join(binDelim,
                    FiVector.GroupBy(grpByFunc ?? (d => d)).OrderByDescending(g => g.Key).Select(g => g.Key + kvDelim + g.Count()));


        private Dictionary<int, int> _degreeCountDictionary = null;
        public Dictionary<int, int> DegreeCountDictionary
        {
            get
            {
                cacheIsEmpty = false;
                return _degreeCountDictionary ??
                       (_degreeCountDictionary =
                           Nodes.GroupBy(n => n.Degree).ToDictionary(g => g.Key, g => g.Count()));
            }
        }

        private Dictionary<int, double> _nodeDegreeProbabilityDictionary = null;

        private Dictionary<int, double> NodeDegreeProbabilityDictionary
        {
            get
            {
                cacheIsEmpty = false;
                return _nodeDegreeProbabilityDictionary ??
                       (_nodeDegreeProbabilityDictionary = Nodes.GroupBy(n => n.Degree)
                           .ToDictionary(g => g.Key, g => (double) g.Count()/Nodes.Count()));
            }
        }

        public double GetNodeDegreeProbability(int degree)
        {

            if (!NodeDegreeProbabilityDictionary.ContainsKey(degree))
                return 0;
            return NodeDegreeProbabilityDictionary[degree];
        }

        private Dictionary<int, double> _edgeNodeDegreeProbabilityDictionary = null;
        private Dictionary<int, double> EdgeNodeDegreeProbabilityDictionary
        {
            get
            {
                cacheIsEmpty = false;
                if (_edgeNodeDegreeProbabilityDictionary != null) return _edgeNodeDegreeProbabilityDictionary;
                double denom = Nodes.Sum(n => n.Degree);
                 return _edgeNodeDegreeProbabilityDictionary = Nodes.GroupBy(n => n.Degree)
                    .ToDictionary(g => g.Key, g => g.Count()*g.Key/denom);
            }
        }
        public double GetEdgeNodeDegreeProbability(int degree)
        {
            if (!EdgeNodeDegreeProbabilityDictionary.ContainsKey(degree))
                return 0;
            return _edgeNodeDegreeProbabilityDictionary[degree];
        }

        // For Newman's assortativity measure, q_k refers to the probability of an excess degree of k based on edges
        public double GetEdgeExcessNodeDegreeProbability(int excessDegree)
            => GetEdgeNodeDegreeProbability(excessDegree + 1);

        // The joint probability that both ends of an edge will have excess degrees j and k
        private Dictionary<int, Dictionary<int, double>> _jointEdgeNodeExcessDegreeProbabilityDictionary = null;

        private Dictionary<int, Dictionary<int, double>> JointEdgeNodeExcessDegreeProbabilityDictionary
        {
            get
            {
                cacheIsEmpty = false;
                if (_jointEdgeNodeExcessDegreeProbabilityDictionary != null) return _jointEdgeNodeExcessDegreeProbabilityDictionary;
                _jointEdgeNodeExcessDegreeProbabilityDictionary = new Dictionary<int, Dictionary<int, double>>();
                var excDegreeGroups = Nodes.GroupBy(n => n.ExcDegree);
                foreach (var excDegreeGroup in excDegreeGroups)
                {
                    _jointEdgeNodeExcessDegreeProbabilityDictionary[excDegreeGroup.Key] = new Dictionary<int, double>();
                    var allNeighbors = excDegreeGroup.SelectMany(n => n.Neighbors).GroupBy(n => n.ExcDegree);
                    var totalExcDegree = (excDegreeGroup.Key + 1)*excDegreeGroup.Count();
                    foreach (var neighborGroup in allNeighbors)
                    {
                        _jointEdgeNodeExcessDegreeProbabilityDictionary[excDegreeGroup.Key][neighborGroup.Key] =
                            GetEdgeExcessNodeDegreeProbability(excDegreeGroup.Key)*
                            ((double) neighborGroup.Count()/totalExcDegree);
                    }
                }
                return _jointEdgeNodeExcessDegreeProbabilityDictionary;
            }
        }

        public double GetEdgeExcessNodeDegreeProbability(int j, int k)
        {

            if (!JointEdgeNodeExcessDegreeProbabilityDictionary.ContainsKey(j))
                return 0.0;
            if (!JointEdgeNodeExcessDegreeProbabilityDictionary[j].ContainsKey(k))
                return 0.0;
            return JointEdgeNodeExcessDegreeProbabilityDictionary[j][k];
        }

        private double _excessDegreeVariance = Double.NegativeInfinity;

        public double ExcessDegreeVariance
        {
            get
            {
                cacheIsEmpty = false;
                return double.IsNegativeInfinity(_excessDegreeVariance)
                    ? (_excessDegreeVariance =
                        EdgeNodeDegreeProbabilityDictionary.Sum(kvp => Math.Pow(kvp.Key - 1, 2)*kvp.Value) -
                        Math.Pow(EdgeNodeDegreeProbabilityDictionary.Sum(kvp => (kvp.Key - 1)*kvp.Value), 2))
                    : _excessDegreeVariance;

            }
        }

        private double _graphAssortativity = double.NegativeInfinity;

        public double GraphAssortativity
        {
            get
            {
                cacheIsEmpty = false;
                if (!double.IsNegativeInfinity(_graphAssortativity))
                    return _graphAssortativity;

                if (ExcessDegreeVariance == 0) // All vertices are the same degree, automatically perfect assortativity
                    return _graphAssortativity = 1.0;

                var AllExcessDegrees = Nodes.Select(n => n.ExcDegree).Distinct().ToList();
                var total = 0.0;

                foreach (var j in AllExcessDegrees)
                    foreach (var k in AllExcessDegrees)
                        total += j*k*
                                 (GetEdgeExcessNodeDegreeProbability(j, k) -
                                  GetEdgeExcessNodeDegreeProbability(j)*GetEdgeExcessNodeDegreeProbability(k));
                return _graphAssortativity = total/ExcessDegreeVariance;

            }
        }

        // This is a second version following the more typical corelation equation, I'm mostly
        // including it for verification, may completely take it out.. YN 7/17/17
        private double _graphAssortativity2 = double.NegativeInfinity;
        public double GraphAssortativity2()
        {
            cacheIsEmpty = false;
            if (!Double.IsNegativeInfinity(_graphAssortativity2))
                return _graphAssortativity2;

            if (ExcessDegreeVariance == 0) // All vertices are the same degree, automatically perfect assortativity
                return _graphAssortativity2 = 1.0;

            double M = Edges.Count();
            double numerator = ((1/M)*(EdgesAsNodes.Sum(e => e.Item1.Degree*e.Item2.Degree)) -
                                Math.Pow((1/M)*0.5*EdgesAsNodes.Sum(e => e.Item1.Degree + e.Item2.Degree), 2.0));
            double denominator = ((1/M)*0.5*
                                  EdgesAsNodes.Sum(e => Math.Pow(e.Item1.Degree, 2) + Math.Pow(e.Item2.Degree, 2)) -
                                  Math.Pow((1/M)*0.5*EdgesAsNodes.Sum(e => e.Item1.Degree + e.Item2.Degree), 2.0));

            return _graphAssortativity2 = numerator/denominator;
        }

        #region EDGE_SWAPPING_EXPERIMENT
        // This is experimental (for now at least), trying to see if we can swap edges and change assortativity while
        // maintaining the degree vector, really need to reorganize this part. Would move it to a new class that inherits
        // from Graph, but I want to use it in ER and BA..
        public bool SwapRandomEdgesToIncreaseAssortativity()
        {
            Randomizer rand = new Randomizer();

            var edges = EdgesAsNodes.ToList();
            var e1 = edges[rand.Next(edges.Count)];
            var e2 = edges[rand.Next(edges.Count)];

            if (e1.Item1 == e2.Item1 || e1.Item1 == e2.Item2 || e1.Item2 == e2.Item1 || e1.Item2 == e2.Item2)
                return false;


            var currVal = ((double) Math.Max(e1.Item1.Degree, e1.Item2.Degree)/
                           Math.Min(e1.Item1.Degree, e1.Item2.Degree) +
                           (double) Math.Max(e2.Item1.Degree, e2.Item2.Degree)/
                           Math.Min(e2.Item1.Degree, e2.Item2.Degree))/2.0;
            var e1Ae2A = ((double) Math.Max(e1.Item1.Degree, e2.Item1.Degree)/
                          Math.Min(e1.Item1.Degree, e2.Item1.Degree) +
                          (double) Math.Max(e1.Item2.Degree, e2.Item2.Degree)/
                          Math.Min(e1.Item2.Degree, e2.Item2.Degree))/2.0;
            var e1Ae2B = ((double) Math.Max(e1.Item1.Degree, e2.Item2.Degree)/
                          Math.Min(e1.Item1.Degree, e2.Item2.Degree) +
                          (double) Math.Max(e1.Item2.Degree, e2.Item1.Degree)/
                          Math.Min(e1.Item2.Degree, e2.Item1.Degree))/2.0;

            String oldDegSeq = String.Join(",", DegreeVector);

            if (e1Ae2A < currVal && e1Ae2A < e1Ae2B
                && !e1.Item1.Neighbors.Contains(e2.Item1) && !e1.Item2.Neighbors.Contains(e2.Item2))
            {
                RemoveEdge(e1.Item1, e1.Item2);
                RemoveEdge(e2.Item1, e2.Item2);
                AddEdge(e1.Item1, e2.Item1);
                AddEdge(e1.Item2, e2.Item2);
                if (oldDegSeq != String.Join(",", DegreeVector))
                    throw new Exception("Holy cow");
                return true;
            }
            else if (e1Ae2B < currVal && e1Ae2B < e1Ae2A
                && !e1.Item1.Neighbors.Contains(e2.Item2) && !e1.Item2.Neighbors.Contains(e2.Item1))
            {
                RemoveEdge(e1.Item1, e1.Item2);
                RemoveEdge(e2.Item1, e2.Item2);
                AddEdge(e1.Item1, e2.Item2);
                AddEdge(e1.Item2, e2.Item1);
                if (oldDegSeq != String.Join(",", DegreeVector))
                    throw new Exception("Holy cow");
                return true;
            }
            return false;

        }

        public void SwapRandomEdgesToIncreaseAssortativity(int iters)
        {
            for (int i = 0; i < iters; i++)
                SwapRandomEdgesToIncreaseAssortativity();
        }

        private enum EdgeSwap
        {
            //e1Ae1B_e2Ae2B, // current configuration
            e1Ae2A_e1Be2B,
            e1Ae2B_e1Be2A
        }

        public void IterativelySwapEdgesToIncreaseAssortativity()
        {
            var edges = EdgesAsNodes.OrderBy(e => LocalAssort(e)).ToList();
            while (edges.Count > 1)
            {
                var e1 = edges[0];
                edges.Remove(e1);
                var otherEdges =
                    edges.Where(
                        e2 =>
                            e1.Item1 != e2.Item1 &&
                            e1.Item1 != e2.Item2 &&
                            e1.Item2 != e2.Item1 &&
                            e1.Item2 != e2.Item2).ToList();

                var currVal = LocalAssort(e1);
                var currMaxImprovement = 0.0;
                Tuple<Node, Node> currMaxEdge = null;
                EdgeSwap bestEdgeSwap = EdgeSwap.e1Ae2A_e1Be2B; // just need to assign something..

                foreach (var e2 in otherEdges)
                {
                    double improvement;

                    if (!e1.Item1.Neighbors.Contains(e2.Item1) && !e1.Item2.Neighbors.Contains(e2.Item2))
                    {
                        improvement = (LocalAssort(new Tuple<Node, Node>(e1.Item1, e2.Item1)) +
                                       LocalAssort(new Tuple<Node, Node>(e1.Item2, e2.Item2)))/
                                      2.0 - currVal;
                        if (improvement > currMaxImprovement)
                        {
                            currMaxImprovement = improvement;
                            currMaxEdge = e2;
                            bestEdgeSwap = EdgeSwap.e1Ae2A_e1Be2B;
                        }
                    }
                    if (!e1.Item1.Neighbors.Contains(e2.Item2) && !e1.Item2.Neighbors.Contains(e2.Item1))
                    {
                        improvement = (LocalAssort(new Tuple<Node, Node>(e1.Item1, e2.Item2)) +
                                       LocalAssort(new Tuple<Node, Node>(e1.Item2, e2.Item1)))/
                                      2.0 - currVal;
                        if (improvement > currMaxImprovement)
                        {
                            currMaxImprovement = improvement;
                            currMaxEdge = e2;
                            bestEdgeSwap = EdgeSwap.e1Ae2B_e1Be2A;
                        }
                    }

                }

                if (currMaxEdge != null)
                {
                    if (bestEdgeSwap == EdgeSwap.e1Ae2A_e1Be2B)
                    {
                        AddEdge(e1.Item1, currMaxEdge.Item1);
                        AddEdge(e1.Item2, currMaxEdge.Item2);
                    }
                    else
                    {
                        AddEdge(e1.Item1, currMaxEdge.Item2);
                        AddEdge(e1.Item2, currMaxEdge.Item1);
                    }
                    RemoveEdge(e1.Item1, e1.Item2);
                    RemoveEdge(currMaxEdge.Item1, currMaxEdge.Item2);
                    edges.Remove(currMaxEdge);
                }
            }
        }

        public void IterativelySwapEdgesToDecreaseAssortativity()
        {
            var edges = EdgesAsNodes.OrderByDescending(e => LocalAssort(e)).ToList();
            while (edges.Count > 1)
            {
                var e1 = edges[0];
                edges.Remove(e1);
                var otherEdges =
                    edges.Where(
                        e2 =>
                            e1.Item1 != e2.Item1 &&
                            e1.Item1 != e2.Item2 &&
                            e1.Item2 != e2.Item1 &&
                            e1.Item2 != e2.Item2).ToList();

                var currVal = LocalAssort(e1);
                var currMaxImprovement = 0.0;
                Tuple<Node, Node> currMaxEdge = null;
                EdgeSwap bestEdgeSwap = EdgeSwap.e1Ae2A_e1Be2B; // just need to assign something..

                foreach (var e2 in otherEdges)
                {
                    double improvement;

                    if (!e1.Item1.Neighbors.Contains(e2.Item1) && !e1.Item2.Neighbors.Contains(e2.Item2))
                    {
                        improvement = currVal - (LocalAssort(new Tuple<Node, Node>(e1.Item1, e2.Item1)) +
                                      LocalAssort(new Tuple<Node, Node>(e1.Item2, e2.Item2))) /
                                      2.0;
                        if (improvement > currMaxImprovement)
                        {
                            currMaxImprovement = improvement;
                            currMaxEdge = e2;
                            bestEdgeSwap = EdgeSwap.e1Ae2A_e1Be2B;
                        }
                    }
                    if (!e1.Item1.Neighbors.Contains(e2.Item2) && !e1.Item2.Neighbors.Contains(e2.Item1))
                    {
                        improvement = currVal - (LocalAssort(new Tuple<Node, Node>(e1.Item1, e2.Item2)) +
                                       LocalAssort(new Tuple<Node, Node>(e1.Item2, e2.Item1))) /
                                      2.0;
                        if (improvement > currMaxImprovement)
                        {
                            currMaxImprovement = improvement;
                            currMaxEdge = e2;
                            bestEdgeSwap = EdgeSwap.e1Ae2B_e1Be2A;
                        }
                    }

                }

                if (currMaxEdge != null)
                {
                    if (bestEdgeSwap == EdgeSwap.e1Ae2A_e1Be2B)
                    {
                        AddEdge(e1.Item1, currMaxEdge.Item1);
                        AddEdge(e1.Item2, currMaxEdge.Item2);
                    }
                    else
                    {
                        AddEdge(e1.Item1, currMaxEdge.Item2);
                        AddEdge(e1.Item2, currMaxEdge.Item1);
                    }
                    RemoveEdge(e1.Item1, e1.Item2);
                    RemoveEdge(currMaxEdge.Item1, currMaxEdge.Item2);
                    edges.Remove(currMaxEdge);
                }
            }
        }


        // For edge swapping, define this measure of local assortativity, Min/Max s.t. the result will be between 0 and 1, 1 is the highest assortativity 
        private double LocalAssort(Tuple<Node, Node> edge)
            => (double)Math.Min(edge.Item1.Degree, edge.Item2.Degree)/Math.Max(edge.Item1.Degree, edge.Item2.Degree);
        #endregion        

        private double _sumOfAllNodesAvgDiffs = Double.NegativeInfinity;
        public double SumOfAllNodesAvgDiffs
        {
            get
            {
                cacheIsEmpty = false;
                return Double.IsNegativeInfinity(_sumOfAllNodesAvgDiffs)
                    ? _sumOfAllNodesAvgDiffs = Nodes.Sum(n => n.AvgDiffFromNeighbors)
                    : _sumOfAllNodesAvgDiffs;
            }
        }

        // For Thedchan's local assort, the graph needs this measure of Lambda:
        private double _thedchansLambda = Double.NegativeInfinity;
        public double ThedchansLambda
        {
            get
            {
                cacheIsEmpty = false;
                return Double.IsNegativeInfinity(_thedchansLambda)
                    ? _thedchansLambda = (GraphAssortativity + 1.0)/Nodes.Count()
                    : _thedchansLambda;
            }
        }

        // A second measure we will use similar to Thedchan:
        private double _sumOfAllFiMaxOverMin = Double.NegativeInfinity;
        public double SumOfAllFiMaxOverMin
        {
            get
            {
                cacheIsEmpty = false;
                return Double.IsNegativeInfinity(_sumOfAllFiMaxOverMin)
                    ? _sumOfAllFiMaxOverMin = Nodes.Sum(n => n.FiMaxOverMin)
                    : _sumOfAllFiMaxOverMin;
            }
        }

        // A third measure we will use similar to Thedchan:
        private double _sumOfAllFiAbsoluteOfLn = Double.NegativeInfinity;
        public double SumOfAllFiAbsoluteOfLn
        {
            get
            {
                cacheIsEmpty = false;
                return Double.IsNegativeInfinity(_sumOfAllFiAbsoluteOfLn)
                    ? _sumOfAllFiAbsoluteOfLn = Nodes.Sum(n => n.FiAbsoluteOfLn)
                    : _sumOfAllFiAbsoluteOfLn;
            }
        }

        #region RANDOM_GRAPHS  // methods to generate types of random graphs
        // For random graph generating algorithms, this function will take a collection of nodes (caller may want to filter it for some reason)
        // and a function that will give each node a value v, and node i will have v_i/sum(v) probability of being selected
        protected List<GraphNode> SelectNodesWithBias(List<GraphNode> nodeCollection,
            int numNodesToSelect, Func<GraphNode, double> nodeBiasValFunc, Randomizer randomizer, bool keepSelectedInCollection = false)
        {
            List<GraphNode> selectedNodes = new List<GraphNode>();

            //var nodeCollectionList = nodeCollection.Select(n => new {node = (GraphNode)n, val = nodeBiasValFunc((GraphNode)n)}).ToList();
            double sumOfAllVals = nodeCollection.Sum(nodeBiasValFunc);
            

            for (int i = 0; i < numNodesToSelect; i++)
            {
                double curr = 0.0;
                double selectedVal = randomizer.NextDouble()*sumOfAllVals;
                foreach (var n in nodeCollection)
                {
                    var currNodesVal = nodeBiasValFunc(n);
                    if (curr <= selectedVal && curr + currNodesVal > selectedVal)
                    {
                        selectedNodes.Add(n);
                        if (!keepSelectedInCollection)
                        {
                            nodeCollection.Remove(n);
                            sumOfAllVals -= currNodesVal;
                        }
                        break;
                    }
                    curr += currNodesVal;
                }
            }

            return selectedNodes;
        }

        // for random graphs, we'll allow the caller to skip the Id and just autoincrement using this (ignoring the risk of caller adding an existing id manually)
        private int nextIdValue = 0;
        protected string nextId => "n" + nextIdValue++;

        // Add a new node per the Barabasi Albert algorithm, selecting a node with probability proportional to its degree
        public void AddNewBaNode(string id, int m, Randomizer randomizer)
        {
            var node = NewGraphNode(id);
            var connections = SelectNodesWithBias(Nodes.Select(n => (GraphNode)n).ToList(), m, n => n.Degree, randomizer);

            foreach (var connection in connections)
                AddEdge(node, connection);
        }

        // Wrapper to allow caller to rely on auto id
        public void AddNewBaNode(int m, Randomizer randomizer) => AddNewBaNode(nextId, m, randomizer);

        public static Graph NewBarabasiAlbertGraph(int n, int m, Randomizer randomizer)
        {
            Graph graph = new Graph();
            GraphNode firstNode = graph.NewGraphNode(graph.nextId);
            for (int i = 0; i < m; i++)
                graph.AddEdge(firstNode, graph.NewGraphNode(graph.nextId));

            for (int i = 0; i < n; i++) // leaving it imprecise, there will actually be n + m + 1 total nodes
                graph.AddNewBaNode(m, randomizer);

            return graph;
        }



        public static Graph NewErdosRenyiGraph(int n, double p, Randomizer randomizer)
        {
            Graph graph = new Graph();
            for (int i = 0; i < n; i++)
                graph.AddNode(graph.nextId);

            var nodesList = graph._nodesDictionary.Values.ToList();
            for (int i = 0; i < nodesList.Count; i++)
                for (int j = i + 1; j < nodesList.Count; j++)
                    if (randomizer.GetTrueWithProbability(p))
                        graph.AddEdge(nodesList[i], nodesList[j]);

            return graph;
        }

        // Trying to create a graph with both preferential attachment and high assortativity, have a few ideas for how to do
        // this but trying this one because I think it's simplest, may come back and try a few others.. 
        public static Graph NewCallawayPrefAttachmentAssortativeGraph(int n, long m, Randomizer randomizer,
            bool highAssortativity = true)
        {
            Graph graph = new Graph();

            for (int i = 0; i < n; i++)
                graph.AddNode(graph.nextId);

            for (long i = 0; i < m; i++)
            {
                var allNodesList = graph._nodesDictionary.Values.ToList(); // have to include zero-degree
                GraphNode firstNode = graph.SelectNodesWithBias(allNodesList, 1, node => node.Degree + 1, randomizer).First();
                // Remove nodes that are already neighbors
                allNodesList.RemoveAll(new HashSet<GraphNode>(firstNode.Neighbors.Select(node => (GraphNode) node)).Contains);
                if (!allNodesList.Any())
                    continue;
                var maxDiff = allNodesList.Max(node => Math.Abs(node.Degree - firstNode.Degree));
                Func<GraphNode, double> biasFunc;
                if (highAssortativity)
                    biasFunc = node => Math.Pow(Math.Abs(node.Degree - maxDiff) + 1, 2);
                else
                    biasFunc = node => Math.Pow(Math.Abs(node.Degree - firstNode.Degree) + 1, 2);
                var connection = graph.SelectNodesWithBias(allNodesList, 1, biasFunc, randomizer).FirstOrDefault();
                if (connection != null) // unlikely, but if this node is already connected to all others the collection will be empty
                    graph.AddEdge(connection, firstNode);
            }

            return graph;
        }

        #endregion

        /// <summary>
        /// Parse a graph from a string of lines (delimted by \n) in the form of N1\tN2
        /// </summary>
        /// <param name="graphstring"></param>
        public static Graph ParseGraphFromTsvEdgesString(string graphstring)
        {
            Graph graph = new Graph();
            // have to clean up different types of line delimeters
            graphstring = graphstring.Replace("\r", "\n").Replace("\n\n", "\n");
            var lines = Regex.Split(graphstring, @"\n");
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var parts = Regex.Split(line, @"\t+"); // allow multiple \t characters between the parts
                if (parts.Length != 2)
                    throw new GraphCreationException($"Line: {line} does not contain exactly two nodes.");
                graph.AddEdge(parts[0], parts[1]);
            }
            return graph;
        }

        /// <summary>
        /// Parse a graph from a file containing a string of lines (delimted by \n) in the form of N1\tN2
        /// </summary>
        /// <param name="graphstring"></param>
        public static Graph ParseGraphFromTsvEdgesFile(string path)
        {
            return ParseGraphFromTsvEdgesString(File.ReadAllText(path));
        }

        /// <summary>
        /// Parse a graph from a string of lines (delimted by \n) in the form of N\tn1,n2,n3...
        /// </summary>
        /// <param name="graphstring"></param>
        public static Graph ParseGraphFromTsvNodeNeighborsString(string graphstring, bool AddMissingNodesAndEdges = false)
        {
            Graph graph = new Graph();
            Dictionary<string, ISet<string>> nodes = new Dictionary<string, ISet<string>>();

                // have to clean up different types of line delimeters
            graphstring = graphstring.Replace("\r", "\n").Replace("\n\n", "\n");
            var lines = Regex.Split(graphstring, @"\n");
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var parts = Regex.Split(line, @"\t+"); // allow multiple \t characters between the parts
                if (parts.Length != 2)
                    throw new GraphCreationException($"Line: {line} does not contain exactly one node with neighbors list");
                
                if (nodes.ContainsKey(parts[0].Trim()))
                    throw new GraphCreationException($"Multiple entries found for node {parts[0]}");

                nodes[parts[0].Trim()] = new HashSet<string>(Regex.Split(parts[1], ",").Select(s => s.Trim()));
            }

            if (!AddMissingNodesAndEdges)
            {
                // Are there any missing nodes
                var missingNode = nodes.Values.SelectMany(s => s).FirstOrDefault(n => !nodes.Keys.Contains(n));
                if (!string.IsNullOrEmpty(missingNode))
                    throw new GraphCreationException(
                        $"{missingNode} is listed as a neighbor but is not found in the graph.");

                // Are there any asymmetric edges
                var missingEdge = nodes.FirstOrDefault(n1 => n1.Value.Any(n2 => !nodes[n2].Contains(n1.Key))).Key;
                if (!String.IsNullOrEmpty(missingEdge))
                    throw new GraphCreationException(
                        $"{missingEdge} has a neighbor that does not list it as its own neighbor");
            }


            nodes.ToList().ForEach(n1 => n1.Value.ToList().ForEach(n2 => graph.AddEdge(n1.Key, n2, true)));
         

            return graph;
        }

        /// <summary>
        /// Parse a graph from a file containing a string of lines (delimted by \n) in the form of N\tn1,n2,n3...
        /// </summary>
        /// <param name="graphstring"></param>
        public static Graph ParseGraphFromTsvNodeNeighborsFile(string path, bool AddMissingEdges = false)
        {
            return ParseGraphFromTsvNodeNeighborsString(File.ReadAllText(path), AddMissingEdges);
        }

        public static Graph Clone(Graph graph)
        {
            Graph retGraph = new Graph();
            foreach (var edge in graph.Edges)
                retGraph.AddEdge(edge.Item1, edge.Item2);
            return retGraph;
        }

        public Graph Clone()
        {
            return Clone(this);
        }

    }
}
