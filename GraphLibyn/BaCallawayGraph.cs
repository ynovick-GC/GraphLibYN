using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphLibyn
{
    // YN 7/23/17 This is just an(other) experiment, to see what happens if we grow a graph
    // by merging the ideas of Barabasi Albert and Callaway to grow a graph both by adding vertices
    // and by connecting existing ones
    public class BaCallawayGraph : BaGraph
    {
        private readonly int M2; // the number of existing nodes that will attach to new neighbors in a time step

        public BaCallawayGraph(int m1, int m2, IRandomizer r, int startingNodes = 1, double exp = 1.0)
            : base(m1, r, startingNodes, exp)
        {
            throw new Exception("Deprecating this code, adding static factory methods to graph instead YN 9/10/17");
            M2 = m2;
        }

        public new void AddNodesToBaCaGraphWithPreferentialAttachment(int numberOfNodes)
        {
            for (int i = 0; i < numberOfNodes; i++)
                AddNodeToBaCaGraphWithPreferentialAttachment();
        }

        public void AddNodeToBaCaGraphWithPreferentialAttachment()
        {
            base.AddNodeToBaGraph(); // add a new node per the original algorithm



            // now add edges to the existing graph
            for (int i = 0; i < M2; i++)
            {
                var currNodes = Nodes.ToList();
                GraphNode n1 = SelectNodeFromPoolWithPreferentialAttachment(currNodes);
                GraphNode n2 = SelectNodeFromPoolWithPreferentialAttachment(currNodes);

                n1.AddNeighbor(n2);
            }
        }

        public void AddEdgesToBaCaBraphWithAssortativityBias(int numberOfEdges, bool positiveAssortativity = true)
        {
            for(int i = 0; i < numberOfEdges; i++)
                AddEdgeToBaCaGraphWithAssortativityBias(positiveAssortativity);
        }

        public void AddEdgeToBaCaGraphWithAssortativityBias(bool positiveAssortativity = true)
        {
            base.AddNodeToBaGraph();
            var currNodes = Nodes.ToList();

            for (int i = 0; i < M2; i++)
            {
                GraphNode n1 = SelectNodeFromPoolWithPreferentialAttachment(currNodes);
                GraphNode n2 = SelectNodeFromPoolBasedOnAssortitivity(n1, currNodes, positiveAssortativity);

                n1.AddNeighbor(n2);
            }
        }

        public new void AddNodeToBaGraph_WITH_OUTPUT()
        {
            throw new NotImplementedException("Output is not available on a BaCallaway graph right now. Later? We'll see...");
        }

        // For the second step of this algorithm we select one node at a time, immediately removing it from the pool and
        // recalculating, so there's no state to save, so a separate method makes sense
        private GraphNode SelectNodeFromPoolWithPreferentialAttachment(List<Node> nodes, bool keepInPool = false)
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
                    selectedNode = (GraphNode) n;
                    break;
                }
                curr += Math.Pow(n.Degree, Exp);
            }

            if (!keepInPool)
                nodes.Remove(selectedNode);

            return selectedNode;
        }

        private GraphNode SelectNodeFromPoolBasedOnAssortitivity(GraphNode firstNode, List<Node> remainingNodes, bool positiveAssortitivity = true)
        {
            GraphNode selectedNode = null;

            // create a "pool" of the sum of all nodes' diff from the selected node
            var maxDiff = remainingNodes.Max(n => Math.Abs(n.Degree - firstNode.Degree));
            Func<Node, double> nodeValFunc = n => Math.Pow(positiveAssortitivity
                ? maxDiff - (Math.Abs(n.Degree - firstNode.Degree) + 1)
                : Math.Abs(n.Degree - firstNode.Degree) + 1, Exp);

            var totalVals = remainingNodes.Sum(n => nodeValFunc(n));
            double val = random.NextDouble()*totalVals;

            double curr = 0;

            foreach (var n in remainingNodes)
            {
                if (curr <= val && curr + nodeValFunc(n) > val)
                {
                    selectedNode = (GraphNode) n;
                    break;
                }
                curr += nodeValFunc(n);
            }
            return selectedNode;
        }
    }
}
