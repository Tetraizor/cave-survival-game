using System;

namespace CaveGame.Utils
{
    public static class RandomHelpers
    {
        public static Random RandomInstance { get; private set; }

        public static void SetRandomInstance(Random random)
        {
            RandomInstance = random;
        }

        public static T GetRandomElement<T>(this T[] array)
        {
            if (array == null || array.Length == 0)
                throw new ArgumentException("Array cannot be null or empty.");

            int index = RandomInstance.Next(array.Length);
            return array[index];
        }

        public static T GetRandomElementWithTable<T>(this T[] array, float[] probabilityTable, out int index)
        {
            if (array == null || array.Length == 0)
                throw new ArgumentException("Array cannot be null or empty.");

            if (probabilityTable == null || probabilityTable.Length != array.Length)
                throw new ArgumentException("Probability table must be the same length as the array.");

            float total = 0;
            foreach (var prob in probabilityTable)
            {
                if (prob < 0)
                    throw new ArgumentException("Probability values cannot be negative.");
                total += prob;
            }

            if (total <= 0)
                throw new ArgumentException("Sum of probability values must be greater than zero.");

            double randomValue = RandomInstance.NextDouble() * total;
            double cumulative = 0;

            for (int i = 0; i < array.Length; i++)
            {
                cumulative += probabilityTable[i];
                if (randomValue < cumulative)
                {
                    index = i;
                    return array[i];
                }
            }

            index = array.Length - 1;
            return array[index];
        }
    }
}