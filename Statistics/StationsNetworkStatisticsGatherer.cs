using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations
{
    public class StationsNetworkStatisticsGatherer
    {
        private readonly FuelStationsNetwork _trackingNetwork;
        private readonly Dictionary<FuelStation, StationStatisticsGatherer> _stationsStatistics = new();

        public StationsNetworkStatisticsGatherer(FuelStationsNetwork trackingNetwork)
        {
            if (trackingNetwork == null)
                throw new ArgumentNullException(nameof(trackingNetwork));
            _trackingNetwork = trackingNetwork;
            foreach (var station in _trackingNetwork.Stations)
            {
                _stationsStatistics[station] = new StationStatisticsGatherer(station);
            }
        }

        public StationStatisticsGatherer[] Stations
            => _trackingNetwork.Stations
            .Select(s => _stationsStatistics[s])
            .ToArray();

        public StationStatisticsGatherer[] GetStationsByType(StationType stationType)
            => Stations
            .Where(s => s.StationType == stationType)
            .ToArray();

        public StationStatisticsGatherer GetStatisticsForStation(FuelStation station)
            => _stationsStatistics[station];

        public float GetTotalRevenueByStationType(StationType stationType)
            => GetStationsByType(stationType).Sum(s => s.StationRevenue);

        public int GetTotalSuccessfullyServedOrdersByStationType(StationType stationType)
            => GetStationsByType(stationType).Sum(s => s.SuccessfullyServedClients);
    }
}
