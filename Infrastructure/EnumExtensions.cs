using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations.Infrastructure
{
    public static class EnumExtensions
    {
        //generic class?
        public static FuelType[] FuelTypes { get; } = GetValues<FuelType>();
        public static ClientType[] ClientTypes { get; } = GetValues<ClientType>();

        public static T[] GetValues<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        }
    }
}
