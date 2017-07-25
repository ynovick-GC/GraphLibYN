using System;
using System.Linq;
using GraphLibyn;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class ErGraph_TESTS
    {
        [TestMethod]
        public void ErGraphHasCorrectVectorSizeAfterBeingCreated()
        {
            ErGraph g = new ErGraph(100, 0.15, new Randomizer());
            Assert.AreEqual(100, g.DegreeVector.Count());
        }
    }
}
