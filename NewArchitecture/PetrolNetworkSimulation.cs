using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasStations
{
    public class PetrolNetworkSimulation
    {
        private List<IPetrolStation> _stations;

        public SimulationParameters Parameters { get; }
        public IReadOnlyList<IPetrolStation> Stations => _stations;
        public long CurrentTimeInMinutes { get; private set; }

        public event Action<PetrolNetworkSimulation> TimeIntervalPassed;

        //OrderGenerator.GenerateForStation(station)

        public void SpendTimeInterval()
        {
            //foreach stations not waiting for orders (cars/trucks)
            //generate new order (car/truck)
            //serve orders
            //spend one tick for trucks
        }
    }
}
