using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphLibyn;
using GraphLibyn.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    // This class is specifically to unit test protected methods that can't be exposed easily through a TestGraph : Graph class
    // so have this whole TestClass inherit from Graph and use it itself. But only use to directly test the behavior of the
    // protected 
    [TestClass]
    class GraphProtectedMethods_TESTS : Graph
    {      
         
        // For class invariants, any graph created should be added to this list
        private List<Graph> TestGraphs = new List<Graph>();

        private bool AllGraphsHaveNoZeroDegreeNodes()
        {
            return TestGraphs.All(g => g.AllNodes.All(n => n.Degree > 0));
        }

        [TestMethod]
        public void ProtectedAddEdgeAddsEdgeToExistingNodes()
        {
           
            GraphProtectedMethods_TESTS g = new GraphProtectedMethods_TESTS();
            TestGraphs.Add(g);

            g.AddEdge("n1", "n2");
            g.AddEdge("n2", "n3");
            
            g.AddEdge(g.AllNodes.First(n => n.Id == "n1"), g.AllNodes.First(n => n.Id == "n3"));
            Assert.IsTrue(g.AllNodes.First(n => n.Id == "n3").Neighbors.Any(n => n.Id == "n1"));

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void ProtectedAddEdgeAddsNodesToGraph()
        {
            GraphProtectedMethods_TESTS g = new GraphProtectedMethods_TESTS();
            TestGraphs.Add(g);

            g.AddEdge("n1", "n2");
            g.AddEdge("n2", "n3");

            Node n4 = new Node("n4");
            g.AddEdge(n4, g.AllNodes.First(n => n.Id == "n2"), true);
            Assert.IsTrue(g.AllNodes.First(n => n.Id == "n2").Neighbors.Any(n => n.Id == "n4"),
                "Connecting one existing node with one new one failed.");

            Node n5 = new Node("n5");
            Node n6 = new Node("n6");
            g.AddEdge(n5, n6, true);
            Assert.IsTrue(g.AllNodes.First(n => n.Id == "n5").Neighbors.Any(n => n.Id == "n6"),
                "Connecting two new nodes and adding to graph failed.");

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        [ExpectedException(typeof(GraphCreationException))]
        public void ProtectedAddEdgeThrowsExceptionIfIdExists()
        {
            GraphProtectedMethods_TESTS g = new GraphProtectedMethods_TESTS();

            g.AddEdge("n1", "n2");
            g.AddEdge("n2", "n3");

            Node n1 = new Node("n1");
            g.AddEdge(n1, g.AllNodes.First(n => n.Id == "n3"), true);
        }

        [TestMethod]
        [ExpectedException(typeof(GraphCreationException))]
        public void ProtectedAddEdgeThrowsExceptionWhenAddingNodesAndAddToGraphIsFalse()
        {
            GraphProtectedMethods_TESTS g = new GraphProtectedMethods_TESTS();

            g.AddEdge("n1", "n2");
            g.AddEdge("n2", "n3");

            GraphNode n4 = NewGraphNode("n4");
            g.AddEdge(n4, g.AllNodes.First(n => n.Id == "n3"), false);
        }

        [TestMethod]
        public void ProtectedRemoveEdgeRemovesCorrectly()
        {
            GraphProtectedMethods_TESTS graph = new GraphProtectedMethods_TESTS();
            TestGraphs.Add(graph);
            
            graph.AddEdge("n1", "n2", true);
            graph.AddEdge("n2", "n3", true);
            graph.AddEdge("n1", "n3");

            Assert.IsTrue(graph.AllNodes.All(n => n.Degree == 2));

            Node n1 = graph.AllNodes.First(n => n.Id == "n1");
            Node n2 = graph.AllNodes.First(n => n.Id == "n2");
            Node n3 = graph.AllNodes.First(n => n.Id == "n3");

            graph.RemoveEdge(n1, n3);

            Assert.AreEqual("1,1,2", String.Join(",", graph.AllNodes.Select(n => n.Degree).OrderBy(n => n)));
            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void ProtectedRemoveEdgeReturnsCorrectValue()
        {
            GraphProtectedMethods_TESTS graph = new GraphProtectedMethods_TESTS();
            TestGraphs.Add(graph);

            Node n1 = graph.NewGraphNode("n1");
            Node n2 = graph.NewGraphNode("n2");

            graph.AddEdge(n1, n2);

            Assert.IsTrue(graph.AllNodes.All(n => n.Degree == 1));

            Assert.IsTrue(graph.RemoveEdge(n1, n2));
            Assert.IsFalse(graph.RemoveEdge(n1, n2));

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void ProtectedEdgesPropertyIsCorrect()
        {
            GraphProtectedMethods_TESTS g = new GraphProtectedMethods_TESTS();
            TestGraphs.Add(g);

            g.AddEdge("n1", "n2");
            g.AddEdge("n2", "n3");
            g.AddEdge("n3", "n4");
            g.AddEdge("n4", "n2");

            Assert.IsTrue(g.EdgesAsNodes.Any(t => t.Item1.Id == "n1" && t.Item2.Id == "n2"));
            Assert.IsTrue(g.EdgesAsNodes.Any(t => t.Item1.Id == "n2" && t.Item2.Id == "n3"));
            Assert.IsTrue(g.EdgesAsNodes.Any(t => t.Item1.Id == "n3" && t.Item2.Id == "n4"));
            Assert.IsTrue(g.EdgesAsNodes.Any(t => t.Item1.Id == "n2" && t.Item2.Id == "n4"));

            Assert.IsFalse(
                g.EdgesAsNodes.Any(
                    t => (t.Item1.Id == "n1" && t.Item2.Id == "n3") || (t.Item1.Id == "n3" && t.Item2.Id == "n1")));
            Assert.IsFalse(
                g.EdgesAsNodes.Any(
                    t => (t.Item1.Id == "n1" && t.Item2.Id == "n4") || (t.Item1.Id == "n4" && t.Item2.Id == "n1")));

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }

        [TestMethod]
        public void ProtectedEdgesPropertyIsCorrectAfterChanges()
        {
            GraphProtectedMethods_TESTS g = new GraphProtectedMethods_TESTS();
            TestGraphs.Add(g);

            g.AddEdge("n1", "n2");
            g.AddEdge("n2", "n3");
            g.AddEdge("n3", "n4");

            Assert.IsTrue(g.EdgesAsNodes.Any(t => t.Item1.Id == "n1" && t.Item2.Id == "n2"));
            Assert.IsTrue(g.EdgesAsNodes.Any(t => t.Item1.Id == "n2" && t.Item2.Id == "n3"));
            Assert.IsTrue(g.EdgesAsNodes.Any(t => t.Item1.Id == "n3" && t.Item2.Id == "n4"));
            Assert.IsFalse(
                g.EdgesAsNodes.Any(
                    t => (t.Item1.Id == "n1" && t.Item2.Id == "n3") || (t.Item1.Id == "n3" && t.Item2.Id == "n1")));
            Assert.IsFalse(
                g.EdgesAsNodes.Any(
                    t => (t.Item1.Id == "n1" && t.Item2.Id == "n4") || (t.Item1.Id == "n4" && t.Item2.Id == "n1")));
            Assert.IsFalse(
                g.EdgesAsNodes.Any(
                    t => (t.Item1.Id == "n2" && t.Item2.Id == "n4") || (t.Item1.Id == "n4" && t.Item2.Id == "n2")));

            g.AddEdge("n4", "n5");

            Assert.IsTrue(g.EdgesAsNodes.Any(t => t.Item1.Id == "n1" && t.Item2.Id == "n2"));
            Assert.IsTrue(g.EdgesAsNodes.Any(t => t.Item1.Id == "n2" && t.Item2.Id == "n3"));
            Assert.IsTrue(g.EdgesAsNodes.Any(t => t.Item1.Id == "n3" && t.Item2.Id == "n4"));
            Assert.IsTrue(g.EdgesAsNodes.Any(t => t.Item1.Id == "n4" && t.Item2.Id == "n5"));

            Assert.IsFalse(
                g.EdgesAsNodes.Any(
                    t => (t.Item1.Id == "n1" && t.Item2.Id == "n3") || (t.Item1.Id == "n3" && t.Item2.Id == "n1")));
            Assert.IsFalse(
                g.EdgesAsNodes.Any(
                    t => (t.Item1.Id == "n1" && t.Item2.Id == "n4") || (t.Item1.Id == "n4" && t.Item2.Id == "n1")));
            Assert.IsFalse(
                g.EdgesAsNodes.Any(
                    t => (t.Item1.Id == "n2" && t.Item2.Id == "n4") || (t.Item1.Id == "n4" && t.Item2.Id == "n2")));
            Assert.IsFalse(
                g.EdgesAsNodes.Any(
                    t => (t.Item1.Id == "n1" && t.Item2.Id == "n5") || (t.Item1.Id == "n5" && t.Item2.Id == "n1")));
            Assert.IsFalse(
                g.EdgesAsNodes.Any(
                    t => (t.Item1.Id == "n2" && t.Item2.Id == "n5") || (t.Item1.Id == "n5" && t.Item2.Id == "n2")));

            Assert.IsTrue(AllGraphsHaveNoZeroDegreeNodes());
        }
    }
}
