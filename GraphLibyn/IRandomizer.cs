using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphLibyn
{
    public interface IRandomizer
    {
        bool GetTrueWithProbability(double p);
        double NextDouble();
        int Next();
        int Next(int maxValue);
        int Next(int minValue, int maxValue);
    }
}
