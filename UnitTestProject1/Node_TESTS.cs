using System;
using System.Linq;
using GraphLibyn;
using GraphLibyn.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class Node_TESTS
    {
        // Just a test wrapper class to expose AddNeighbor for testing, and 
        // dirty (as a readonly) so it can be included in testing
        private class TestNode : Node
        {
            public TestNode(string id) : base(id)
            {
            }

            // Need this wrapper just to test the method itself, shouldn't be used in other contexts
            public new bool AddNeighbor(Node n) => base.AddNeighbor(n);

            // For testing, expose dirty as a readonly variable
            public new bool dirty => base.dirty;
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
            GNDegreeZero = new TestNode("Degree0");

            GNDegreeOne = new TestNode("Degree1");
            GNDegreeOne.AddNeighbor(new Node("Degree1_Neighbor1"));

            GNDegreeFour = new TestNode("Degree4");
            GNDegreeFour.AddNeighbor(new TestNode("Degree4_Neighbor1"));
            GNDegreeFour.AddNeighbor(new TestNode("Degree4_Neighbor2"));
            GNDegreeFour.AddNeighbor(new TestNode("Degree4_Neighbor3"));
            GNDegreeFour.AddNeighbor(new TestNode("Degree4_Neighbor4"));

            GNodeWith3NeighborsDegrees1_3_6 = new TestNode("GnodeWithNDegrees1-3-6");
            TestNode OneDegreeNeighbor = new TestNode("1 Degree Neighbor");
            OneDegreeNeighbor.AddNeighbor(GNodeWith3NeighborsDegrees1_3_6);
            TestNode ThreeDegreeNode = new TestNode("3 degree neighbor");
            ThreeDegreeNode.AddNeighbor(GNodeWith3NeighborsDegrees1_3_6);
            ThreeDegreeNode.AddNeighbor(new TestNode("3 degree neighbor n2"));
            ThreeDegreeNode.AddNeighbor(new TestNode("3 degree neighbor n3"));
            TestNode SixDegreeNeighbor = new TestNode("6 degree neighbor");
            SixDegreeNeighbor.AddNeighbor(GNodeWith3NeighborsDegrees1_3_6);
            SixDegreeNeighbor.AddNeighbor(new TestNode("6 degree neighbor n2"));
            SixDegreeNeighbor.AddNeighbor(new TestNode("6 degree neighbor n3"));
            SixDegreeNeighbor.AddNeighbor(new TestNode("6 degree neighbor n4"));
            SixDegreeNeighbor.AddNeighbor(new TestNode("6 degree neighbor n5"));
            SixDegreeNeighbor.AddNeighbor(new TestNode("6 degree neighbor n6"));
        }

        [TestMethod]
        [ExpectedException(typeof(NoSelfLoopsException))]
        public void TestNoSelfLoops()
        {
            TestNode n = new TestNode("Id");
            n.AddNeighbor(n);
        }

        [TestMethod]
        public void NeighborsIffAdded()
        {
            TestNode n1 = new TestNode("n1");
            TestNode n2 = new TestNode("n2");

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
            n2.AddNeighbor(new Node("n3"));

            Assert.IsFalse(n1.Neighbors.Contains(n1));
            Assert.IsTrue(n1.Neighbors.Contains(n2));
            Assert.IsFalse(n2.Neighbors.Contains(n2));
            Assert.IsTrue(n2.Neighbors.Contains(n1));

        }

        [TestMethod]
        public void NewNodeIsDirty()
        {
            TestNode n = new TestNode("test");
            Assert.IsTrue(n.dirty);
        }

        [TestMethod]
        public void AccessingFIndexSetsDirtyToFalse()
        {
            TestNode n = new TestNode("test");
            n.AddNeighbor(new Node("n2"));
            var d = n.FIndex;
            Assert.IsFalse(n.dirty);
        }

        [TestMethod]
        public void AddingNodeSetsDirtyToTrueAfterFIndexIsAccessed()
        {
            TestNode n = new TestNode("test");
            n.AddNeighbor(new TestNode("n2"));
            var d = n.FIndex;
            Assert.IsFalse(n.dirty);
            n.AddNeighbor(new TestNode("n3"));
            Assert.IsTrue(n.dirty);
            d = n.FIndex;
            Assert.IsFalse(n.dirty);
            d = n.FIndex + 2;
            Assert.IsFalse(n.dirty);
        }

        [TestMethod]
        public void NeighborsMaintainedAfterAdditionalAdds()
        {
            TestNode n1 = new TestNode("n1");
            TestNode n2 = new TestNode("n2");

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

            n2.AddNeighbor(new Node("n3"));

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
            GNDegreeZero.AddNeighbor(GNDegreeOne);
            Assert.AreEqual(1, GNDegreeZero.Degree);
            Assert.AreEqual(2, GNDegreeOne.Degree);

            GNDegreeZero.AddNeighbor(GNDegreeOne);
            Assert.AreEqual(1, GNDegreeZero.Degree);
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
            TestNode neighborOf4 = (TestNode)GNDegreeFour.Neighbors.First();
            Assert.AreEqual(4.0, neighborOf4.AvgOfNeighborsDegree, Tolerance);

            // get the three degree neighbor of GNodeWith3NeighborsDegree1_3_6, its neighbors will be 1, 3, 3
            TestNode threeDegreeNeighbor = (TestNode)GNodeWith3NeighborsDegrees1_3_6.Neighbors.First(n => n.Degree == 3);
            Assert.AreEqual(1.6666666666666666, threeDegreeNeighbor.AvgOfNeighborsDegree, Tolerance);
        }

        [TestMethod]
        public void ChangingNeighborsKeepsAvgNeighborsDegreeCorrect()
        {
            GNDegreeZero.AddNeighbor(new TestNode("new neighbor of degree 0"));
            Assert.AreEqual(1.0, GNDegreeZero.AvgOfNeighborsDegree, Tolerance);

            ((TestNode) GNDegreeFour.Neighbors.First()).AddNeighbor(new Node("new neighbor of 4 degree neighbor"));
            Assert.AreEqual(1.25, GNDegreeFour.AvgOfNeighborsDegree, Tolerance);

            ((TestNode) GNodeWith3NeighborsDegrees1_3_6.Neighbors.First(n => n.Degree == 3)).AddNeighbor(
                new TestNode("new neighbor of 3"));
            Assert.AreEqual(3.66666666666, GNodeWith3NeighborsDegrees1_3_6.AvgOfNeighborsDegree, Tolerance);

            GNodeWith3NeighborsDegrees1_3_6.AddNeighbor(new TestNode("new neighbor"));
            Assert.AreEqual(3.0, GNodeWith3NeighborsDegrees1_3_6.AvgOfNeighborsDegree, Tolerance);
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

            Assert.AreEqual(4.0, GNDegreeFour.FIndex, Tolerance);
            Assert.AreEqual(0.25, GNDegreeFour.Neighbors.First().FIndex);

            Assert.AreEqual(0.9000000009, GNodeWith3NeighborsDegrees1_3_6.FIndex, Tolerance);
            Assert.AreEqual(0.3333333333, GNodeWith3NeighborsDegrees1_3_6.Neighbors.First(n => n.Degree == 1).FIndex,
                Tolerance);
            Assert.AreEqual(1.8, GNodeWith3NeighborsDegrees1_3_6.Neighbors.First(n => n.Degree == 3).FIndex,
                Tolerance);
            Assert.AreEqual(4.5, GNodeWith3NeighborsDegrees1_3_6.Neighbors.First(n => n.Degree == 6).FIndex,
                Tolerance);
        }

        [TestMethod]
        public void FIndexIsCorrectAfterChanges()
        {
            GNDegreeZero.AddNeighbor(new TestNode("new neighbor of degree zero"));
            Assert.AreEqual(1.0, GNDegreeZero.FIndex, Tolerance);

            GNDegreeOne.AddNeighbor(new TestNode("new neighbor of degree one"));
            Assert.AreEqual(2.0, GNDegreeOne.FIndex, Tolerance);

            ((TestNode)GNDegreeFour.Neighbors.First()).AddNeighbor(new Node("new neighbor of neighbor of four"));
            Assert.AreEqual(3.2, GNDegreeFour.FIndex, Tolerance);
            Assert.AreEqual(0.25, GNDegreeFour.Neighbors.First(n => n.Degree == 1).FIndex, Tolerance);
            Assert.AreEqual(0.8, GNDegreeFour.Neighbors.First(n => n.Degree == 2).FIndex, Tolerance);

            GNodeWith3NeighborsDegrees1_3_6.AddNeighbor(new TestNode("new neighbor of 1-3-6"));
            ((TestNode)GNodeWith3NeighborsDegrees1_3_6.Neighbors.First(n => n.Degree == 6))
                .AddNeighbor(new TestNode("new neighbor of 6 degree"));
            Assert.AreEqual(1.33333333333333, GNodeWith3NeighborsDegrees1_3_6.FIndex, Tolerance);
        }

    }
}
