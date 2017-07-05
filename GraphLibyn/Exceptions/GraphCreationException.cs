using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphLibyn.Exceptions
{
    public class GraphCreationException : Exception
    {
        public GraphCreationException() : base()
        {
        }

        public GraphCreationException(string msg) : base(msg)
        {
        }

        public GraphCreationException(string msg, Exception ex) : base(msg, ex)
        {
        }
    }
}
