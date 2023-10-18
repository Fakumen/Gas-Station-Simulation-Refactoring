using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasStations
{
    public class SimulationStatisticsGatherer
    {
        public SimulationStatisticsGatherer(Simulation trackingSimulation)
        {
            TrackingSimulation = trackingSimulation;
            OrdersAppearStatistics 
                = new OrdersAppearStatisticsGatherer(trackingSimulation.OrderProvider);
            StationsNetworkStatistics 
                = new StationsNetworkStatisticsGatherer(trackingSimulation.StationsNetwork);
        }

        public Simulation TrackingSimulation { get; }
        public StationsNetworkStatisticsGatherer StationsNetworkStatistics { get; }
        public OrdersAppearStatisticsGatherer OrdersAppearStatistics { get; }
    }
}
