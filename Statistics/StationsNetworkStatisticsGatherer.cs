using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations
{
    public class StationsNetworkStatisticsGatherer
    {
        private readonly GasStationSystem _trackingNetwork;
        private readonly Dictionary<GasStation, StationStatisticsGatherer> _stationsStatistics = new();

        public StationsNetworkStatisticsGatherer(GasStationSystem trackingNetwork)
        {
            if (trackingNetwork == null)
                throw new ArgumentNullException(nameof(trackingNetwork));
            _trackingNetwork = trackingNetwork;
            foreach (var station in _trackingNetwork.GasStations)
            {
                _stationsStatistics[station] = new StationStatisticsGatherer(station);
            }
        }

        public IEnumerable<StationStatisticsGatherer> GasStations
            => _trackingNetwork.GasStations.Select(s => _stationsStatistics[s]);

        public StationStatisticsGatherer GetStatisticsForStation(GasStation station)
            => _stationsStatistics[station];
    }
}
