﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphLibyn.Exceptions
{
    // For friendship paradox we just have to exclude all vertices of degree 0 so we'll just exclude them
    // on the node level
    public class EmptyGraphException : GraphException
    {
        public EmptyGraphException() : base()
        {
        }

        public EmptyGraphException(string msg) : base(msg)
        {
        }

        public EmptyGraphException(string msg, Exception ex) : base(msg, ex)
        {
        }
    }
}
