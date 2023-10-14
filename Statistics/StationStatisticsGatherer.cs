using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations
{
    public class StationStatisticsGatherer
    {
        private readonly GasStation _trackingStation;
        private readonly List<ClientOrder> _queuedOrders = new();
        private readonly List<ServedOrder> _servedOrders = new();

        public StationStatisticsGatherer(GasStation trackingStation)
        {
            if (trackingStation == null)
                throw new ArgumentNullException(nameof(trackingStation));
            _trackingStation = trackingStation;
            _trackingStation.OrderQueued += OnOrderQueued;
            _trackingStation.OrderServed += OnOrderServed;
            _trackingStation.FuelVolumesRefillRequested += OnRefillRequested;
        }

        public StationType StationType => _trackingStation.StationType;
        public IReadOnlyDictionary<FuelType, FuelContainer> AvailableFuel => _trackingStation.AvailableFuel;
        public IReadOnlyList<ClientOrder> QueuedOrders => _queuedOrders;
        public IReadOnlyList<ServedOrder> ServedOrders => _servedOrders;

        public int RefillRequestsCount { get; private set; }
        public bool IsWaitingForGasolineTanker => _trackingStation.IsWaitingForGasolineTanker;
        public float StationRevenue => _servedOrders.Sum(o => o.OrderRevenue);
        public int SuccessfullyServedClients => _servedOrders.Where(o => o.IsSuccessful).Count();

        public IReadOnlyDictionary<ClientType, int> OrdersIntervalSum
            => _queuedOrders
            .GroupBy(o => o.ClientType)
            .ToDictionary(g => g.Key, g => g.Sum(o => o.OrderAppearInterval));

        //TODO: Incorrect behaviour. Counts when order queued! Not Appeared!
        public int TotalQueuedOrders => _queuedOrders.Count;
        public IReadOnlyDictionary<ClientType, int> TotalQueuedOrdersByClientType
            => _queuedOrders
            .GroupBy(o => o.ClientType)
            .ToDictionary(g => g.Key, g => g.Count());

        private void OnOrderQueued(GasStation station, ClientOrder order)
        {
            if (station != _trackingStation)
                throw new InvalidProgramException();
            _queuedOrders.Add(order);
        }

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
