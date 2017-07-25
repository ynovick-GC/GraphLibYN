using System;
using System.Linq;
using GraphLibyn;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class BaGraph_TESTS
    {



        [TestMethod]
        public void BaGraphHasCorrectVectorsBeforeAndAfterAddingNode()
        {
            BaGraph g = new BaGraph(3, new Randomizer(), 10);
            Assert.AreEqual(14, g.DegreeVector.Count());
            g.AddNodeToBaGraph();
            Assert.AreEqual(15, g.DegreeVector.Count());
        }
    }
}
