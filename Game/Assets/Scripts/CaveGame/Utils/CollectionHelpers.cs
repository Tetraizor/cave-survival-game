using System;
using System.Collections;
using System.Collections.Generic;

namespace CaveGame.Utils
{
    public static class CollectionHelpers
    {
        private static Random _rand = new Random();

        public static void Shuffle<T>(this IList<T> list, Random random = null)
        {
            if (random == null) random = _rand;

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}