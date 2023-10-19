using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations
{
    public class StationStatisticsGatherer
    {
        private readonly GasStation _trackingStation;
        private readonly List<ServedOrder> _servedOrders = new();

        public StationStatisticsGatherer(GasStation trackingStation)
        {
            if (trackingStation == null)
                throw new ArgumentNullException(nameof(trackingStation));
            _trackingStation = trackingStation;
            //_trackingStation.OrderQueued += OnOrderQueued;
            _trackingStation.OrderServed += OnOrderServed;
            _trackingStation.FuelVolumesRefillRequested += OnRefillRequested;
        }

        public GasStation StationModel => _trackingStation;
        public StationType StationType => _trackingStation.StationType;
        public IReadOnlyDictionary<FuelType, IReadOnlyFuelContainer> AvailableFuel => _trackingStation.AvailableFuel;
        public IReadOnlyList<ServedOrder> ServedOrders => _servedOrders;

        public int RefillRequestsCount { get; private set; }
        public bool IsWaitingForGasolineTanker => _trackingStation.IsWaitingForGasolineTanker;
        public float StationRevenue => _servedOrders.Sum(o => o.OrderRevenue);
        public int SuccessfullyServedClients => _servedOrders.Where(o => o.IsSuccessful).Count();

        private void OnOrderServed(GasStation station, ServedOrder order)
        {
            if (station != _trackingStation)
                throw new InvalidProgramException();
            _servedOrders.Add(order);
        }

        private void OnRefillRequested(GasStation station, IReadOnlyDictionary<FuelType, int> fuelSections)
        {
            if (station != _trackingStation)
                throw new InvalidProgramException();
            RefillRequestsCount++;
        }
    }
}
