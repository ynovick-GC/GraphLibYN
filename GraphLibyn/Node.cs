using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphLibyn.Exceptions;

namespace GraphLibyn
{
    public class Node
    {
        private readonly string _id;
        public string Id => _id;

        private bool cacheIsEmpty = true;

        // The graph this node belongs to
        protected Graph _graph; // making it protected so a graph can deserialize from file and then set each node to point to the graph
        public Graph Graph => _graph;

        public Node(string Id, Graph graph)
        {
            this._id = Id;
            this._graph = graph;
        }

        // Any time a value is set the cache should be reset because all values become invalid. But if the cache is already
        // empty there is no reason to reset again. The first time any value is calculated, it should be cached and the flag
        // should be set so that any new change will reset the cache again.
        protected void EmptyCache()
        {
            if (!cacheIsEmpty)
            {
                _neighborsInOrderById = null;
                _avgOfNeighborsDegree = double.NegativeInfinity;
                _avgDiffFromNeighbors = double.NegativeInfinity;
                _avgDiffFromNeighborsAsFractionOfTotal = double.NegativeInfinity;
                _thedchansLocalAssort = double.NegativeInfinity;
                _fiMaxOverMin = double.NegativeInfinity;
                _fiMaxOverMinAsFractionOfTotal = double.NegativeInfinity;
                _thedchanScaledFiMaxOverMin = -1.0;
                _fiAbsoluteOfLn = double.NegativeInfinity;
                _fiAbsoluteOfLnAsFractionOfTotal = double.NegativeInfinity;
                _thedchanScaledFiAbsoluteOfLn = double.NegativeInfinity;
            }
            cacheIsEmpty = true;
        }

        protected ISet<Node> _neighbors = new HashSet<Node>();
        protected List<Node> _neighborsInOrderById = null;

        public IEnumerable<Node> Neighbors
        {
            get
            {
                cacheIsEmpty = false;
                return _neighborsInOrderById ?? (_neighborsInOrderById = _neighbors.OrderBy(n => n.Id).ToList());
            }
        }

        protected bool AddNeighbor(Node n)
        {
            if (n == this)
                throw new NoSelfLoopsException("Self loop id: " + Id);

            // Throw an exception if a node is attached to a node that isn't from the same graph
            if (this.Graph != n.Graph)
                throw new GraphCreationException(
                    $"Attempting to connect two nodes from different graphs, {this.Id}, {n.Id}");

            bool changeMade = this._neighbors.Add(n);
            n._neighbors.Add(this);

            // if a change was made empty the cache
            if (changeMade)
            {
                EmptyCache();
                n.EmptyCache();
            }
            return changeMade;
        }

        protected bool RemoveNeighbor(Node n)
        {
            if (!Neighbors.Contains(n))
                return false;

            n._neighbors.Remove(this);
            _neighbors.Remove(n);
            n.EmptyCache();
            EmptyCache();
            return true;
        }

        // Assuming _neighbors already has an updated Count property so no going through Neighbors and forcing a recalculation if dirty
        public int Degree => _neighbors.Count;
        public int ExcDegree => Degree - 1;
        // Newman's assortativity measure makes use of "excess", or "remaining", degree

        private double _avgOfNeighborsDegree = Double.NegativeInfinity;
        public double AvgOfNeighborsDegree
        {
            get
            {
                if (Degree == 0) throw new DegreeZeroNodeInGraphException();
                cacheIsEmpty = false;
                return double.IsNegativeInfinity(_avgOfNeighborsDegree)
                    ? (_avgOfNeighborsDegree = Neighbors.Average(n => n.Degree))
                    : _avgOfNeighborsDegree;
            }
        }

        public double FIndex => AvgOfNeighborsDegree/Degree;

        // For the Thedchanamoorthy et al paper's definition of local assortativity, need this value
        private double _avgDiffFromNeighbors = double.NegativeInfinity;
        public double AvgDiffFromNeighbors
        {
            get
            {
                cacheIsEmpty = false;
                return double.IsNegativeInfinity(_avgDiffFromNeighbors)
                    ? _avgDiffFromNeighbors = Neighbors.Average(n => Math.Abs(this.Degree - n.Degree))
                    : _avgDiffFromNeighbors;
            }
        }

