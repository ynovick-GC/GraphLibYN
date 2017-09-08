using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DbContext;
using GraphLibyn;
using MathNet.Numerics.Distributions;
using Newtonsoft.Json;

namespace GraphStudy_Summer2017
{
    // YN 9/8/17 - this is the entry point for the actual program, may just run scratchpad code here or may compile
    // and run actual Analyses functions from the command line. We'll see..
    class Program
    {
        static Randomizer GlobalRand = new Randomizer();

        static void Main(string[] args)
        {
            Analyses.SaveStatsForGraph(args[0]);
            Pause();
        }



        static void Pause()
        {
            Console.WriteLine("\nAny key\n");
            Console.ReadKey();
        }

 
    }
}
