using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketRisk.Testing
{
    public class ListUtils
    {
        /// <summary>
        /// Optimization Problem: Selecting all possible combinations from a list
        /// </summary>
        /// <typeparam name="T">type of list</typeparam>
        /// <param name="source">input list</param>
        /// <returns>all possible combinations that can be selected from the list</returns>
        public static IEnumerable<T[]> Combinations<T>(IEnumerable<T> source)
        {
            if (null == source)
                throw new ArgumentNullException(nameof(source));

            T[] data = source.ToArray();

            return Enumerable
              .Range(0, 1 << (data.Length))
              .Select(index => data
                 .Where((v, i) => (index & (1 << i)) != 0)
                 .ToArray());
        }
    }
}
