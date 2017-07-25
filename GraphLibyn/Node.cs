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
        protected bool dirty = true; // Any change must update dirty

        private readonly string _id;
        public string Id => _id;
        public Node(string Id)
        {
            this._id = Id;
        }


        protected ISet<Node> _neighbors = new HashSet<Node>();
        protected List<Node> _neighborsInOrderById = new List<Node>();

        public IEnumerable<Node> Neighbors
        {
            get
            {

                if (dirty)
                {
                    _neighborsInOrderById = _neighbors.OrderBy(n => n.Id).ToList();
                    dirty = false;
                }
                return _neighborsInOrderById;
            }
        }

        protected bool AddNeighbor(Node n)
        {
            if (n == this)
                throw new NoSelfLoopsException("Self loop id: " + Id);

            bool changeMade = this._neighbors.Add(n);
            n._neighbors.Add(this);

            // if a change was made assign to dirty before returning
            if (changeMade)
            {
                dirty = true;
                n.dirty = true;
            }
            return changeMade;
        }

        protected bool RemoveNeighbor(Node n)
        {
            if (!Neighbors.Contains(n))
                return false;

            n._neighbors.Remove(this);
            _neighbors.Remove(n);
            n.dirty = true;
            dirty = true;
            return true;
        }

        // Assuming _neighbors already has an updated Count property so no going through Neighbors and forcing a recalculation if dirty
        public int Degree => _neighbors.Count;
        public int ExcDegree => Degree - 1; // Newman's assortativity measure makes use of "excess", or "remaining", degree

        public double AvgOfNeighborsDegree
        {
            get
            {
                if (Degree == 0) throw new DegreeZeroNodeInGraphException();
                return Neighbors.Average(n => n.Degree);
            }
        }

        public double FIndex => Degree/AvgOfNeighborsDegree;
    }
}
