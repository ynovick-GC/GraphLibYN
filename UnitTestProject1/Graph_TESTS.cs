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
        // For class invariants, any graph created should be added to this list
        private List<Graph> TestGraphs = new List<Graph>();

        private bool AllGraphsHaveNoZeroDegreeNodes()
        {
            return TestGraphs.All(g => g.AllNodes.All(n => n.Degree > 0));
        }

        // For testing doubles, set the tolerance here
        private static readonly double Tolerance = 0.000005;

        // A graph I dreamed up... (strange it didn't occur to me to add a cycle..)
        /****** GRAPH ******
         *    n1      n2
         *     \      /
         *      \    /    n7  n8
         *        n3      |  /
         *        |       | /
         *        n5------n6----n9
         *       /        | \
         *      /         |  \
         *    n4         n10  n11
         */

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

        [TestMethod]
        [ExpectedException((typeof(GraphException)))]
        public void RemoveEdgeThrowsExceptionOnNonExistentNode()
        {
            // So far I don't have a specific Exception type for this, just using the general one, may revisit that..
            Graph g = new Graph();
            g.AddEdge("n1", "n2");
            g.RemoveEdge("n1", "n3");
        }

        [TestMethod]
        public void RemoveEdgeRemovesCorrectly()
        {
            Graph g = new Graph();

            TestGraphs.Add(g);
            g.AddEdge("n1", "n2");
            g.AddEdge("n2", "n3");
            g.AddEdge("n3", "n1");

            Assert.IsTrue(g.AllNodes.All(n => n.Degree == 2));
            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());

            g.RemoveEdge("n3", "n1");
            var n1 = g.AllNodes.First(n => n.Id == "n1");
            var n3 = g.AllNodes.First(n => n.Id == "n3");
            Assert.IsFalse(n1.Neighbors.Contains(n3));
            Assert.IsFalse(n3.Neighbors.Contains(n1));
            Assert.AreEqual("1,1,2", String.Join(",", g.DegreeVector));

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void RemoveEdgesGivesCorrectReturnValue()
        {
            Graph g = new Graph();

            TestGraphs.Add(g);
            g.AddEdge("n1", "n2");
            g.AddEdge("n2", "n3");
            g.AddEdge("n3", "n1");

            Assert.IsTrue(g.AllNodes.All(n => n.Degree == 2));
            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());

            Assert.IsTrue(g.RemoveEdge("n3", "n1"));
            Assert.IsFalse(g.RemoveEdge("n3", "n1"));

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());

        }

        [TestMethod]
        public void RemoveEdgeResultsInCorrectEdgeTuples()
        {
            Graph g = new Graph();
            g.AddEdge("n1", "n2");
            g.AddEdge("n2", "n3");
            g.AddEdge("n3", "n1");

            Assert.AreEqual("n1,n2$n1,n3$n2,n3", GetEdgeTuplesAsString(g.Edges));
            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());

            g.RemoveEdge("n3", "n1");

            Assert.AreEqual("n1,n2$n2,n3", GetEdgeTuplesAsString(g.Edges));
            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());

        }

        [TestMethod]
        public void DegreeVectorIsCorrectBeforeAndAfterRemoveEdge()
        {
            Graph g = Graph.ParseGraphFromTsvNodeNeighborsString(testGraphString1);
            TestGraphs.Add(g);

            Assert.AreEqual("1,1,1,1,1,1,1,1,3,3,6", String.Join(",", g.DegreeVector));

            g.RemoveEdge("n5", "n3");

            Assert.AreEqual("1,1,1,1,1,1,1,1,2,2,6", String.Join(",", g.DegreeVector));

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

            Assert.AreEqual("0.5,2.0,2.0", String.Join(",", graph.FiVector.Select(d => d.ToString("0.0"))));

            TestGraphs.Add(graph = Graph.ParseGraphFromTsvNodeNeighborsString(testGraphString1));
            Assert.AreEqual("0.222,0.556,1.111,3.000,3.000,3.000,6.000,6.000,6.000,6.000,6.000",
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
            Assert.AreEqual("0.333,3.000,3.000,3.000", string.Join(",", graph.FiVector.Select(d => d.ToString("0.000"))));
            graph.AddEdge("n4", "n1");
            Assert.AreEqual("0.556,1.250,1.250,3.000", string.Join(",", graph.FiVector.Select(d => d.ToString("0.000"))));


            TestGraphs.Add(graph = Graph.ParseGraphFromTsvNodeNeighborsString(testGraphString1));
            graph.AddEdge("n5", "n11");
            Assert.AreEqual("0.278,0.667,0.750,2.500,3.000,3.000,4.000,6.000,6.000,6.000,6.000",
                string.Join(",", graph.FiVector.Select(d => d.ToString("0.000"))));

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void DegreeCountDictionaryIsCorrect()
        {
            var graph = Graph.ParseGraphFromTsvNodeNeighborsString(testGraphString1);
            TestGraphs.Add(graph);

            Assert.AreEqual(8, graph.DegreeCountDictionary[1]);
            Assert.AreEqual(2, graph.DegreeCountDictionary[3]);
            Assert.AreEqual(1, graph.DegreeCountDictionary[6]);

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void DegreeCountDictionaryIsCorrectAfterChanges()
        {
            var graph = Graph.ParseGraphFromTsvNodeNeighborsString(testGraphString1);
            TestGraphs.Add(graph);

            graph.AddEdge("n7", "n3");

            Assert.AreEqual(7, graph.DegreeCountDictionary[1]);
            Assert.AreEqual(1, graph.DegreeCountDictionary[2]);
            Assert.AreEqual(1, graph.DegreeCountDictionary[3]);
            Assert.AreEqual(1, graph.DegreeCountDictionary[4]);
            Assert.AreEqual(1, graph.DegreeCountDictionary[6]);

            graph.AddEdge("n8", "n12");

            Assert.AreEqual(7, graph.DegreeCountDictionary[1]);
            Assert.AreEqual(2, graph.DegreeCountDictionary[2]);
            Assert.AreEqual(1, graph.DegreeCountDictionary[3]);
            Assert.AreEqual(1, graph.DegreeCountDictionary[4]);
            Assert.AreEqual(1, graph.DegreeCountDictionary[6]);

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void NodeDegreeProbabilityDictionaryIsCorrect()
        {
            Graph g = Graph.ParseGraphFromTsvNodeNeighborsString(testGraphString1);
            TestGraphs.Add(g);

            Assert.AreEqual(8.0/11.0, g.GetNodeDegreeProbability(1), Tolerance);
            Assert.AreEqual(2.0/11.0, g.GetNodeDegreeProbability(3), Tolerance);
            Assert.AreEqual(1.0/11.0, g.GetNodeDegreeProbability(6), Tolerance);

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void NodeDegreeProbabilityIsZeroForNonExistentNodeDegree()
        {
            Graph g = Graph.ParseGraphFromTsvNodeNeighborsString(testGraphString1);
            TestGraphs.Add(g);

            Assert.AreEqual(0.0, g.GetNodeDegreeProbability(2), Tolerance);
            Assert.AreEqual(0.0, g.GetNodeDegreeProbability(8), Tolerance);

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void NodeDegreeProbabilityIsCorrectAfterChanges()
        {
            Graph g = Graph.ParseGraphFromTsvNodeNeighborsString(testGraphString1);
            TestGraphs.Add(g);

            Assert.AreEqual(8.0/11.0, g.GetNodeDegreeProbability(1), Tolerance);
            Assert.AreEqual(2.0/11.0, g.GetNodeDegreeProbability(3), Tolerance);
            Assert.AreEqual(1.0/11.0, g.GetNodeDegreeProbability(6), Tolerance);
            Assert.AreEqual(0.0, g.GetNodeDegreeProbability(2), Tolerance);
            Assert.AreEqual(0.0, g.GetNodeDegreeProbability(8), Tolerance);

            g.AddEdge("n3", "n7");

            Assert.AreEqual(7.0/11.0, g.GetNodeDegreeProbability(1), Tolerance);
            Assert.AreEqual(1.0/11.0, g.GetNodeDegreeProbability(2), Tolerance);
            Assert.AreEqual(1.0/11.0, g.GetNodeDegreeProbability(3), Tolerance);
            Assert.AreEqual(1.0/11.0, g.GetNodeDegreeProbability(4), Tolerance);
            Assert.AreEqual(1.0/11.0, g.GetNodeDegreeProbability(6), Tolerance);
            Assert.AreEqual(0.0, g.GetNodeDegreeProbability(7), Tolerance);
            Assert.AreEqual(0.0, g.GetNodeDegreeProbability(8), Tolerance);

            g.AddEdge("n8", "n12", true);

            Assert.AreEqual(7.0/12.0, g.GetNodeDegreeProbability(1), Tolerance);
            Assert.AreEqual(2.0/12.0, g.GetNodeDegreeProbability(2), Tolerance);
            Assert.AreEqual(1.0/12.0, g.GetNodeDegreeProbability(3), Tolerance);
            Assert.AreEqual(1.0/12.0, g.GetNodeDegreeProbability(4), Tolerance);
            Assert.AreEqual(1.0/12.0, g.GetNodeDegreeProbability(6), Tolerance);
            Assert.AreEqual(0.0, g.GetNodeDegreeProbability(7), Tolerance);
            Assert.AreEqual(0.0, g.GetNodeDegreeProbability(8), Tolerance);

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void EdgeNodeDegreeProbabilityIsCorrect()
        {
            Graph g = Graph.ParseGraphFromTsvNodeNeighborsString(testGraphString1);
            TestGraphs.Add(g);

            Assert.AreEqual(8.0/20.0, g.GetEdgeNodeDegreeProbability(1), Tolerance);
            Assert.AreEqual(6.0/20.0, g.GetEdgeNodeDegreeProbability(3), Tolerance);
            Assert.AreEqual(6.0/20.0, g.GetEdgeNodeDegreeProbability(6), Tolerance);

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void EdgeNodeDegreeProbabilityIsZeroForNonExistentNodeDegree()
        {
            Graph g = Graph.ParseGraphFromTsvNodeNeighborsString(testGraphString1);
            TestGraphs.Add(g);

            Assert.AreEqual(0.0, g.GetEdgeNodeDegreeProbability(2), Tolerance);
            Assert.AreEqual(0.0, g.GetEdgeNodeDegreeProbability(8), Tolerance);

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void EdgeNodeDegreeProbabilityIsCorrectAfterChanges()
        {
            Graph g = Graph.ParseGraphFromTsvNodeNeighborsString(testGraphString1);
            TestGraphs.Add(g);

            Assert.AreEqual(8.0/20.0, g.GetEdgeNodeDegreeProbability(1), Tolerance);
            Assert.AreEqual(6.0/20.0, g.GetEdgeNodeDegreeProbability(3), Tolerance);
            Assert.AreEqual(6.0/20.0, g.GetEdgeNodeDegreeProbability(6), Tolerance);
            Assert.AreEqual(0.0, g.GetEdgeNodeDegreeProbability(2), Tolerance);
            Assert.AreEqual(0.0, g.GetEdgeNodeDegreeProbability(8), Tolerance);

            g.AddEdge("n3", "n7");

            Assert.AreEqual(7.0/22.0, g.GetEdgeNodeDegreeProbability(1), Tolerance);
            Assert.AreEqual(2.0/22.0, g.GetEdgeNodeDegreeProbability(2), Tolerance);
            Assert.AreEqual(3.0/22.0, g.GetEdgeNodeDegreeProbability(3), Tolerance);
            Assert.AreEqual(4.0/22.0, g.GetEdgeNodeDegreeProbability(4), Tolerance);
            Assert.AreEqual(6.0/22.0, g.GetEdgeNodeDegreeProbability(6), Tolerance);
            Assert.AreEqual(0.0, g.GetEdgeNodeDegreeProbability(7), Tolerance);
            Assert.AreEqual(0.0, g.GetEdgeNodeDegreeProbability(8), Tolerance);

            g.AddEdge("n8", "n12", true);

            Assert.AreEqual(7.0/24.0, g.GetEdgeNodeDegreeProbability(1), Tolerance);
            Assert.AreEqual(4.0/24.0, g.GetEdgeNodeDegreeProbability(2), Tolerance);
            Assert.AreEqual(3.0/24.0, g.GetEdgeNodeDegreeProbability(3), Tolerance);
            Assert.AreEqual(4.0/24.0, g.GetEdgeNodeDegreeProbability(4), Tolerance);
            Assert.AreEqual(6.0/24.0, g.GetEdgeNodeDegreeProbability(6), Tolerance);
            Assert.AreEqual(0.0, g.GetEdgeNodeDegreeProbability(7), Tolerance);
            Assert.AreEqual(0.0, g.GetEdgeNodeDegreeProbability(8), Tolerance);

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void EdgeTuplesOfErGraphMatchForEdgeNodeProbability()
        {
            Randomizer r = new Randomizer();
            ErGraph graph = new ErGraph(100, 0.15, r);

            Dictionary<int, double> erGraphEdgeNodeProbability =
                graph.Edges.SelectMany(t => new List<Node>
                {
                    graph.AllNodes.First(n => n.Id == t.Item1),
                    graph.AllNodes.First(n => n.Id == t.Item2)
                }).GroupBy(n => n.Degree).ToDictionary(g => g.Key, g => (double) g.Count()/(graph.Edges.Count()*2));

            Assert.IsTrue(
                erGraphEdgeNodeProbability.All(
                    kvp => Math.Abs(kvp.Value - graph.GetEdgeNodeDegreeProbability(kvp.Key)) < Tolerance));

        }

        [TestMethod]
        public void JointProbabilityOfEdgeExcessDegreeIsCorrect()
        {
            Graph g = Graph.ParseGraphFromTsvNodeNeighborsString(testGraphString1);
            TestGraphs.Add(g);

            Assert.AreEqual(0.05, g.GetEdgeExcessNodeDegreeProbability(2, 5), Tolerance);
            Assert.AreEqual(0.25, g.GetEdgeExcessNodeDegreeProbability(0, 5), Tolerance);

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void JointProbabilityOfEdgeExcessDegreeIsZeroForNonExistentNodes()
        {
            Graph g = Graph.ParseGraphFromTsvNodeNeighborsString(testGraphString1);
            TestGraphs.Add(g);

            Assert.AreEqual(0.0, g.GetEdgeExcessNodeDegreeProbability(5, 5), Tolerance);
            Assert.AreEqual(0.0, g.GetEdgeExcessNodeDegreeProbability(4, 3), Tolerance);
            Assert.AreEqual(0.0, g.GetEdgeExcessNodeDegreeProbability(4, 0), Tolerance);

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void JointProbabilityOfEdgeExcessDegreeIsSymetric()
        {
            Graph g = Graph.ParseGraphFromTsvNodeNeighborsString(testGraphString1);
            TestGraphs.Add(g);

            for (int i = 0; i < 7; i++)
                for (int j = 0; j < 7; j++)
                    Assert.IsTrue(Math.Abs(g.GetEdgeExcessNodeDegreeProbability(i, j) -
                                           g.GetEdgeExcessNodeDegreeProbability(j, i)) < Tolerance);

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void JointProbabilityOfEdgeExcessDegreeIsCorrectBeforeAndAfterChanges()
        {
            Graph g = Graph.ParseGraphFromTsvNodeNeighborsString(testGraphString1);
            TestGraphs.Add(g);

            Assert.AreEqual(0.05, g.GetEdgeExcessNodeDegreeProbability(2, 5), Tolerance);
            Assert.AreEqual(0.25, g.GetEdgeExcessNodeDegreeProbability(0, 5), Tolerance);

            g.AddEdge("n3", "n7");
            g.AddEdge("n12", "n8", true);

            Assert.AreEqual(0.041666666666, g.GetEdgeExcessNodeDegreeProbability(1, 0), Tolerance);
            Assert.AreEqual(0.083333333333, g.GetEdgeExcessNodeDegreeProbability(5, 1), Tolerance);
            Assert.AreEqual(0.041666666666, g.GetEdgeExcessNodeDegreeProbability(0, 1), Tolerance);
            Assert.AreEqual(0.083333333333, g.GetEdgeExcessNodeDegreeProbability(1, 5), Tolerance);

            Assert.AreEqual(0.0, g.GetEdgeExcessNodeDegreeProbability(3, 4), Tolerance);
            Assert.AreEqual(0.0, g.GetEdgeExcessNodeDegreeProbability(15, 4), Tolerance);

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void JointProbabilityOfEdgeExcessDegreeFollowsProbabilityRules()
        {
            Graph g = new ErGraph(100, 0.15, new Randomizer());
            TestGraphs.Add(g);

            double total = 0;
            for (int i = 0; i <= 100; i++)
                for (int j = 0; j <= 100; j++)
                    total += g.GetEdgeExcessNodeDegreeProbability(i, j);

            Assert.AreEqual(1.0, total, Tolerance);

            for (int i = 0; i <= 100; i++)
            {
                total = 0;
                for (int j = 0; j <= 100; j++)
                    total += g.GetEdgeExcessNodeDegreeProbability(i, j);
                Assert.AreEqual(total, g.GetEdgeExcessNodeDegreeProbability(i), Tolerance);
            }
            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void ExcessDegreeVarianceIsCorrect()
        {
            Graph g = Graph.ParseGraphFromTsvNodeNeighborsString(testGraphString1);
            TestGraphs.Add(g);

            Assert.AreEqual(4.29, g.ExcessDegreeVariance, Tolerance);

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void ExcessDegreeVarianceIsCorrectBeforeAndAfterChanges()
        {
            Graph g = Graph.ParseGraphFromTsvNodeNeighborsString(testGraphString1);
            TestGraphs.Add(g);

            Assert.AreEqual(4.29, g.ExcessDegreeVariance, Tolerance);

            g.AddEdge("n3", "n7");
            g.AddEdge("n12", "n8", true);

            Assert.AreEqual(3.722222222222, g.ExcessDegreeVariance, Tolerance);
            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void GraphAssortativityIsCorrect()
        {
            Graph g = Graph.ParseGraphFromTsvNodeNeighborsString(testGraphString1);
            TestGraphs.Add(g);

            Assert.AreEqual(-0.701631702, g.GraphAssortativity, Tolerance);
            Assert.AreEqual(-0.701631702, g.GraphAssortativity2(), Tolerance);
            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void GraphAssortativityIsCorrectBeforeAndAfterChanges()
        {
            Graph g = Graph.ParseGraphFromTsvNodeNeighborsString(testGraphString1);
            TestGraphs.Add(g);

            Assert.AreEqual(-0.701631702, g.GraphAssortativity, Tolerance);
            Assert.AreEqual(-0.701631702, g.GraphAssortativity2(), Tolerance);

            g.AddEdge("n3", "n7");
            g.AddEdge("n12", "n8", true);

            Assert.AreEqual(-0.611940299, g.GraphAssortativity, Tolerance);
            Assert.AreEqual(-0.611940299, g.GraphAssortativity2(), Tolerance);
            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());            
        }

        [TestMethod]
        public void AssortativityOfCompleteGraphIsOne()
        {
            Graph g = new Graph();
            g.AddEdge("n1", "n2");
            g.AddEdge("n2", "n3");
            g.AddEdge("n3", "n4");
            g.AddEdge("n1", "n3");
            g.AddEdge("n1", "n4");
            g.AddEdge("n2", "n4");

            Assert.AreEqual(1.0, g.GraphAssortativity, Tolerance);
            Assert.AreEqual(1.0, g.GraphAssortativity2(), Tolerance);
        }

        [TestMethod]
        public void AssortativityOfStarIsNegativeOne()
        {
            Graph g = new Graph();
            g.AddEdge("n1", "n2");
            g.AddEdge("n1", "n2");
            g.AddEdge("n1", "n3");
            g.AddEdge("n1", "n4");
            g.AddEdge("n1", "n5");
            g.AddEdge("n1", "n6");

            Assert.AreEqual(-1.0, g.GraphAssortativity, Tolerance);
            Assert.AreEqual(-1.0, g.GraphAssortativity2(), Tolerance);
        }

        [TestMethod]
        public void GraphAssortativityOfRandomGraphsIsInRange()
        {
            Randomizer rand = new Randomizer();

            for (int i = 0; i < 10; i++)
            {
                ErGraph erg = new ErGraph((int)(rand.NextDouble()*300 + 20), rand.NextDouble()*0.4, new Randomizer(new Random(rand.Next())));
                TestGraphs.Add(erg);
                Assert.IsTrue(-1 <= erg.GraphAssortativity && erg.GraphAssortativity <= 1);
                Assert.IsTrue(-1 <= erg.GraphAssortativity2() && erg.GraphAssortativity2() <= 1);
                Assert.IsTrue(Math.Abs(erg.GraphAssortativity - erg.GraphAssortativity2())<Tolerance);

                BaGraph bag = new BaGraph((int)(rand.NextDouble()*20+2),new Randomizer(new Random(rand.Next())),(int)(rand.NextDouble()*800 + 100));
                TestGraphs.Add(bag);
                Assert.IsTrue(-1 <= bag.GraphAssortativity && bag.GraphAssortativity <= 1);
                Assert.IsTrue(-1 <= bag.GraphAssortativity2() && bag.GraphAssortativity2() <= 1);
                Assert.IsTrue(Math.Abs(bag.GraphAssortativity - bag.GraphAssortativity2()) < Tolerance);
            }
            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void IterativeSwappingMaintainsDegreeVector()
        {
            Randomizer rand = new Randomizer();
            
            // Random graphs
            for (int i = 0; i < 10; i++)
            {
                ErGraph erg = new ErGraph((int)(rand.NextDouble() * 300 + 20), rand.NextDouble() * 0.4, new Randomizer(new Random(rand.Next())));
                TestGraphs.Add(erg);
                string prevVector = String.Join(",", erg.DegreeVector);
                erg.IterativelySwapEdgesToIncreaseAssortativity();
                Assert.AreEqual(prevVector, String.Join(",", erg.DegreeVector));

                BaGraph bag = new BaGraph((int)(rand.NextDouble() * 20 + 2), new Randomizer(new Random(rand.Next())), (int)(rand.NextDouble() * 800 + 100));
                TestGraphs.Add(bag);
                prevVector = String.Join(",", erg.DegreeVector);
                erg.IterativelySwapEdgesToIncreaseAssortativity();
                Assert.AreEqual(prevVector, String.Join(",", erg.DegreeVector));
            }
            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void ClonedGraphHasSameDegreeVector()
        {
            Randomizer r = new Randomizer();
            Graph g = new ErGraph(50, .01, r);

            Graph g2 = g.Clone();

            Assert.AreEqual(String.Join(",", g.DegreeVector), string.Join(",", g2.DegreeVector));
            foreach (var edge in g.Edges)
            {
                Assert.IsTrue(g2.Edges.Any(e => e.Item1 == edge.Item1 && e.Item2 == edge.Item2));
            }


        }
    }
}
