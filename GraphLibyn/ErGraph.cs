﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphLibyn
{
    public class ErGraph : Graph
    {
        public int N { get; }

        private readonly double p;
        public double P => p;
        private IRandomizer random;

        public ErGraph(int n, double p, IRandomizer r)
        {
            throw new Exception("Deprecating this code, adding static factory methods to graph instead YN 9/10/17");
            this.N = n;
            this.p = p;
            this.random = r;

            for (int i = 1; i <= n; i++)
                AddNode("n" + i);

            var nodesList = _nodesDictionary.Values.ToList();
            for (int i = 0; i < nodesList.Count; i++)
                for (int j = i + 1; j < nodesList.Count; j++)
                    if (random.GetTrueWithProbability(p))
                        AddEdge(nodesList[i], nodesList[j]);
        }

    }
}
