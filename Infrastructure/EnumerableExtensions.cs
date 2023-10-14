using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations.Infrastructure
{
    public static class EnumerableExtensions
    {
        public static T TakeRandom<T>(this IEnumerable<T> collection, Random randomizer)
        {
            var list = collection.ToList();
            if (list.Count == 0)
                throw new InvalidOperationException("Collection is empty.");
            var randomIndex = randomizer.Next(list.Count);
            return list[randomIndex];
        }
    }
}
