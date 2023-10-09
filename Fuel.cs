using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasStations
{
    public class Fuel
    {
        public string FuelTypeName;
        public float Cost;

        public Fuel(string name, float cost)
        {
            FuelTypeName = name;
            Cost = cost;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Fuel))
                return false;
            return FuelTypeName == ((Fuel)obj).FuelTypeName;
        }

        public override int GetHashCode()
            => FuelTypeName.GetHashCode();

        public static bool operator ==(Fuel f1, Fuel f2)
        {
            if (f1 is null || f2 is null)
                return f1 is null && f2 is null;
            return f1.Equals(f2);
        }
        public static bool operator !=(Fuel f1, Fuel f2)
            => !(f1 == f2);
    }
}
