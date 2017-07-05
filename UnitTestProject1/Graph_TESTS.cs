using System;
using System.Collections.Generic;
using System.Linq;
using GraphLibyn;
using GraphLibyn.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class Graph_TESTS
    {
        // Wrapper for testing, expose protected methods and private variables
       /* private class TestGraph : Graph
        {
            // Expose the AddEdge method that works directly with nodes, should only be used to
            // test the method itself
            public new bool AddEdge(Node node1, Node node2, bool addNodesToGraphIfAbsent = false)
                => base.AddEdge(node1, node2, addNodesToGraphIfAbsent);

            // Expose the protected property that gives Edges as Tuple<Node, Node> objects
            public new IEnumerable<Tuple<Node, Node>> EdgesAsNodes => base.EdgesAsNodes;


        }*/

        // For class invariants, any graph created should be added to this list
        private List<Graph> TestGraphs = new List<Graph>();

        private bool AllGraphsHaveNoZeroDegreeNodes()
        {
            return TestGraphs.All(g => g.AllNodes.All(n => n.Degree > 0));
        }

        // A graph I dreamed up...
        private string testGraphString1 = "n1\tn3" + "\n" +
                                          "n2\tn3" + "\n" +
                                          "n3\tn1,n2,n5" + "\n" +
                                          "n4\tn5" + "\n" +
                                          "n5\tn3,n4,n6" + "\n" +
                                          "n6\tn5,n7,n8,n9,n10,n11" + "\n" +
                                          "n7\tn6" + "\n" +
                                          "n8\tn6" + "\n" +
                                          "n9\tn6" + "\n" +
                                          "n10\tn6" + "\n" +
                                          "n11\tn6" + "\n";

        [TestInitialize]
        public void Setup()
        {
            TestGraphs = new List<Graph>();
        }

        [TestMethod]
        public void ParseNodeNodeStringParsesCorrectly()
        {
            // A graph with 1 node
            string graphString = "n1\tn2";
            Graph graph = Graph.ParseGraphFromTsvEdgesString(graphString);
            TestGraphs.Add(graph);
            Assert.AreEqual(2, graph.AllNodes.Count());
            Assert.IsTrue(graph.AllNodes.Any(n => n.Id == "n1") && graph.AllNodes.Any(n => n.Id == "n2"));
            Assert.IsTrue(graph.AllNodes.First(n => n.Id == "n1").Neighbors.Any(n => n.Id == "n2") &&
                          graph.AllNodes.First(n => n.Id == "n2").Neighbors.Any(n => n.Id == "n1"));
            Assert.IsTrue(graph.AllNodes.All(n => n.Degree == 1));

            // A graph n1 <--> n2 <--> n3
            graphString = "n1\tn2" + "\n" +
                          "n2\tn3";
            graph = Graph.ParseGraphFromTsvEdgesString(graphString);
            TestGraphs.Add(graph);
            Assert.AreEqual(3, graph.AllNodes.Count());
            Assert.AreEqual("n1,n2,n3", String.Join(",", graph.AllNodes.Select(n => n.Id).OrderBy(s => s)));
            Assert.IsTrue(graph.AllNodes.First(n => n.Id == "n1").Degree == 1 &&
                          graph.AllNodes.First(n => n.Id == "n2").Degree == 2 &&
                          graph.AllNodes.First(n => n.Id == "n3").Degree == 1);

            // A triangle 
            graphString = "n1\tn2" + "\n" +
                          "n2\tn3" + "\n" +
                          "n3\tn1";
            graph = Graph.ParseGraphFromTsvEdgesString(graphString);
            TestGraphs.Add(graph);
            Assert.AreEqual(3, graph.AllNodes.Count());
            Assert.AreEqual("n1,n2,n3", String.Join(",", graph.AllNodes.Select(n => n.Id).OrderBy(s => s)));
            Assert.IsTrue(graph.AllNodes.First(n => n.Id == "n1").Degree == 2 &&
                          graph.AllNodes.First(n => n.Id == "n2").Degree == 2 &&
                          graph.AllNodes.First(n => n.Id == "n3").Degree == 2);
            Assert.IsTrue(graph.AllNodes.First(n => n.Id == "n1").Neighbors.Any(n => n.Id == "n3") &&
                          graph.AllNodes.First(n => n.Id == "n3").Neighbors.Any(n => n.Id == "n1"));

            // n1<-->n2 n3<-->n4
            graphString = "n1\tn2" + "\n" +
                          "n4\tn3";
            graph = Graph.ParseGraphFromTsvEdgesString(graphString);
            TestGraphs.Add(graph);
            Assert.AreEqual(4, graph.AllNodes.Count());
            Assert.AreEqual("n1,n2,n3,n4", String.Join(",", graph.AllNodes.Select(n => n.Id).OrderBy(s => s)));
            Assert.IsTrue(graph.AllNodes.First(n => n.Id == "n1").Degree == 1 &&
                          graph.AllNodes.First(n => n.Id == "n2").Degree == 1 &&
                          graph.AllNodes.First(n => n.Id == "n3").Degree == 1 &&
                          graph.AllNodes.First(n => n.Id == "n4").Degree == 1);
            Assert.IsTrue(graph.AllNodes.First(n => n.Id == "n1").Neighbors.Any(n => n.Id == "n2") &&
                          graph.AllNodes.First(n => n.Id == "n2").Neighbors.Any(n => n.Id == "n1"));
            Assert.IsTrue(graph.AllNodes.First(n => n.Id == "n3").Neighbors.Any(n => n.Id == "n4") &&
                          graph.AllNodes.First(n => n.Id == "n4").Neighbors.Any(n => n.Id == "n3"));

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void ParseNodeNodeStringHashLinesAreIgnored()
        {
            var graphString = "n1\tn2" + "\n" +
                              "#This is a comment" + "\n" +
                              "n2\tn3";
            var graph = Graph.ParseGraphFromTsvEdgesString(graphString);
            TestGraphs.Add(graph);
            Assert.AreEqual("n1,n2,n3", string.Join(",", graph.AllNodes.Select(n => n.Id).OrderBy(s => s)));

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void ParseNodeNeighborsIsCorrect()
        {
            // Triangle
            var graphString = "n1\tn2,n3" + "\n" +
                              "n2\tn3,n1" + "\n" +
                              "n3\tn2,n1";
            var graph = Graph.ParseGraphFromTsvNodeNeighborsString(graphString);
            TestGraphs.Add(graph);
            Assert.AreEqual("n1,n2,n3", String.Join(",", graph.AllNodes.Select(n => n.Id).OrderBy(s => s)));
            Assert.IsTrue(graph.AllNodes.All(n => n.Degree == 2));

            // n1 <--> n2 <--> n3
            graphString = "n1\tn2" + "\n" +
                          "n2\tn1,n3" + "\n" +
                          "n3\tn2";
            TestGraphs.Add(graph = Graph.ParseGraphFromTsvNodeNeighborsString(graphString));
            Assert.AreEqual("n1,n2,n3", String.Join(",", graph.AllNodes.Select(n => n.Id).OrderBy(s => s)));
            Assert.AreEqual(2, graph.AllNodes.First(n => n.Id == "n2").Degree);
            Assert.AreEqual(1, graph.AllNodes.First(n => n.Id == "n1").Degree);
            Assert.AreEqual(1, graph.AllNodes.First(n => n.Id == "n3").Degree);
            Assert.IsTrue(graph.AllNodes.First(n => n.Id == "n2").Neighbors.Any(n => n.Id == "n3") &&
                          graph.AllNodes.First(n => n.Id == "n2").Neighbors.Any(n => n.Id == "n1"));
            Assert.IsFalse(graph.AllNodes.First(n => n.Id == "n3").Neighbors.Any(n => n.Id == "n1"));

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void ParseNodeNeighborsIgnoresHashes()
        {
            var graphString = "n1\tn2" + "\n" +
                              "n2\tn1,n3" + "\n" +
                              "#n4\tn5,n6" + "\n" +
                              "n3\tn2";
            var graph = Graph.ParseGraphFromTsvNodeNeighborsString(graphString);
            TestGraphs.Add(graph);
            Assert.IsFalse(graph.AllNodes.Any(n => n.Id == "n4"));

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        [ExpectedException(typeof(GraphCreationException))]
        public void ParseNodeNeighborsThrowsExceptionIfSameNodeListedTwice()
        {
            String graphString = "n1\tn2,n3" + "\n" +
                                 "n2\tn1,n3" + "\n" +
                                 "n3\tn2,n1" + "\n" +
                                 "n2\tn1,n3";
            Graph g = Graph.ParseGraphFromTsvNodeNeighborsString(graphString);
        }
        [TestMethod]
        public void AddEdgeConnectsTwoExistingNodes()
        {
            Graph g = new Graph();
            TestGraphs.Add(g);

            g.AddNode("n1");
            g.AddNode("n2");

            g.AddEdge("n1", "n2");

            Assert.IsTrue(g.AllNodes.First(n => n.Id == "n1").Neighbors.Any(n => n.Id == "n2") &&
                g.AllNodes.First(n => n.Id == "n2").Neighbors.Any(n => n.Id == "n1")
                );

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void AddEdgeAddsTwoNewNodes()
        {
            Graph g = new Graph();
            TestGraphs.Add(g);

            g.AddEdge("n1", "n2");

            Assert.IsTrue(g.AllNodes.First(n => n.Id == "n1").Neighbors.Any(n => n.Id == "n2") &&
                g.AllNodes.First(n => n.Id == "n2").Neighbors.Any(n => n.Id == "n1")
                );

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void AddEdgeAddsOneNewNode()
        {
            Graph g = new Graph();
            TestGraphs.Add(g);

            g.AddNode("n1");
            g.AddEdge("n1", "n2");

            Assert.IsTrue(g.AllNodes.First(n => n.Id == "n1").Neighbors.Any(n => n.Id == "n2") &&
                g.AllNodes.First(n => n.Id == "n2").Neighbors.Any(n => n.Id == "n1")
                );

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        // helper method, convert edges into string to make testing easier. Will be in the form of "n1,n2$n1,n3$n2,n4" etc.
        private string GetEdgeTuplesAsString(IEnumerable<Tuple<string, string>> edges)
        {
            return String.Join("$",
                edges.OrderBy(t => t.Item1).ThenBy(t => t.Item2).Select(t => t.Item1 + "," + t.Item2));
        }

        [TestMethod]
        public void EdgesPropertyIsCorrect()
        {
            Graph g = new Graph();
            TestGraphs.Add(g);

            g.AddEdge("n1", "n2");
            g.AddEdge("n2", "n3");
            g.AddEdge("n3", "n4");
            g.AddEdge("n4", "n2");

            Assert.AreEqual("n1,n2$n2,n3$n2,n4$n3,n4", GetEdgeTuplesAsString(g.Edges));
            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void EdgesPropertyIsCorrectAfterChanges()
        {
            Graph g = new Graph();
            TestGraphs.Add(g);

            g.AddEdge("n1", "n2");
            g.AddEdge("n2", "n3");
            g.AddEdge("n3", "n4");

            Assert.AreEqual("n1,n2$n2,n3$n3,n4",
                GetEdgeTuplesAsString(g.Edges));

            g.AddEdge("n4", "n5");

            Assert.AreEqual("n1,n2$n2,n3$n3,n4$n4,n5", GetEdgeTuplesAsString(g.Edges));

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        [ExpectedException(typeof(EmptyGraphException))]
        public void AddNodeFailsIfNoNeighbor()
        {
            Graph g = new Graph();
            g.AddNode("n1");
            var b = g.AllNodes.Any();
        }

        [TestMethod]
        public void AccessingEdgesThenNodesWorksForChanges()
        {
            Graph g = new Graph();
            TestGraphs.Add(g);

            g.AddEdge("n1", "n2");
            g.AddEdge("n2", "n3");
            var e = g.Edges;
            Assert.IsTrue(g.AllNodes.First(n => n.Id == "n1").Neighbors.Any(n => n.Id == "n2"),
                "Accessing edges, then nodes failed");

            TestGraphs.Add(g = new Graph());
            g.AddEdge("n1", "n2");
            g.AddEdge("n2", "n3");

            e = g.Edges;

            g.AddEdge("n3", "n4");
            Assert.IsTrue(g.AllNodes.First(n => n.Id == "n3").Neighbors.Any(n => n.Id == "n4"),
                "Accessing edges, changing, and accessing nodes failed");

            TestGraphs.Add(g = new Graph());

            g.AddEdge("n1", "n2");
            g.AddEdge("n2", "n3");

            e = g.Edges;
            var node = g.AllNodes.First();

            g.AddEdge("n3", "n4");
            Assert.IsTrue(g.AllNodes.First(n => n.Id == "n3").Neighbors.Any(n => n.Id == "n4"),
                "Accessing edges, then nodes, then changing, and then accessing nodes again failed");


            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void AccessingNodesThenEdgesWorksForChanges()
        {
            Graph g = new Graph();
            TestGraphs.Add(g);

            g.AddEdge("n1", "n2");
            g.AddEdge("n2", "n3");
            var node = g.AllNodes.First();
            Assert.IsTrue(g.Edges.Any(t => t.Item1 == "n1" && t.Item2 == "n2"),
                "Accessing nodes, then edges failed");

            TestGraphs.Add(g = new Graph());
            g.AddEdge("n1", "n2");
            g.AddEdge("n2", "n3");

            node = g.AllNodes.First();

            g.AddEdge("n3", "n4");
            Assert.IsTrue(g.Edges.Any(t => t.Item1 == "n3" && t.Item2 == "n4"),
                "Accessing nodes, then changing, then accessing edges failed");

            TestGraphs.Add(g = new Graph());

            g.AddEdge("n1", "n2");
            g.AddEdge("n2", "n3");

            node = g.AllNodes.First();
            var e = g.Edges;

            g.AddEdge("n3", "n4");
            Assert.IsTrue(g.Edges.Any(t => t.Item1 == "n3" && t.Item2 == "n4"),
                "Accessing nodes, then edges, then changing, and then accessing edges again failed");


            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void DegreeVectorsAreCorrect()
        {
            // n1 <--> n2 <--> n3
            Graph graph = Graph.ParseGraphFromTsvNodeNeighborsString(
                "n1\tn2" + "\n" +
                "n2\tn1,n3" + "\n",
                true);
            TestGraphs.Add(graph);
            Assert.AreEqual("1,1,2", String.Join(",", graph.DegreeVector));

            TestGraphs.Add(graph = Graph.ParseGraphFromTsvNodeNeighborsString(testGraphString1));
            Assert.AreEqual("1,1,1,1,1,1,1,1,3,3,6", String.Join(",", graph.DegreeVector));

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod] 
        public void DegreeVectorsAreCorrectAfterChanges()
        {
            // n1 <--> n2 <--> n3
            Graph graph = Graph.ParseGraphFromTsvNodeNeighborsString(
                "n1\tn2" + "\n" +
                "n2\tn1,n3" + "\n",
                true);
            TestGraphs.Add(graph);
            Assert.AreEqual("1,1,2", string.Join(",", graph.DegreeVector));
            graph.AddEdge("n2", "n4");
            Assert.AreEqual("1,1,1,3", string.Join(",", graph.DegreeVector));
            graph.AddEdge("n4", "n1");
            Assert.AreEqual("1,2,2,3", string.Join(",", graph.DegreeVector));


            TestGraphs.Add(graph = Graph.ParseGraphFromTsvNodeNeighborsString(testGraphString1));
            Assert.AreEqual("1,1,1,1,1,1,1,1,3,3,6", string.Join(",", graph.DegreeVector));
            graph.AddEdge("n5", "n11");
            Assert.AreEqual("1,1,1,1,1,1,1,2,3,4,6", string.Join(",", graph.DegreeVector));

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void FiVectorsAreCorrect()
        {
            // n1 <--> n2 <--> n3
            Graph graph = Graph.ParseGraphFromTsvNodeNeighborsString(
                "n1\tn2" + "\n" +
                "n2\tn1,n3" + "\n",
                true);
            TestGraphs.Add(graph);

            Assert.AreEqual("0.5,0.5,2.0", String.Join(",", graph.FiVector.Select(d => d.ToString("0.0"))));

            TestGraphs.Add(graph = Graph.ParseGraphFromTsvNodeNeighborsString(testGraphString1));
            Assert.AreEqual("0.167,0.167,0.167,0.167,0.167,0.333,0.333,0.333,0.900,1.800,4.500",
                String.Join(",", graph.FiVector.Select(d => d.ToString("0.000"))));

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }


        [TestMethod]
        public void FiVectorsAreCorrectAfterChanges()
        {
            // n1 <--> n2 <--> n3
            Graph graph = Graph.ParseGraphFromTsvNodeNeighborsString(
                "n1\tn2" + "\n" +
                "n2\tn1,n3" + "\n",
                true);
            TestGraphs.Add(graph);
            graph.AddEdge("n2", "n4");
            Assert.AreEqual("0.333,0.333,0.333,3.000", string.Join(",", graph.FiVector.Select(d => d.ToString("0.000"))));
            graph.AddEdge("n4", "n1");
            Assert.AreEqual("0.333,0.800,0.800,1.800", string.Join(",", graph.FiVector.Select(d => d.ToString("0.000"))));


            TestGraphs.Add(graph = Graph.ParseGraphFromTsvNodeNeighborsString(testGraphString1));
            graph.AddEdge("n5", "n11");
            Assert.AreEqual("0.167,0.167,0.167,0.167,0.250,0.333,0.333,0.400,1.333,1.500,3.600", string.Join(",", graph.FiVector.Select(d => d.ToString("0.000"))));

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }
    }
}
