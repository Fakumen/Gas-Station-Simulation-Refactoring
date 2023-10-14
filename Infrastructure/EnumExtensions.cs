using System;
using System.Linq;

namespace GasStations.Infrastructure
{
    public static class EnumExtensions
    {
        public static T[] GetValues<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        }
    }
}
