using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphLibyn
{
    public class BaGraph : Graph
    {
        public int M { get; }
        // By default Exp will be 1.0 which will mean that every node has probability of being selected exactly equal to
        // its degree/sum(all degrees), but every node's portion of this pool can be raised to Exp, so we can hopefully 
        // tweak the preferential attachment, as Exp increases the higher nodes are favored that much more
        public double Exp { get; }
        private IRandomizer random;

        private int nextId = 1;

        public BaGraph(int m, IRandomizer r, int startingNodes = 1, double exp = 1.0)
        {
            M = m;
            random = r;
            Exp = exp;

            for (int i = 0; i < m; i++)
                AddNode("n" + nextId++);

            // Get a list of these nodes before adding the next one
            var existingNodes = _nodes.Values.ToList();

            // startingNodes is not precise, the graph will always start with m+1 nodes, then add startingNodes
            GraphNode firstStartingNode = NewGraphNode("n" + nextId++);
            existingNodes.ForEach(n => AddEdge(firstStartingNode, (GraphNode) n));

            AddNodesToBaGraph(startingNodes);
        }

        public void AddNodesToBaGraph(int numberOfNodes)
        {
            for (int i = 0; i < numberOfNodes; i++)
                AddNodeToBaGraph();
        }

        public void AddNodeToBaGraph()
        {
            // Get list of nodes before new node is added
            var currNodes = _nodes.Values.ToList(); // we will choose M nodes from this list

            GraphNode newNode = NewGraphNode("n" + nextId++);

            // now choose its neighbors per the BA algorithm:
            for (int i = 0; i < M; i++)
            {
                // Create a "pool" of the sum of all degrees
                var totalDegrees = currNodes.Sum(n => Math.Pow(n.Degree, Exp));
                double val = random.NextDouble()*totalDegrees; // Some point in this "pool"
                // Move throgh the "pool" node by node until we get to the node that is at this number's location
                double curr = 0;
                GraphNode selectedNode = null;
                foreach (var n in currNodes)
                {
                    if (curr <= val && curr + Math.Pow(n.Degree, Exp) > val)
                    {
                        selectedNode = (GraphNode) n;
                        selectedNode.AddNeighbor(newNode);
                        break;
                    }
                    curr += n.Degree;
                }
                currNodes.Remove(selectedNode);
            }
        }
    }
}
