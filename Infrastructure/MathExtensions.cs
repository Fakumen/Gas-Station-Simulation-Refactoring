using System;

namespace GasStations.Infrastructure
{
    public static class MathExtensions
    {
        public static int Clamp(int value, int min, int max)
        {
            return Math.Max(Math.Min(value, max), min);
        }
    }
}
