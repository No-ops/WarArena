using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WarArena.Utilities
{
    static class RandomizationFunctions
    {
        static Random random = new Random();

        public static bool Chance(int percentProbability)
        {
            return percentProbability >= random.Next(1, 101);
        }

        public static int GetRandomNumber(int lowerBound, int upperBound)
        {
            return random.Next(lowerBound, upperBound + 1);
        }
    }

}
