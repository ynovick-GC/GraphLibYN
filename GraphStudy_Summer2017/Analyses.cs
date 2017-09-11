using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphLibyn;

namespace GraphStudy_Summer2017
{
    // Writing this class to hold functions that are useful, and not just one-time hacks. More orgainzed that way hopefully.
    public static class Analyses
    {
        static Randomizer GlobalRand = new Randomizer();
        // this function will parse a graph from a file (edge edge CSV, like the ones on the ASU site)
        // and save two files to the directory where the file is, one that has the general stats
        // and one that is a tab-delimited file of the relevant vectors. I've written python code that
        // will give some plots for the graphs stored in this format. YN 9/8/17
        public static void SaveStatsForGraph(String filePath)
        {
            StringBuilder stats = new StringBuilder();
            StringBuilder vectors = new StringBuilder();
            vectors.AppendLine("Id\tDegree\tFi\tLocalAssort");

            var directory = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
            var fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1);

            String fileContents = File.ReadAllText(filePath).Replace(",", "\t");
            Graph graph = Graph.ParseGraphFromTsvEdgesString(fileContents);

            var graphAssortativity = graph.GraphAssortativity;
            double degAlpha, degSigma;
            PyPowerLaw.TryPythonPowerLaw(graph.DegreeVector.Select(d => d.ToString()), out degAlpha, out degSigma, true);
            double fiAlpha, fiSigma;
            PyPowerLaw.TryPythonPowerLaw(graph.FiVector.Select(d => d.ToString()), out fiAlpha, out fiSigma, false);

            stats.AppendLine($"Assortativity\t{graphAssortativity}");
            stats.AppendLine($"Deg Alpha\t{degAlpha}");
            stats.AppendLine($"Deg Sigma\t{degSigma}");
            stats.AppendLine($"Fi Alpha\t{fiAlpha}");
            stats.AppendLine($"Fi Sigma\t{fiSigma}");

            graph.Nodes.OrderBy(n => n.Degree).ToList().ForEach(
                n => vectors.AppendLine($"{n.Id}\t{n.Degree}\t{n.FIndex}\t{n.ThedchansLocalAssort}")
                );

            File.WriteAllText(directory + "Stats.txt", stats.ToString());
            File.WriteAllText(directory + "Vectors.txt", vectors.ToString());

        }

    }
}
