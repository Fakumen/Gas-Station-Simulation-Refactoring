using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasStations
{
    public enum StationType
    {
        Stationary,
        Mini
    }

    public class GasStation : IWaiter
    {
        private readonly Dictionary<FuelType, float> _fuelPrices;
        private readonly Dictionary<FuelType, FuelContainer> _availableFuel;

        private CarClientOrder _currentCarOrder;
        private TruckClientOrder _currentTruckOrder;
        private int _ticksPassed = 0;

        #region Statistics
        public float Revenue { get; private set; }
        public int TotalOrders => TotalCarOrders + TotalTruckOrders;
        public int TotalCarOrders { get; private set; }
        public int TotalTruckOrders { get; private set; }
        public int ServedClients => ServedCarClients + ServedTruckClients;
        public int ServedCarClients { get; private set; }
        public int ServedTruckClients { get; private set; }
        public int CarOrdersIntervalSum { get; private set; }
        public int TruckOrdersIntervalSum { get; private set; }
        public int GasolineTankersCalls { get; private set; }
        #endregion

        public int ScheduleRefillInterval { get; }
        public int CriticalFuelLevel { get; }
        public StationType StationType { get; }

        public bool IsWaitingForGasolineTanker => _availableFuel.Any(e => e.Value.ReservedVolume > 0);
        public bool IsRequireGasolineTanker
            => _availableFuel.Any(e => e.Value.EmptyUnreservedSpace >= GasolineTanker.TankCapacity);
        public bool IsExpectingCarOrder => _currentCarOrder != null;
        public bool IsExpectingTruckOrder => _currentTruckOrder != null;

        public IReadOnlyDictionary<FuelType, FuelContainer> AvailableFuel => _availableFuel;

        public event Action<GasStation> ScheduleRefillIntervalPassed;
        public event Action<GasStation, FuelType> CriticalFuelLevelReached;

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

        /// <summary>
        /// Возвращает общий список нужного в цистернах топлива по одному элементу для каждой цистерны.
        /// </summary>
        public List<FuelType> GetFuelToRefillList()
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
            return result;
        }

        public void WaitOneTick()
        {
            if (_ticksPassed % ScheduleRefillInterval == 0 && _ticksPassed != 0)
                ScheduleRefillIntervalPassed?.Invoke(this);
            _currentCarOrder.WaitOneTick();
            _currentTruckOrder.WaitOneTick();
            _ticksPassed++;
        }

        public void ConfirmGasolineOrder()
        {
            if (!IsRequireGasolineTanker) throw new InvalidOperationException();
            GasolineTankersCalls++;
            foreach (var fuel in GetFuelToRefillList())
            {
                _availableFuel[fuel].ReserveSpace(GasolineTanker.TankCapacity);
            }
        }

        public void Refill(FuelType fuelToRefill, int amount)
        {
            if (amount < 0) throw new ArgumentException();
            _availableFuel[fuelToRefill].Fill(amount);
        }

        public void AddOrderInQueue(CarClientOrder order)
        {
            if (_currentCarOrder != null) throw new InvalidOperationException();
            _currentCarOrder = order;
            CarOrdersIntervalSum += order.OrderAppearTime;
            TotalCarOrders++;
            order.OrderAppeared += OnCarOrderAppeared;
        }
        public void AddOrderInQueue(TruckClientOrder order)
        {
            if (_currentTruckOrder != null) throw new InvalidOperationException();
            _currentTruckOrder = order;
            TruckOrdersIntervalSum += order.OrderAppearTime;
            TotalTruckOrders++;
            order.OrderAppeared += OnTruckOrderAppeared;
        }

        private void OnCarOrderAppeared()
        {
            _currentCarOrder.OrderAppeared -= OnCarOrderAppeared;
            ServeOrder(_currentCarOrder, out var cost);
            _currentCarOrder = null;
            Revenue += cost;
            if (cost > 0)
                ServedCarClients++;
        }

        private void OnTruckOrderAppeared()
        {
            _currentTruckOrder.OrderAppeared -= OnTruckOrderAppeared;
            ServeOrder(_currentTruckOrder, out var cost);
            _currentTruckOrder = null;
            Revenue += cost;
            if (cost > 0)
                ServedTruckClients++;
        }

        private void ServeOrder(ClientOrder order, out float orderCost)
        {
            if (order.TicksUntilOrderAppear > 0) throw new InvalidOperationException();
            var requestedFuel = order.GetRequestedFuel(_availableFuel);
            var requestedVolume = order.GetRequestedVolume(_availableFuel[requestedFuel].CurrentVolume);
            _availableFuel[requestedFuel].Take(requestedVolume);
            orderCost = requestedVolume * _fuelPrices[requestedFuel];
            CheckCriticalFuelLevel(requestedFuel);
        }

        private void CheckCriticalFuelLevel(FuelType fuelToCheck)
        {
            if (_availableFuel[fuelToCheck].CurrentVolume <= CriticalFuelLevel)
                CriticalFuelLevelReached?.Invoke(this, fuelToCheck);
        }
    }
}
