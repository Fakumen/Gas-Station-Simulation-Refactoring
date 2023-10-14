using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasStations
{
    public class OrderedFuel
    {
        public readonly GasStation OwnerStation;
        public readonly FuelType FuelType;

        public OrderedFuel(GasStation owner, FuelType fuelType)
        {
            OwnerStation = owner;
            FuelType = fuelType;
        }
    }
}
