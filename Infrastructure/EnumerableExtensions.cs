using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations.Infrastructure
{
    public static class EnumerableExtensions
    {
        public static T TakeRandom<T>(
            this IEnumerable<T> collection, Random randomizer, bool randomizeOnSingleElement = false)
        {
            var list = collection.ToList();
            if (list.Count == 0)
                throw new InvalidOperationException("Collection is empty.");
            if (list.Count == 1 && !randomizeOnSingleElement)
                return list[0];
            var randomIndex = randomizer.Next(list.Count);
            return list[randomIndex];
        }
    }
}
