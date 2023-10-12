using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasStations
{
    public class SimulationParameters
    {
        public Random Randomizer { get; }
        public int FuelTankerVolumeCapacityInLiters { get; } = 6000;
        public long PetrolStationRefillIntervalInMinutes { get; } = 24 * 60;
        public int PetrolStationCriticalFuelVolumeInLiters { get; } = 1000;
        public long TotalSimulationTimeInMinutes { get; } = 10 * 24 * 60;
    }
}
