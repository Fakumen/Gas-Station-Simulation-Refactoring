using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations
{
    public enum StationType
    {
        Stationary,
        Mini
    }

    public class GasStation : ISimulationEntity
    {
        private readonly Dictionary<FuelType, float> _fuelPrices;
        private readonly Dictionary<FuelType, FuelContainer> _availableFuel;
        private readonly Dictionary<ClientType, ClientOrder> _currentClientOrders = new();
        private int _ticksPassed = 0;

        public int ScheduleRefillInterval { get; }
        public int CriticalFuelLevel { get; }
        public StationType StationType { get; }
        public IReadOnlyDictionary<FuelType, FuelContainer> AvailableFuel => _availableFuel;

        public bool IsWaitingForGasolineTanker => _availableFuel.Any(e => e.Value.ReservedVolume > 0);
        public bool IsRequireGasolineTanker
            => _availableFuel.Any(e => e.Value.EmptyUnreservedSpace >= GasolineTanker.TankCapacity);

        public event Action<GasStation> ScheduleRefillIntervalPassed;
        public event Action<GasStation, FuelType> CriticalFuelLevelReached;
        public event Action<GasStation, ClientOrder> OrderQueued;
        public event Action<GasStation, ServedOrder> OrderServed;
        public event Action<GasStation> FuelTankerCalled;

        public GasStation(
            StationType stationType, 
            IReadOnlyDictionary<FuelType, float> fuelPrices,
            IReadOnlyDictionary<FuelType, int> fuelSectionVolumes, 
            int refillInterval = 24 * 60, 
            int criticalFuelLevel = 1000)
        {
            StationType = stationType;
            _fuelPrices = fuelPrices.ToDictionary(kv => kv.Key, kv => kv.Value);
            _availableFuel = fuelSectionVolumes.ToDictionary(
                kv => kv.Key, 
                kv => new FuelContainer(kv.Value));
            ScheduleRefillInterval = refillInterval;
            CriticalFuelLevel = criticalFuelLevel;
        }

        public bool IsExpectingOrder(ClientType clientType)
            => _currentClientOrders.ContainsKey(clientType) && _currentClientOrders[clientType] != null;

        /// <summary>
        /// Возвращает общий список нужного в цистернах топлива по одному элементу для каждой цистерны.
        /// </summary>
        public FuelType[] GetFuelToRefillList()
        {
            var result = new List<FuelType>();
            var gasolineTankerCapacity = GasolineTanker.TankCapacity;
            foreach (var container in _availableFuel)
            {
                var emptyUnreservedSpace = container.Value.EmptyUnreservedSpace;
                if (emptyUnreservedSpace >= gasolineTankerCapacity)
                {
                    for (var i = 0; i < emptyUnreservedSpace / gasolineTankerCapacity; i++)
                        result.Add(container.Key);
                }
            }
            return result.ToArray();
        }

        public void OnSimulationTickPassed()
        {
            if (_ticksPassed % ScheduleRefillInterval == 0 && _ticksPassed != 0)
                ScheduleRefillIntervalPassed?.Invoke(this);
            foreach (var order in _currentClientOrders.Values.ToArray())
            {
                order.OnSimulationTickPassed();
            }
            _ticksPassed++;
        }

        public void ConfirmGasolineOrder()
        {
            if (!IsRequireGasolineTanker) throw new InvalidOperationException();
            FuelTankerCalled?.Invoke(this);
            foreach (var fuel in GetFuelToRefillList())
            {
                _availableFuel[fuel].ReserveVolume(GasolineTanker.TankCapacity);
            }
        }

        public void Refill(FuelType fuelToRefill, int amount)
        {
            if (amount < 0) throw new ArgumentException();
            _availableFuel[fuelToRefill].Fill(amount);
        }

        public void AddOrderInQueue(ClientOrder order)
        {
            var client = order.ClientType;

            if (!_currentClientOrders.ContainsKey(client))
                _currentClientOrders.Add(client, null);

            if (_currentClientOrders[client] != null)
                throw new InvalidOperationException();
            _currentClientOrders[client] = order;
            order.OrderAppeared += OnOrderAppeared;
            OrderQueued?.Invoke(this, order);
        }

        private void OnOrderAppeared(ClientOrder order)
        {
            var client = order.ClientType;
            var currentOrder = _currentClientOrders[client];
            if (currentOrder != order)
                throw new InvalidProgramException();

            currentOrder.OrderAppeared -= OnOrderAppeared;
            var servedOrder = ServeOrder(currentOrder);
            _currentClientOrders[client] = null;
        }

        private ServedOrder ServeOrder(ClientOrder order)
        {
            var requestedFuel = order.GetRequestedFuel(_availableFuel);
            var providedVolume = Math.Min(
                order.RequestedFuelVolume, _availableFuel[requestedFuel].CurrentVolume);
            _availableFuel[requestedFuel].Take(providedVolume);
            CheckCriticalFuelLevel(requestedFuel);
            var servedOrder = new ServedOrder(order, providedVolume, _fuelPrices[requestedFuel]);
            OrderServed?.Invoke(this, servedOrder);
            return servedOrder;
        }

        private void CheckCriticalFuelLevel(FuelType fuelToCheck)
        {
            if (_availableFuel[fuelToCheck].CurrentVolume <= CriticalFuelLevel)
                CriticalFuelLevelReached?.Invoke(this, fuelToCheck);
        }
    }
}
