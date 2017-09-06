using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GraphLibyn
{
    /* *** HACK ***
     * Using a python package to find Power Law alpha and sigma because I haven't found
     * an equivalent C# library and it looks difficult to write. Couldn't get it to work
     * with a python engine for some reason, so just doing it with a Process. Yes, a 
     * terrible hack..
     */

    public static class PyPowerLaw
    {
        public static bool TryPythonPowerLaw(IEnumerable<string> vals, out double alpha, out double sigma,
                bool discrete = true, int TimeOutInMillis = 10000)
            // need to make TimeOut a function of vals.Count at some point...
        {
            bool success = false;

            object dtsLock = new object();
            string dts;
            lock (dtsLock)
            {
                Thread.Sleep(200);
                dts = DateTime.Now.ToString("yy.MM.dd.HH.mm.ss.ffffff");
            }
            String DIR = Directory.GetCurrentDirectory() + "\\" + dts + "\\";
            Directory.CreateDirectory(DIR);

            var valsFile = DIR + "vals_" + dts + ".txt";
            var alphaFile = DIR + "alpha_" + dts + ".txt";
            var sigmaFile = DIR + "sigma_" + dts + ".txt";

            File.WriteAllText(valsFile, String.Join("\n", vals));

            string pyscript = "import powerlaw" + "\n" +
                              "import numpy as np" + "\n" +
                              $"f = open(r'{valsFile}')" + "\n" +
                              "vals = np.array(f.read().split())" + "\n" +
                              $"pl_results = powerlaw.Fit(vals, discrete = {(discrete ? "True" : "False")})" + "\n" +
                              $"f = open(r'{alphaFile}', 'w')" + "\n" +
                              "f.write(str(pl_results.power_law.alpha))" + "\n" +
                              $"f = open(r'{sigmaFile}', 'w')" + "\n" +
                              "f.write(str(pl_results.power_law.sigma))";
            File.WriteAllText(DIR + "alphacalc.py", pyscript);
            Process p = new Process();
            p.StartInfo.FileName = @"C:\Users\ynovick\Anaconda2\python.exe";
            p.StartInfo.Arguments = DIR + "alphacalc.py";
            p.StartInfo.WorkingDirectory = DIR;
            p.StartInfo.CreateNoWindow = false;
            p.StartInfo.UseShellExecute = true;

            p.Start();
            success = p.WaitForExit(TimeOutInMillis);

            if (success)
            {
                alpha = double.Parse(File.ReadAllText(alphaFile));
                sigma = double.Parse(File.ReadAllText(sigmaFile));
                return true;
            }

            alpha = sigma = -1.0;
            return false;
        }
    }
}
