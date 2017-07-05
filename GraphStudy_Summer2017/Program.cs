using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphLibyn;

namespace GraphStudy_Summer2017
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < 10; i++)
            {
                TestBaGraph();
                Console.ReadKey();
            }
        }

        static void TestBaGraph()
        {
            BaGraph baGraph = new BaGraph(5, new Randomizer(), 500, 0.9);
            var groups = baGraph.AllNodes.GroupBy(n => n.Degree).OrderBy(g => g.Key);
            foreach (var g in groups)
            {
                Console.WriteLine(g.Key + ": " + g.Count());
            }
            Console.WriteLine();
        }

        static void TestErGraph()
        {
            // Some test code to be able to step through ER and BA graphs
            // If this is still here even a few days after 7/2/17 shame on me

            Console.WriteLine("\n****************\n");
            ErGraph erg = new ErGraph(500, 0.25, new Randomizer());
            var groups = erg.AllNodes.GroupBy(n => n.Degree).OrderBy(g => g.Key);
            foreach (var g in groups)
            {
                Console.WriteLine(g.Key + ": " + g.Count());
            }
            Console.WriteLine();

        }
    }
}
