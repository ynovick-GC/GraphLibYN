using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphLibyn
{
    /* *** HACK ***
     * Using a python package to find Power Law alpha and sigma because I haven't found
     * an equivalent C# library and it looks difficult to write. Couldn't get it to work
     * with a python engine for some reason, do just doing it with a Process. Yes, a 
     * terrible hack..
     */

    public static class PyPowerLaw 
    {
        public static bool TryPythonPowerLaw(IEnumerable<string> vals, out double alpha, out double sigma, int TimeOutInMillis = 10000) // need to make TimeOut a function of vals.Count at some point...
        {
            var DIR = @"D:\Data\AlphaCalculator\";

            bool success = false;

            // Clean up old files. Note that this is not thread safe
            new DirectoryInfo(DIR).GetFiles("*.txt").ToList().ForEach(
                f => File.Move(f.FullName, DIR + @"ARCHIVE\" + f.Name)
                );

            string dts = DateTime.Now.ToString("yyMMddHHmmss");
            var valsFile = DIR + "vals_" + dts + ".txt";
            var alphaFile = DIR + "alpha_" + dts + ".txt";
            var sigmaFile = DIR + "sigma_" + dts + ".txt";

            File.WriteAllText(valsFile, String.Join("\n", vals));

            Process p = new Process();
            p.StartInfo.FileName = @"C:\Users\ynovick\Anaconda2\python.exe";
            p.StartInfo.Arguments = DIR + "alphacalc.py" + " " + valsFile + " " + alphaFile + " " + sigmaFile;
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
