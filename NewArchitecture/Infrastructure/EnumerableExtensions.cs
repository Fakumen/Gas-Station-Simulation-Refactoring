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
            var randomIndex = randomizer.Next(0, list.Count);
            return list[randomIndex];
        }
    }
}
