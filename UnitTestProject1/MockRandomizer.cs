using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphLibyn;

namespace UnitTestProject1
{
    // A mock object to allow the caller to specify the next n bool values that will be retrieved and then get them one-by-one
    public class MockRandomizer : IRandomizer
    {
        private Queue<bool> BoolValues;
        private double nextDouble;
        private int nextInt;

        // The queue is always reset completely, whenever the object is used the caller should set all of the values
        // that will be needed for their test scenario, then retrieve them. This is necessary because there are times
        // where one operation will cause multiple calls to GetTrueWithProbability.
        public void SetUpcomingBools(IEnumerable<bool> vals)
        {
            BoolValues = new Queue<bool>(vals);
        }

        // A different setter for the simpler case where just one value needs to be set and then retrieved
        public void SetNextBool(bool val)
        {
            SetUpcomingBools(new [] {val});
        }

        public bool GetTrueWithProbability(double p)
        {
            return BoolValues.Dequeue();
        }

        public void SetNextDouble(double d)
        {
            nextDouble = d;
        }

        public double NextDouble()
        {
            return nextDouble;
        }

        public void SetNextInt(int i)
        {
            nextInt = i;
        }

        public int Next()
        {
            return nextInt;
        }

        public int Next(int maxValue)
        {
            if (nextInt >= maxValue)
                throw new Exception($"Mock object preloaded with value {nextInt} greater or equal than specified max {maxValue}.");
            return nextInt;
        }

        public int Next(int minValue, int maxValue)
        {
            if (nextInt < minValue || nextInt <= maxValue)
                throw new Exception($"Mock object preloaded with value {nextInt} out of specified range ({minValue}, {maxValue}).");
            return nextInt;
        }
    }
}
