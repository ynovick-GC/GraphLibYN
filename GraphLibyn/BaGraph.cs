using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
        protected IRandomizer random;

        protected int nextId = 1;

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

            // Create a "pool" of the sum of all degrees
            var totalDegrees = currNodes.Sum(n => Math.Pow(n.Degree, Exp));

            // now choose its neighbors per the BA algorithm:
            for (int i = 0; i < M; i++)
            {

                double val = random.NextDouble()*totalDegrees; // Some point in this "pool"
                // Move throgh the "pool" node by node until we get to the node that is at this number's location
                double curr = 0;
                GraphNode selectedNode = null;
                foreach (var n in currNodes)
                {
                    if (curr <= val && curr + Math.Pow(n.Degree, Exp) > val)
                    {
                        selectedNode = (GraphNode) n;
                        totalDegrees -= Math.Pow(selectedNode.Degree, Exp);
                        AddEdge(selectedNode, newNode);
                        currNodes.Remove(selectedNode);
                        break;
                    }
                    curr += Math.Pow(n.Degree, Exp);
                }
            }
        }

        public void AddNodeToBaGraph_WITH_OUTPUT()
        {
            throw new Exception("This code is not trusted at the moment, can come back to it..");
            // Get list of nodes before new node is added
            var currNodes = _nodes.Values.ToList(); // we will choose M nodes from this list

            GraphNode newNode = NewGraphNode("n" + nextId++);

            Console.WriteLine($"\n***Adding node {newNode.Id}***");
            // Create a "pool" of the sum of all degrees
            var totalDegrees = currNodes.Sum(n => Math.Pow(n.Degree, Exp));

            // now choose its neighbors per the BA algorithm:
            for (int i = 0; i < M; i++)
            {
                Console.WriteLine($"\nSelecting neighbor {i + 1}...");



                double currTmp = 0;
                List<Tuple<int, string>> output = new List<Tuple<int, string>>();
                foreach (var node in currNodes)
                {
                    var degree = node.Degree;
                    var scaledDegree = Math.Pow(degree, Exp);
                    var probabilityOfBeingSelected = scaledDegree/totalDegrees;
                    var lowerBound = currTmp;
                    var upperBound = currTmp + scaledDegree;
                    currTmp += scaledDegree;
                    output.Add(new Tuple<int, string>(node.Degree,
                        $"Node {node.Id}\tdeg: {degree},\tscaled: {scaledDegree.ToString("0.###")},\tprobability: {probabilityOfBeingSelected.ToString("0.###")},\tbtwn: {Math.Round(lowerBound, 3)}, {Math.Round(upperBound, 3)}"));
                }
                output.OrderByDescending(t => t.Item1).Select(t => t.Item2).ToList().ForEach(Console.WriteLine);

                double val = random.NextDouble()*totalDegrees; // Some point in this "pool"
                Console.WriteLine($"The selected number is: {val.ToString("#.000")} of {totalDegrees.ToString("#.000")}");
                // Move throgh the "pool" node by node until we get to the node that is at this number's location
                double curr = 0;
                GraphNode selectedNode = null;
                foreach (var n in currNodes)
                {
                    if (curr <= val && curr + Math.Pow(n.Degree, Exp) > val)
                    {
                        selectedNode = (GraphNode) n;
                        Console.WriteLine($"The selected node is {selectedNode.Id}," +
                                          $" degree {selectedNode.Degree}.");
                        totalDegrees -= Math.Pow(selectedNode.Degree, Exp);
                        AddEdge(selectedNode, newNode);
                        currNodes.Remove(selectedNode);
                        break;
                    }
                    curr += Math.Pow(n.Degree, Exp);
                }

                Console.ReadKey();
            }

            Console.WriteLine("\n" +
                              String.Join("\n",
                                  DegreeCountDictionary.OrderByDescending(kvp => kvp.Key)
                                      .Select(kvp => kvp.Key + "\t\t" + kvp.Value)));
        }
    }
}
