using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphLibyn.Exceptions
{
    public class NoSelfLoopsException : Exception
    {
        public NoSelfLoopsException() : base()
        {
        }

        public NoSelfLoopsException(string msg) : base(msg)
        {
        }

        public NoSelfLoopsException(string msg, Exception ex) : base(msg, ex)
        {
        }
    }
}
