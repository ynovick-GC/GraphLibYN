using System;
using System.Linq;
using System.Text;
using GraphLibyn;
using GraphLibyn.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class Node_TESTS
    {
        // Just a test wrapper class to expose AddNeighbor for testing
        private class TestNode : Node
        {
            public TestNode(string id, Graph g) : base(id, g)
            {
            }

            // Need these wrappers just to test the methods themselves, shouldn't be used in other contexts
            public new bool AddNeighbor(Node n) => base.AddNeighbor(n);
            public new bool RemoveNeighbor(Node n) => base.RemoveNeighbor(n);

            public new Graph Graph => base.Graph;
        }

        private TestNode GNDegreeZero;
        private TestNode GNDegreeOne;
        private TestNode GNDegreeFour;
        private TestNode GNodeWith3NeighborsDegrees1_3_6;

        // For testing doubles, set the tolerance here
        private static readonly double Tolerance = 0.000005;

        [TestInitialize]
        public void Setup()
        {
            GNDegreeZero = new TestNode("Degree0", new Graph());

            GNDegreeOne = new TestNode("Degree1", new Graph());
            GNDegreeOne.AddNeighbor(new Node("Degree1_Neighbor1", GNDegreeOne.Graph));

            GNDegreeFour = new TestNode("Degree4", new Graph());
            GNDegreeFour.AddNeighbor(new TestNode("Degree4_Neighbor1", GNDegreeFour.Graph));
            GNDegreeFour.AddNeighbor(new TestNode("Degree4_Neighbor2", GNDegreeFour.Graph));
            GNDegreeFour.AddNeighbor(new TestNode("Degree4_Neighbor3", GNDegreeFour.Graph));
            GNDegreeFour.AddNeighbor(new TestNode("Degree4_Neighbor4", GNDegreeFour.Graph));

            GNodeWith3NeighborsDegrees1_3_6 = new TestNode("GnodeWithNDegrees1-3-6", new Graph());
            TestNode OneDegreeNeighbor = new TestNode("1 Degree Neighbor", GNodeWith3NeighborsDegrees1_3_6.Graph);
            OneDegreeNeighbor.AddNeighbor(GNodeWith3NeighborsDegrees1_3_6);
            TestNode ThreeDegreeNode = new TestNode("3 degree neighbor", GNodeWith3NeighborsDegrees1_3_6.Graph);
            ThreeDegreeNode.AddNeighbor(GNodeWith3NeighborsDegrees1_3_6);
            ThreeDegreeNode.AddNeighbor(new TestNode("3 degree neighbor n2", GNodeWith3NeighborsDegrees1_3_6.Graph));
            ThreeDegreeNode.AddNeighbor(new TestNode("3 degree neighbor n3", GNodeWith3NeighborsDegrees1_3_6.Graph));
            TestNode SixDegreeNeighbor = new TestNode("6 degree neighbor", GNodeWith3NeighborsDegrees1_3_6.Graph);
            SixDegreeNeighbor.AddNeighbor(GNodeWith3NeighborsDegrees1_3_6);
            SixDegreeNeighbor.AddNeighbor(new TestNode("6 degree neighbor n2", GNodeWith3NeighborsDegrees1_3_6.Graph));
            SixDegreeNeighbor.AddNeighbor(new TestNode("6 degree neighbor n3", GNodeWith3NeighborsDegrees1_3_6.Graph));
            SixDegreeNeighbor.AddNeighbor(new TestNode("6 degree neighbor n4", GNodeWith3NeighborsDegrees1_3_6.Graph));
            SixDegreeNeighbor.AddNeighbor(new TestNode("6 degree neighbor n5", GNodeWith3NeighborsDegrees1_3_6.Graph));
            SixDegreeNeighbor.AddNeighbor(new TestNode("6 degree neighbor n6", GNodeWith3NeighborsDegrees1_3_6.Graph));
        }

        [TestMethod]
        [ExpectedException(typeof(NoSelfLoopsException))]
        public void TestNoSelfLoops()
        {
            TestNode n = new TestNode("Id", new Graph());
            n.AddNeighbor(n);
        }

        [TestMethod]
        [ExpectedException(typeof(GraphCreationException))]
        public void AddingNeighborFromDifferentGraphThrowsException()
        {
            TestNode n1 = new TestNode("n1", new Graph()), n2 = new TestNode("n2", new Graph());
            n1.AddNeighbor(n2);
        }

        [TestMethod]
        public void NeighborsIffAdded()
        {
            TestNode n1 = new TestNode("n1", new Graph());
            TestNode n2 = new TestNode("n2", n1.Graph);

            Assert.IsFalse(n1.Neighbors.Contains(n1));
            Assert.IsFalse(n1.Neighbors.Contains(n2));
            Assert.IsFalse(n2.Neighbors.Contains(n2));
            Assert.IsFalse(n2.Neighbors.Contains(n1));

            n1.AddNeighbor(n2);

            Assert.IsFalse(n1.Neighbors.Contains(n1));
            Assert.IsTrue(n1.Neighbors.Contains(n2));
            Assert.IsFalse(n2.Neighbors.Contains(n2));
            Assert.IsTrue(n2.Neighbors.Contains(n1));

            // second add doesn't break anything
            n1.AddNeighbor(n2);

            Assert.IsFalse(n1.Neighbors.Contains(n1));
            Assert.IsTrue(n1.Neighbors.Contains(n2));
            Assert.IsFalse(n2.Neighbors.Contains(n2));
            Assert.IsTrue(n2.Neighbors.Contains(n1));

            // adding a different node won't break the first connection
            n2.AddNeighbor(new Node("n3", n1.Graph));

            Assert.IsFalse(n1.Neighbors.Contains(n1));
            Assert.IsTrue(n1.Neighbors.Contains(n2));
            Assert.IsFalse(n2.Neighbors.Contains(n2));
            Assert.IsTrue(n2.Neighbors.Contains(n1));

        }

        [TestMethod]
        public void AddNeighborReturnsCorrectValue()
        {
            TestNode n1 = new TestNode("n1", new Graph());
            TestNode n2 = new TestNode("n2", n1.Graph);

            Assert.IsTrue(n1.AddNeighbor(n2));
            Assert.IsFalse(n1.AddNeighbor(n2));
            Assert.IsFalse(n2.AddNeighbor(n1));
        }

        [TestMethod]
        public void RemoveNeighborWorksCorrectly()
        {
            TestNode n1 = new TestNode("n1", new Graph());
            TestNode n2 = new TestNode("n2", n1.Graph);
            n1.AddNeighbor(n2);
            Assert.IsTrue(n1.Neighbors.Contains(n2));
            Assert.IsTrue(n2.Neighbors.Contains(n1));
            n1.RemoveNeighbor(n2);
            Assert.IsFalse(n1.Neighbors.Contains(n2));
            Assert.IsFalse(n2.Neighbors.Contains(n1));
            n2.RemoveNeighbor(n1);
            Assert.IsFalse(n1.Neighbors.Contains(n2));
            Assert.IsFalse(n2.Neighbors.Contains(n1));
        }

        [TestMethod]
        public void RemoveNeighborReturnsCorrectValue()
        {
            TestNode n1 = new TestNode("n1", new Graph());
            TestNode n2 = new TestNode("n2", n1.Graph);
            n1.AddNeighbor(n2);
            Assert.IsTrue(n1.RemoveNeighbor(n2));
            Assert.IsFalse(n2.RemoveNeighbor(n1));
        }

        [TestMethod]
        public void NeighborsMaintainedAfterAdditionalAdds()
        {
            Graph g = new Graph();
            TestNode n1 = new TestNode("n1", g);
            TestNode n2 = new TestNode("n2", g);

            n1.AddNeighbor(n2);

            Assert.IsFalse(n1.Neighbors.Contains(n1));
            Assert.IsTrue(n1.Neighbors.Contains(n2));
            Assert.IsFalse(n2.Neighbors.Contains(n2));
            Assert.IsTrue(n2.Neighbors.Contains(n1));

            n1.AddNeighbor(n2);

            Assert.IsFalse(n1.Neighbors.Contains(n1));
            Assert.IsTrue(n1.Neighbors.Contains(n2));
            Assert.IsFalse(n2.Neighbors.Contains(n2));
            Assert.IsTrue(n2.Neighbors.Contains(n1));

            n2.AddNeighbor(new Node("n3", g));

            Assert.IsFalse(n1.Neighbors.Contains(n1));
            Assert.IsTrue(n1.Neighbors.Contains(n2));
            Assert.IsFalse(n2.Neighbors.Contains(n2));
            Assert.IsTrue(n2.Neighbors.Contains(n1));

        }

        [TestMethod]
        public void DegreeOfTestNodesIsCorrect()
        {
            Assert.AreEqual(0, GNDegreeZero.Degree, "Degree 0 node doesn't have degree 0");
            Assert.AreEqual(1, GNDegreeOne.Degree, "Degree 1 node doesn't have degree 1");
            Assert.AreEqual(4, GNDegreeFour.Degree, "Degree 4 node doesn't have degree 4");
        }

        [TestMethod]
        public void DegreeOfTestNodesChangeCorrectly()
        {
            TestNode newNode = new TestNode("new node", GNDegreeOne.Graph);
            newNode.AddNeighbor(GNDegreeOne);
            Assert.AreEqual(1, newNode.Degree);
            Assert.AreEqual(2, GNDegreeOne.Degree);

            newNode.AddNeighbor(GNDegreeOne);
            Assert.AreEqual(1, newNode.Degree);
            Assert.AreEqual(2, GNDegreeOne.Degree);
        }

        [TestMethod]
        public void DegreeDoesntChangeFromFailedSelfLoop()
        {
            try
            {
                GNDegreeZero.AddNeighbor(GNDegreeZero);
            }
            catch (Exception)
            {
            }

            try
            {
                GNDegreeZero.AddNeighbor(GNDegreeZero);
            }
            catch (Exception)
            {
            }

            try
            {
                GNDegreeZero.AddNeighbor(GNDegreeZero);
            }
            catch (Exception)
            {
            }

            Assert.AreEqual(0, GNDegreeZero.Degree);
            Assert.AreEqual(1, GNDegreeOne.Degree);
            Assert.AreEqual(4, GNDegreeFour.Degree);
        }

        [TestMethod]
        public void DegreeIsCorrectAfterRemovingEdge()
        {
            Graph g = new Graph();
            TestNode n1 = new TestNode("n1", g);
            TestNode n2 = new TestNode("n2", g);
            TestNode n3 = new TestNode("n3", g);

            n1.AddNeighbor(n2);
            n1.AddNeighbor(n3);
            n2.AddNeighbor(n3);

            Assert.AreEqual(2, n1.Degree);
            Assert.AreEqual(2, n2.Degree);
            Assert.AreEqual(2, n3.Degree);

            n2.RemoveNeighbor(n3);

            Assert.AreEqual(2, n1.Degree);
            Assert.AreEqual(1, n2.Degree);
            Assert.AreEqual(1, n3.Degree);
        }

        [TestMethod]
        [ExpectedException(typeof(DegreeZeroNodeInGraphException))]
        public void DegreeZeroThrowsExceptionOnAvgNeighborsDegree()
        {
            double d = GNDegreeZero.AvgOfNeighborsDegree;
        }

        [TestMethod]
        public void AvgNeighborsDegreeIsCorrect()
        {
            // The first test nodes just have degree 1 neighbors
            Assert.AreEqual(1.0, GNDegreeOne.AvgOfNeighborsDegree, Tolerance);
            Assert.AreEqual(1.0, GNDegreeFour.AvgOfNeighborsDegree, Tolerance);
            Assert.AreEqual(3.33333333333, GNodeWith3NeighborsDegrees1_3_6.AvgOfNeighborsDegree, Tolerance);

            // Any neighbor of GNDegreeFour should have average of degree 4 (its only neighbor)
            TestNode neighborOf4 = (TestNode) GNDegreeFour.Neighbors.First();
            Assert.AreEqual(4.0, neighborOf4.AvgOfNeighborsDegree, Tolerance);

            // get the three degree neighbor of GNodeWith3NeighborsDegree1_3_6, its neighbors will be 1, 3, 3
            TestNode threeDegreeNeighbor =
                (TestNode) GNodeWith3NeighborsDegrees1_3_6.Neighbors.First(n => n.Degree == 3);
            Assert.AreEqual(1.6666666666666666, threeDegreeNeighbor.AvgOfNeighborsDegree, Tolerance);
        }

        [TestMethod]
        public void ChangingNeighborsKeepsAvgNeighborsDegreeCorrect()
        {
            GNDegreeZero.AddNeighbor(new TestNode("new neighbor of degree 0", GNDegreeZero.Graph));
            Assert.AreEqual(1.0, GNDegreeZero.AvgOfNeighborsDegree, Tolerance);

            ((TestNode) GNDegreeFour.Neighbors.First()).AddNeighbor(new Node("new neighbor of 4 degree neighbor",
                GNDegreeFour.Graph));
            Assert.AreEqual(1.25, GNDegreeFour.AvgOfNeighborsDegree, Tolerance);

            ((TestNode) GNodeWith3NeighborsDegrees1_3_6.Neighbors.First(n => n.Degree == 3)).AddNeighbor(
                new TestNode("new neighbor of 3", GNodeWith3NeighborsDegrees1_3_6.Graph));
            Assert.AreEqual(3.66666666666, GNodeWith3NeighborsDegrees1_3_6.AvgOfNeighborsDegree, Tolerance);

            GNodeWith3NeighborsDegrees1_3_6.AddNeighbor(new TestNode("new neighbor",
                GNodeWith3NeighborsDegrees1_3_6.Graph));
            Assert.AreEqual(3.0, GNodeWith3NeighborsDegrees1_3_6.AvgOfNeighborsDegree, Tolerance);
        }

        [TestMethod]
        public void AvgNeighborDegreeIsCorrectAfterNeighborChanges()
        {
            Graph g = new Graph();
            g.AddEdge("n", "n2");
            g.AddEdge("n", "n3");
            g.AddEdge("n", "n4");

            var n = g.Nodes.First(nd => nd.Id == "n");

            Assert.AreEqual(1.0, n.AvgOfNeighborsDegree, Tolerance);

            g.AddEdge("n2", "n2b");

            Assert.AreEqual(1.333333333333, n.AvgOfNeighborsDegree, Tolerance);
        }

        [TestMethod]
        [ExpectedException(typeof(DegreeZeroNodeInGraphException))]
        public void FIndexThrowsExceptionOnZeroDegree()
        {
            var x = GNDegreeZero.FIndex;
        }

        [TestMethod]
        public void FIndexIsCorrect()
        {
            Assert.AreEqual(1.0, GNDegreeOne.FIndex, Tolerance);
            Assert.AreEqual(1.0, GNDegreeOne.Neighbors.First().FIndex, Tolerance);

            Assert.AreEqual(0.25, GNDegreeFour.FIndex, Tolerance);
            Assert.AreEqual(4.0, GNDegreeFour.Neighbors.First().FIndex);

            Assert.AreEqual(1.1111111111, GNodeWith3NeighborsDegrees1_3_6.FIndex, Tolerance);
            Assert.AreEqual(3.0, GNodeWith3NeighborsDegrees1_3_6.Neighbors.First(n => n.Degree == 1).FIndex,
                Tolerance);
            Assert.AreEqual(0.5555555555, GNodeWith3NeighborsDegrees1_3_6.Neighbors.First(n => n.Degree == 3).FIndex,
                Tolerance);
            Assert.AreEqual(0.2222222222, GNodeWith3NeighborsDegrees1_3_6.Neighbors.First(n => n.Degree == 6).FIndex,
                Tolerance);
        }

        [TestMethod]
        public void FIndexIsCorrectAfterChanges()
        {
            GNDegreeZero.AddNeighbor(new TestNode("new neighbor of degree zero", GNDegreeZero.Graph));
            Assert.AreEqual(1.0, GNDegreeZero.FIndex, Tolerance);

            GNDegreeOne.AddNeighbor(new TestNode("new neighbor of degree one", GNDegreeOne.Graph));
            Assert.AreEqual(0.5, GNDegreeOne.FIndex, Tolerance);

            ((TestNode) GNDegreeFour.Neighbors.First()).AddNeighbor(new Node("new neighbor of neighbor of four",
                GNDegreeFour.Graph));
            Assert.AreEqual(0.3125, GNDegreeFour.FIndex, Tolerance);
            Assert.AreEqual(4.0, GNDegreeFour.Neighbors.First(n => n.Degree == 1).FIndex, Tolerance);
            Assert.AreEqual(1.25, GNDegreeFour.Neighbors.First(n => n.Degree == 2).FIndex, Tolerance);

            GNodeWith3NeighborsDegrees1_3_6.AddNeighbor(new TestNode("new neighbor of 1-3-6",
                GNodeWith3NeighborsDegrees1_3_6.Graph));
            ((TestNode) GNodeWith3NeighborsDegrees1_3_6.Neighbors.First(n => n.Degree == 6))
                .AddNeighbor(new TestNode("new neighbor of 6 degree", GNodeWith3NeighborsDegrees1_3_6.Graph));
            Assert.AreEqual(0.75, GNodeWith3NeighborsDegrees1_3_6.FIndex, Tolerance);
        }

        // For the Thedchanamoorthy local assortativity, take the example from the paper, a vertex with neighbors of degree 1, 2, 3, 4, 6
        private string ThedchanGraphExampleString =
            "v\tv1" + "\n" +
            "v\tv2" + "\n" +
            "v2\tv2n2" + "\n" +
            "v\tv3" + "\n" +
            "v3\tv3n2" + "\n" +
            "v3\tv3n3" + "\n" +
            "v\tv4" + "\n" +
            "v4\tv4n2" + "\n" +
            "v4\tv4n3" + "\n" +
            "v4\tv4n4" + "\n" +
            "v\tv6" + "\n" +
            "v6\tv6n2" + "\n" +
            "v6\tv6n3" + "\n" +
            "v6\tv6n4" + "\n" +
            "v6\tv6n5" + "\n" +
            "v6\tv6n6";


        [TestMethod]
        public void AverageDiffFromNeighborsIsCorrect()
        {
            Graph g = Graph.ParseGraphFromTsvEdgesString(ThedchanGraphExampleString);
            Assert.AreEqual(2.2, g.Nodes.First(n => n.Id == "v").AvgDiffFromNeighbors, Tolerance);
        }

        [TestMethod]
        public void AverageDiffFromNeighborsIsCorrectAfterChange()
        {
            // Take the example from the paper, a vertex with neighbors of degree 1, 2, 3, 4, 6
            Graph g = Graph.ParseGraphFromTsvEdgesString(ThedchanGraphExampleString);

            Node v = g.Nodes.First(n => n.Id == "v");
            Assert.AreEqual(2.2, v.AvgDiffFromNeighbors, Tolerance);

            g.AddEdge("v6", "v6n7");

            Assert.AreEqual(2.4, v.AvgDiffFromNeighbors, Tolerance);

            g.AddEdge("v", "v1b");

            Assert.AreEqual(3.33333333333333, v.AvgDiffFromNeighbors, Tolerance);
        }

        [TestMethod]
        public void AverageDiffFromNeighborsAsFractionIsCorrect()
        {
            Graph g = Graph.ParseGraphFromTsvEdgesString(ThedchanGraphExampleString);
            Assert.AreEqual(0.039262344, g.Nodes.First(n => n.Id == "v").AvgDiffFromNeighborsAsFractionOfTotal,
                Tolerance);
        }

        [TestMethod]
        public void AverageDiffFromNeighborsAsFractionIsCorrectAfterChange()
        {
            Graph g = Graph.ParseGraphFromTsvEdgesString(ThedchanGraphExampleString);

            Node v = g.Nodes.First(n => n.Id == "v");
            Assert.AreEqual(0.039262344, v.AvgDiffFromNeighborsAsFractionOfTotal, Tolerance);

            g.AddEdge("v6", "v6n7");

            Assert.AreEqual(0.035124399, v.AvgDiffFromNeighborsAsFractionOfTotal, Tolerance);

            g.AddEdge("v", "v1b");

            Assert.AreEqual(0.043743165, v.AvgDiffFromNeighborsAsFractionOfTotal, Tolerance);
        }

        [TestMethod]
        public void ThedchansAssortIsNegativeForAllNodesInDisassortativeGraph()
        {
            Graph g = new Graph();
            g.AddEdge("v", "v1");
            g.AddEdge("v", "v2");
            g.AddEdge("v", "v3");
            g.AddEdge("v", "v4");
            Assert.IsTrue(g.Nodes.All(n => n.ThedchansLocalAssort < 0));
        }

        [TestMethod]
        public void ThedchansAssortIsPositiveForSomeNodesInGraphWithSomeAssortativity()
        {
            Graph g = new Graph();
            g.AddEdge("v1", "v2");
            g.AddEdge("v1", "v1A");
            g.AddEdge("v1", "v1B");
            g.AddEdge("v1", "v1C");
            g.AddEdge("v1", "v1D");
            g.AddEdge("v2", "v2A");
            g.AddEdge("v2", "v2B");
            g.AddEdge("v2", "v2C");
            g.AddEdge("v2", "v2D");

            g.AddEdge("out1", "out2");
            Assert.IsTrue(g.Nodes.Any(n => n.ThedchansLocalAssort >= 0));
        }

        // This is a sample graph from the Thedchan paper
        private String thedchanSampleGraphString =
            "L1a \t M1" + "\n" +
            "M1 \t L1a , L1b , H1" + "\n" +
            "L1b \t M1" + "\n" +
            "L2a \t M2" + "\n" +
            "M2 \t L2a , L2b , H2" + "\n" +
            "L2b \t M2" + "\n" +
            "L3a \t M3" + "\n" +
            "M3 \t L3a , L3b , H3" + "\n" +
            "L3b \t M3" + "\n" +
            "L4a \t M4" + "\n" +
            "M4 \t L4a , L4b , H4" + "\n" +
            "L4b \t M4" + "\n" +
            "L5a \t M5" + "\n" +
            "M5 \t L5a , L5b , H5" + "\n" +
            "L5b \t M5" + "\n" +
            "L6a \t M6" + "\n" +
            "M6 \t L6a , L6b , H6" + "\n" +
            "L6b \t M6" + "\n" +
            "L7a \t M7" + "\n" +
            "M7 \t L7a , L7b , H7" + "\n" +
            "L7b \t M7" + "\n" +
            "L8a \t M8" + "\n" +
            "M8 \t L8a , L8b , H8" + "\n" +
            "L8b \t M8" + "\n" +
            "H1 \t M1 , H2 , H3 , H4 , H6 , H7 , H8" + "\n" +
            "H2 \t M2 , H1 , H3 , H4 , H5 , H7 , H8" + "\n" +
            "H3 \t M3 , H1 , H2 , H4 , H5 , H6 , H8" + "\n" +
            "H4 \t M4 , H1 , H2 , H3 , H5 , H6 , H7" + "\n" +
            "H5 \t M5 , H2 , H3 , H4 , H6 , H7 , H8" + "\n" +
            "H6 \t M6 , H1 , H3 , H4 , H5 , H7 , H8" + "\n" +
            "H7 \t M7 , H1 , H2 , H4 , H5 , H6 , H8" + "\n" +
            "H8 \t M8 , H1 , H2 , H3 , H5 , H6 , H7";

        [TestMethod]
        public void AvgDiffOfNeighborsIsCorrect()
        {
            Graph g = Graph.ParseGraphFromTsvNodeNeighborsString(thedchanSampleGraphString);

            Assert.IsTrue(g.Nodes.Where(n => n.Id.StartsWith("L")).All(n => Math.Abs(n.AvgDiffFromNeighbors - 2.0) < Tolerance));
            Assert.IsTrue(g.Nodes.Where(n => n.Id.StartsWith("M")).All(n => Math.Abs(n.AvgDiffFromNeighbors - 2.66666666666666) < Tolerance));
            Assert.IsTrue(g.Nodes.Where(n => n.Id.StartsWith("H")).All(n => Math.Abs(n.AvgDiffFromNeighbors - 0.571428571) < Tolerance));
        }

        [TestMethod]
        public void AvgDiffFromNeighborsAsFractionOfTotalIsCorrect()
        {
            Graph g = Graph.ParseGraphFromTsvNodeNeighborsString(thedchanSampleGraphString);

            Assert.IsTrue(
                g.Nodes.Where(n => n.Id.StartsWith("L"))
                    .All(n => Math.Abs(n.AvgDiffFromNeighborsAsFractionOfTotal - 0.034539474) < Tolerance));
            Assert.IsTrue(
                g.Nodes.Where(n => n.Id.StartsWith("M"))
                    .All(n => Math.Abs(n.AvgDiffFromNeighborsAsFractionOfTotal - 0.046052632) < Tolerance));
            Assert.IsTrue(
                g.Nodes.Where(n => n.Id.StartsWith("H"))
                    .All(n => Math.Abs(n.AvgDiffFromNeighborsAsFractionOfTotal - 0.009868421) < Tolerance));
        }

        [TestMethod]
        public void ThedchanLocalAssortIsCorrect()
        {
            Graph g = Graph.ParseGraphFromTsvNodeNeighborsString(thedchanSampleGraphString);

            Assert.IsTrue(
                g.Nodes.Where(n => n.Id.StartsWith("L"))
                    .All(n => Math.Abs(n.ThedchansLocalAssort - 0.01754386) < Tolerance));
            Assert.IsTrue(
                g.Nodes.Where(n => n.Id.StartsWith("M"))
                    .All(n => Math.Abs(n.ThedchansLocalAssort - 0.006030702) < Tolerance));
            Assert.IsTrue(
                g.Nodes.Where(n => n.Id.StartsWith("H"))
                    .All(n => Math.Abs(n.ThedchansLocalAssort - 0.042214912) < Tolerance));
        }

        [TestMethod]
        public void FiMaxOverMinIsCorrect()
        {
            Graph g = Graph.ParseGraphFromTsvNodeNeighborsString(thedchanSampleGraphString);

            Assert.IsTrue(
                g.Nodes.Where(n => n.Id.StartsWith("L"))
                    .All(n => Math.Abs(n.FiMaxOverMin - 3.0) < Tolerance));
            Assert.IsTrue(
                g.Nodes.Where(n => n.Id.StartsWith("M"))
                    .All(n => Math.Abs(n.FiMaxOverMin - 2.7777777777777) < Tolerance));
            Assert.IsTrue(
                g.Nodes.Where(n => n.Id.StartsWith("H"))
                    .All(n => Math.Abs(n.FiMaxOverMin - 1.19047619) < Tolerance));
        }

        [TestMethod]
        public void FiMaxOverMinAsFractionOfTotalIsCorrect()
        {
            Graph g = Graph.ParseGraphFromTsvNodeNeighborsString(thedchanSampleGraphString);
 
            Assert.IsTrue(
                g.Nodes.Where(n => n.Id.StartsWith("L"))
                    .All(n => Math.Abs(n.FiMaxOverMinAsFractionOfTotal - 0.037619427) < Tolerance));
            Assert.IsTrue(
                g.Nodes.Where(n => n.Id.StartsWith("M"))
                    .All(n => Math.Abs(n.FiMaxOverMinAsFractionOfTotal - 0.034832803) < Tolerance));
            Assert.IsTrue(
                g.Nodes.Where(n => n.Id.StartsWith("H"))
                    .All(n => Math.Abs(n.FiMaxOverMinAsFractionOfTotal - 0.014928344) < Tolerance));
        }

        [TestMethod]
        public void ThedchanScaledFiMaxOverMinIsCorrect()
        {
            Graph g = Graph.ParseGraphFromTsvNodeNeighborsString(thedchanSampleGraphString);

            Assert.IsTrue(
                g.Nodes.Where(n => n.Id.StartsWith("L"))
                    .All(n => Math.Abs(n.ThedchanScaledFiMaxOverMin - 0.014463907) < Tolerance));
            Assert.IsTrue(
                g.Nodes.Where(n => n.Id.StartsWith("M"))
                    .All(n => Math.Abs(n.ThedchanScaledFiMaxOverMin - 0.017250531) < Tolerance));
            Assert.IsTrue(
                g.Nodes.Where(n => n.Id.StartsWith("H"))
                    .All(n => Math.Abs(n.ThedchanScaledFiMaxOverMin - 0.037154989) < Tolerance));
        }

        [TestMethod]
        public void FiAbsoluteOfLn()
        {
            Graph g = Graph.ParseGraphFromTsvNodeNeighborsString(thedchanSampleGraphString);

            Assert.IsTrue(
                g.Nodes.Where(n => n.Id.StartsWith("L"))
                    .All(n => Math.Abs(n.FiAbsoluteOfLn - 1.098612289) < Tolerance));
            Assert.IsTrue(
                g.Nodes.Where(n => n.Id.StartsWith("M"))
                    .All(n => Math.Abs(n.FiAbsoluteOfLn - 1.014840813) < Tolerance));
            Assert.IsTrue(
                g.Nodes.Where(n => n.Id.StartsWith("H"))
                    .All(n => Math.Abs(n.FiAbsoluteOfLn - 0.121042551) < Tolerance));
        }

        [TestMethod]
        public void FiAbsoluteOfLnAsFractionOfTotal()
        {
            Graph g = Graph.ParseGraphFromTsvNodeNeighborsString(thedchanSampleGraphString);

            Assert.IsTrue(
                g.Nodes.Where(n => n.Id.StartsWith("L"))
                    .All(n => Math.Abs(n.FiAbsoluteOfLnAsFractionOfTotal - 0.041200747) < Tolerance));
            Assert.IsTrue(
                g.Nodes.Where(n => n.Id.StartsWith("M"))
                    .All(n => Math.Abs(n.FiAbsoluteOfLnAsFractionOfTotal - 0.038059104) < Tolerance));
            Assert.IsTrue(
                g.Nodes.Where(n => n.Id.StartsWith("H"))
                    .All(n => Math.Abs(n.FiAbsoluteOfLnAsFractionOfTotal - 0.004539403) < Tolerance));
        }

        [TestMethod]
        public void ThedchanScaledFiAbsoluteOfLn()
        {
            Graph g = Graph.ParseGraphFromTsvNodeNeighborsString(thedchanSampleGraphString);

            Assert.IsTrue(
                g.Nodes.Where(n => n.Id.StartsWith("L"))
                    .All(n => Math.Abs(n.ThedchanScaledFiAbsoluteOfLn - 0.010882587) < Tolerance));
            Assert.IsTrue(
                g.Nodes.Where(n => n.Id.StartsWith("M"))
                    .All(n => Math.Abs(n.ThedchanScaledFiAbsoluteOfLn - 0.014024229) < Tolerance));
            Assert.IsTrue(
                g.Nodes.Where(n => n.Id.StartsWith("H"))
                    .All(n => Math.Abs(n.ThedchanScaledFiAbsoluteOfLn - 0.047543931) < Tolerance));
        }
    }
}
