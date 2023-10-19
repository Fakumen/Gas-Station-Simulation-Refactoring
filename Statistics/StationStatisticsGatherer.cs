using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations
{
    public class StationStatisticsGatherer
    {
        private readonly FuelStation _trackingStation;
        private readonly List<ServedOrder> _servedOrders = new();

        public StationStatisticsGatherer(FuelStation trackingStation)
        {
            if (trackingStation == null)
                throw new ArgumentNullException(nameof(trackingStation));
            _trackingStation = trackingStation;
            _trackingStation.OrderServed += OnOrderServed;
            _trackingStation.FuelVolumesRefillRequested += OnRefillRequested;
        }

        public FuelStation StationModel => _trackingStation;
        public StationType StationType => _trackingStation.StationType;
        public IReadOnlyDictionary<FuelType, IReadOnlyFuelContainer> AvailableFuel => _trackingStation.AvailableFuel;
        public IReadOnlyList<ServedOrder> ServedOrders => _servedOrders;

        public int RefillRequestsCount { get; private set; }
        public bool HasReservedFuelVolumes => _trackingStation.HasReservedFuelVolumes;
        public float StationRevenue => _servedOrders.Sum(o => o.OrderRevenue);
        public int SuccessfullyServedClients => _servedOrders.Where(o => o.IsSuccessful).Count();

        private void OnOrderServed(FuelStation station, ServedOrder order)
        {
            if (station != _trackingStation)
                throw new InvalidProgramException();
            _servedOrders.Add(order);
        }

        private void OnRefillRequested(FuelStation station, IReadOnlyDictionary<FuelType, int> fuelSections)
        {
            if (station != _trackingStation)
                throw new InvalidProgramException();
            RefillRequestsCount++;
        }
    }
}
