using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GraphLibyn
{
    public class Randomizer : IRandomizer
    {
        private Random random;

        public Randomizer() : this(new Random())
        {
        }

        // In some multithreading situations it might be more "random" to give each object it's own random
        // that can be seeded by one global random object
        public Randomizer(Random r)
        {
            random = r;
        }

        public bool GetTrueWithProbability(double p)
        {
            if (p < 0.0 || p >= 1.0)
                throw new Exception($"Invalid range for random probability {p}.");

            return random.NextDouble() < p;
        }

        public double NextDouble()
        {
            return random.NextDouble();
        }

        public int Next()
        {
            return random.Next();
        }

        public int Next(int maxValue)
        {
            return random.Next(maxValue);
        }

        public int Next(int minValue, int maxValue)
        {
            return random.Next(minValue, maxValue);
        }

        // Returns a new Randomizer object seeded by the next int of this object. Allows multiple threads
        // to have Randomizer objects that are seeded randomly instead of all by the same clock time
        public Randomizer NextRandomizer()
        {
            return new Randomizer(new Random(this.Next()));
        }
    }
}
