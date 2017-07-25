using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphLibyn
{
    // YN 7/23/17 This is just an(other) experiment, to see what happens if we grow a graph
    // by merging the ideas of 
    public class BaCallawayGraph : BaGraph
    {
        private readonly int M2; // the number of existing nodes that will attach to new neighbors in a time step

        public BaCallawayGraph(int m1, int m2, IRandomizer r, int startingNodes = 1, double exp = 1.0)
            : base(m1, r, startingNodes, exp)
        {
            M2 = m2;
        }

        public new void AddNodesToBaGraph(int numberOfNodes)
        {
            for (int i = 0; i < numberOfNodes; i++)
                AddNodeToBaGraph();
        }

        public new void AddNodeToBaGraph()
        {
            base.AddNodeToBaGraph(); // add a new node per the original algorithm

            var currNodes = AllNodes.ToList();

            // now add edges to the existing graph
            for (int i = 0; i < M2; i++)
            {
                GraphNode n1 = SelectNodeFromPool(currNodes);
                GraphNode n2 = SelectNodeFromPool(currNodes);

                n1.AddNeighbor(n2);
            }
        }

        public new void AddNodeToBaGraph_WITH_OUTPUT()
        {
            throw new NotImplementedException("Output is not available on a BaCallaway graph right now. Later? We'll see...");
        }

        // For the second step of this algorithm we select one node at a time, immediately removing it from the pool and
        // recalculating, so there's no state to save, so a separate method makes sense
        private GraphNode SelectNodeFromPool(List<Node> nodes, bool keepInPool = false)
        {
            GraphNode selectedNode = null;
            
            // Create a "pool" of the sum of all degrees
            var totalDegrees = nodes.Sum(n => Math.Pow(n.Degree, Exp));
            double val = random.NextDouble() * totalDegrees; // Some point in this "pool"
            
            // Move throgh the "pool" node by node until we get to the node that is at this number's location
            double curr = 0;

            foreach (var n in nodes)
            {
                if (curr <= val && curr + Math.Pow(n.Degree, Exp) > val)
                {
                    selectedNode = (GraphNode)n;
                    break;
                }
                curr += n.Degree;
            }

            if (!keepInPool)
                nodes.Remove(selectedNode);

            return selectedNode;
        }
    }
}
