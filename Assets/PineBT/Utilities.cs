using System.Collections.Generic;

namespace PineBT
{
    public static class Utilities
    {
        private static System.Random random = new System.Random();

        /// <summary>Shuffles List elements using Fisher–Yates Shuffle.</summary>
        public static void Shuffle<T>(this IList<T> list)
        {
            int size = list.Count;
            while (size > 1)
            {
                size--;
                int val = random.Next(size + 1);
                T value = list[val];
                list[val] = list[size];
                list[size] = value;
            }
        }
    }
}

