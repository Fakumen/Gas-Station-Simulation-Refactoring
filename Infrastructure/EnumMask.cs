using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GasStations.Infrastructure
{
    public class EnumMask<T> where T : Enum
    {
        private readonly Dictionary<T, bool> _allowedValues = new();

        public static EnumMask<T> Full => GetFilledInstance(true);
        public static EnumMask<T> Empty => GetFilledInstance(false);

        public EnumMask(IReadOnlyDictionary<T, bool> allowedValues)
        {
            foreach (var value in EnumExtensions.GetValues<T>())
            {
                if (!allowedValues.ContainsKey(value))
                    _allowedValues.Add(value, false);
                else
                    _allowedValues.Add(value, allowedValues[value]);
            }
        }

        public EnumMask(IEnumerable<T> allowedValues)
        {
            var allowedValuesHash = allowedValues.ToHashSet();
            foreach (var value in EnumExtensions.GetValues<T>())
            {
                _allowedValues.Add(value, allowedValuesHash.Contains(value));
            }
        }

        public bool this[T enumValue]
        {
            get => _allowedValues[enumValue];
            set => _allowedValues[enumValue] = value;
        }

        private static EnumMask<T> GetFilledInstance(bool value)
            => new(EnumExtensions.GetValues<T>().ToDictionary(e => e, e => value));
    }
}
