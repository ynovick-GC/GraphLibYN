using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphLibyn.Exceptions
{
    // A more generic Exception class for all graph exceptions, more specific ones will inherit from this
    public class GraphException : Exception
    {
        public GraphException() : base()
        {
        }

        public GraphException(string msg) : base(msg)
        {
        }

        public GraphException(string msg, Exception ex) : base(msg, ex)
        {
        }
    }
}
