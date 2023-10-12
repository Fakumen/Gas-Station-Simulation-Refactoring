using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasStations
{
    public class ClientOrderInfo
    {
        public ClientOrderInfo(
            ClientType clientType, FuelType requestedFuelType, int requestedFuelVolume)
        {
            ClientType = clientType;
            RequestedFuelType = requestedFuelType;
            RequestedFuelVolume = requestedFuelVolume;
        }

        //Lazy Constructor

        public ClientType ClientType { get; }
        public FuelType RequestedFuelType { get; }
        public int RequestedFuelVolume { get; }
    }
}
