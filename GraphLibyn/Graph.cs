using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GraphLibyn.Exceptions;

namespace GraphLibyn
{
    public class Graph
    {
        // Internally the class will always work with this node that exposes the AddNeighbor method
        // Externally everything should be done with string IDs. So all nodes in a Graph should be
        // GraphNodes and it will always be safe to cast them, but externally they will be Nodes
        protected class GraphNode : Node
        {
            // Make ctor private, only give access through method 
            private GraphNode(string id) : base(id) { }

            // ensure that any GraphNode that is created is part of a graph
            public static GraphNode NewGraphNode(string id, Graph graph)
            {
                if (graph._nodes.ContainsKey(id))
                    throw new GraphCreationException("Id is already present, perhaps with 0 degree.");
                GraphNode graphNode = new GraphNode(id);
                graph._nodes[id] = graphNode;
                return graphNode;
            }

            // Class basically exists for this method, the publicly accessible Node can't be modified, here in the Graph this one can be
            public bool AddNeighbor(GraphNode n)
            {
                return base.AddNeighbor(n);
            }
        }

        // Simple wrapper, easier to call as method of the graph than it is to always pass in "this"
        protected GraphNode NewGraphNode(string id)
        {
            return GraphNode.NewGraphNode(id, this);
        }

        protected void ResetCollections()
        {
            _allNodes = null;
            _allEdgesAsNodes = null;
            _degreeVector = null;
            _fiVector = null;
        }

        protected Dictionary<string, GraphNode> _nodes = new Dictionary<string, GraphNode>();
        private IEnumerable<GraphNode> _allNodes = null;
        public IEnumerable<Node> AllNodes
        {
            get
            {
                if (_allNodes == null)
                {
                    if(!_nodes.Any(kvp => kvp.Value.Degree > 0))
                        throw new EmptyGraphException("Graph is empty, possibly contains only 0 degree nodes");
                    _allNodes = _nodes.Values.Where(n => n.Degree > 0);
                }
                return _allNodes;
            }
        }

        // For internal use, can get all edges as Tuples of Node,Node
        private List<Tuple<Node, Node>> _allEdgesAsNodes = null;

        protected IEnumerable<Tuple<Node, Node>> EdgesAsNodes
        {
            get
            {
                return _allEdgesAsNodes ?? (_allEdgesAsNodes = AllNodes.SelectMany(n => n.Neighbors.Select(n2 =>
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
            if (_nodes.ContainsKey(Id))
                return false;
            NewGraphNode(Id);
            
            // NOTE: not resetting the collections because a new node is degree 0 and won't be included so this change doesn't affect anything

            return true;
        }

        // For internal use only, external code will only work with IDs, but interallly it
        // will sometimes be useful to create the nodes first and add them later, or use references to
        // existing nodes in order to add an edge between them
        protected bool AddEdge(Node node1, Node node2, bool addNodesToGraphIfAbsent = false)
        {
            var n1 = (GraphNode) node1;
            var n2 = (GraphNode) node2;

            if(!addNodesToGraphIfAbsent && (!_nodes.Values.Contains(n1) || !_nodes.Values.Contains(n2)))
                throw new GraphCreationException("Call to protected AddEdge(Node n1, Node n2) with nodes absent from graph");

            // This next one really can't be tested. Externally you can't create GraphNodes, only nodes. So the only way externally to add a
            // GraphNode that is not part of the graph is if it is part of another graph. Bit remote. So just relying on this exception
            // to be correct if something interally would create a GraphNode and add it to a Graph that already has a Node with that ID.
            if ((!_nodes.Values.Contains(n1) && _nodes.Values.Any(n => n.Id == n1.Id)) ||
                (!_nodes.Values.Contains(n2) && _nodes.Values.Any(n => n.Id == n2.Id)))
                throw new GraphCreationException(
                    "Trying to add node that is absent from graph but another node with this id exists");

            bool changeMade = false;
            if (!_nodes.ContainsKey(n1.Id))
            {
                _nodes[n1.Id] = n1;
                changeMade = true;
            }
            if (!_nodes.ContainsKey(n2.Id))
            {
                _nodes[n2.Id] = n2;
                changeMade = true;
            }
            if (n1.AddNeighbor(n2))
                changeMade = true;

            if (changeMade)
                ResetCollections();

            return changeMade;
        }

        public bool AddEdge(string id1, string id2, bool addNodeIfNotFound = true)
        {
            if (_nodes.ContainsKey(id1) && _nodes.ContainsKey(id2) && _nodes[id1].Neighbors.Any(n => n.Id == id2))
                return false;
            if (!addNodeIfNotFound && (!_nodes.ContainsKey(id1) || !_nodes.ContainsKey(id2)))
                throw new GraphCreationException("One or both nodes specified in an edge is not present in the graph");

            GraphNode n1 = _nodes.ContainsKey(id1) ? _nodes[id1] : NewGraphNode(id1);
            GraphNode n2 = _nodes.ContainsKey(id2) ? _nodes[id2] : NewGraphNode(id2);

            n1.AddNeighbor(n2);
            ResetCollections();
            return true;
        }

        private List<int> _degreeVector = null;
        public IEnumerable<int> DegreeVector
        {
            get { return _degreeVector ?? (_degreeVector = AllNodes.Select(n => n.Degree).OrderBy(i => i).ToList()); }
        }

        private List<double> _fiVector = null;
        public IEnumerable<double> FiVector
        {
            get { return _fiVector ?? (_fiVector = AllNodes.Select(n => n.FIndex).OrderBy(d => d).ToList()); }
        }

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

    }
}