        // Thedchanamoorthy scales the avg diff to a fraction of the total of such values
        private double _avgDiffFromNeighborsAsFractionOfTotal = double.NegativeInfinity;
        public double AvgDiffFromNeighborsAsFractionOfTotal
        {
            get
            {
                cacheIsEmpty = true;
                return double.IsNegativeInfinity(_avgDiffFromNeighborsAsFractionOfTotal)
                    ? _avgDiffFromNeighborsAsFractionOfTotal = AvgDiffFromNeighbors/Graph.SumOfAllNodesAvgDiffs
                    : _avgDiffFromNeighborsAsFractionOfTotal;
            }
        }

        private double _thedchansLocalAssort = double.NegativeInfinity;
        public double ThedchansLocalAssort
        {
            get
            {
                cacheIsEmpty = false;
                return double.IsNegativeInfinity(_thedchansLocalAssort)
                    ? _thedchansLocalAssort = Graph.ThedchansLambda - AvgDiffFromNeighborsAsFractionOfTotal
                    : _thedchansLocalAssort;
            }
        }

        // Another idea for FIndex related to assortativity, take the average ratio but use max/min so its always above 1
        private double _fiMaxOverMin = double.NegativeInfinity;
        public double FiMaxOverMin
        {
            get
            {
                cacheIsEmpty = false;
                return double.IsNegativeInfinity(_fiMaxOverMin)
                    ? _fiMaxOverMin =
                        Neighbors.Average(n => (double) Math.Max(Degree, n.Degree)/Math.Min(Degree, n.Degree))
                    : _fiMaxOverMin;
            }
        }

        private double _fiMaxOverMinAsFractionOfTotal = Double.NegativeInfinity;
        public double FiMaxOverMinAsFractionOfTotal
        {
            get
            {
                cacheIsEmpty = false;
                return double.IsNegativeInfinity(_fiMaxOverMinAsFractionOfTotal)
                    ? _fiMaxOverMinAsFractionOfTotal = FiMaxOverMin/Graph.SumOfAllFiMaxOverMin
                    : _fiMaxOverMinAsFractionOfTotal;
            }
        }

        private double _thedchanScaledFiMaxOverMin = -1.0;
        public double ThedchanScaledFiMaxOverMin
        {
            get
            {
                cacheIsEmpty = false;
                return _thedchanScaledFiMaxOverMin == -1.0
                    ? _thedchanScaledFiMaxOverMin = Graph.ThedchansLambda - FiMaxOverMinAsFractionOfTotal
                    : _thedchanScaledFiMaxOverMin;
            }
        }

        // Another idea for FIndex related to assoratitivy, take the absolute value of ln(ratio)
        private double _fiAbsoluteOfLn = double.NegativeInfinity;
        public double FiAbsoluteOfLn
        {
            get
            {
                cacheIsEmpty = false;
                return double.IsNegativeInfinity(_fiAbsoluteOfLn)
                    ? _fiAbsoluteOfLn = Neighbors.Average(n => Math.Abs(Math.Log((double) n.Degree/Degree)))
                    : _fiAbsoluteOfLn;
            }
        }

        private double _fiAbsoluteOfLnAsFractionOfTotal = double.NegativeInfinity;
        public double FiAbsoluteOfLnAsFractionOfTotal
        {
            get
            {
                cacheIsEmpty = false;
                return double.IsNegativeInfinity(_fiAbsoluteOfLnAsFractionOfTotal)
                    ? _fiAbsoluteOfLnAsFractionOfTotal = FiAbsoluteOfLn/Graph.SumOfAllFiAbsoluteOfLn
                    : _fiAbsoluteOfLnAsFractionOfTotal;
            }
        }

        private double _thedchanScaledFiAbsoluteOfLn = double.NegativeInfinity;
        public double ThedchanScaledFiAbsoluteOfLn
        {
            get
            {
                cacheIsEmpty = false;
                return double.IsNegativeInfinity(_thedchanScaledFiAbsoluteOfLn)
                    ? _thedchanScaledFiAbsoluteOfLn = Graph.ThedchansLambda - FiAbsoluteOfLnAsFractionOfTotal
                    : _thedchanScaledFiAbsoluteOfLn;
            }
        }
    }
}
