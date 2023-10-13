using GasStations.Infrastructure;
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
        private readonly Dictionary<ClientType, ClientOrder> _currentClientOrders = new();
        private int _ticksPassed = 0;
        private readonly List<ServedOrder> _servedOrders = new();

        #region Statistics
        public float Revenue => _servedOrders.Sum(o => o.OrderRevenue);
        public int ServedClients => _servedOrders.Where(o => o.IsSuccessful).Count();
        public int GasolineTankersCalls { get; private set; }
        public Dictionary<ClientType, int> OrdersIntervalSum
            = EnumExtensions.GetValues<ClientType>().ToDictionary(c => c, c => 0);
        public Dictionary<ClientType, int> ServedOrdersByClientType
            = EnumExtensions.GetValues<ClientType>().ToDictionary(c => c, c => 0);

        //TODO: Incorrect behaviour. Counts when order queued! Not Appeared!
        public int TotalOrders => TotalOrdersByClientType.Sum(kv => kv.Value);
        public Dictionary<ClientType, int> TotalOrdersByClientType
            = EnumExtensions.GetValues<ClientType>().ToDictionary(c => c, c => 0);
        #endregion

        public int ScheduleRefillInterval { get; }
        public int CriticalFuelLevel { get; }
        public StationType StationType { get; }

        public bool IsWaitingForGasolineTanker => _availableFuel.Any(e => e.Value.ReservedVolume > 0);
        public bool IsRequireGasolineTanker
            => _availableFuel.Any(e => e.Value.EmptyUnreservedSpace >= GasolineTanker.TankCapacity);
        public bool IsExpectingOrder(ClientType clientType) 
            => _currentClientOrders.ContainsKey(clientType) && _currentClientOrders[clientType] != null;
        //public bool IsExpectingCarOrder => _currentCarOrder != null;
        //public bool IsExpectingTruckOrder => _currentTruckOrder != null;

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

        public void WaitOneTick()
        {
            if (_ticksPassed % ScheduleRefillInterval == 0 && _ticksPassed != 0)
                ScheduleRefillIntervalPassed?.Invoke(this);
            foreach (var order in _currentClientOrders.Values.ToArray())
            {
                order.WaitOneTick();
            }
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

        public void AddOrderInQueue(ClientOrder order)
        {
            var client = order.ClientType;

            if (!_currentClientOrders.ContainsKey(client))
                _currentClientOrders.Add(client, null);

            if (_currentClientOrders[client] != null)
                throw new InvalidOperationException();
            _currentClientOrders[client] = order;
            OrdersIntervalSum[client] += order.OrderAppearInterval;
            TotalOrdersByClientType[client]++;
            order.OrderAppeared += OnOrderAppeared;
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

            //TODO: move check to statistics
            if (servedOrder.IsSuccessful)
                ServedOrdersByClientType[client]++;
        }

        private ServedOrder ServeOrder(ClientOrder order)
        {
            var requestedFuel = order.GetRequestedFuel(_availableFuel);
            var providedVolume = Math.Min(
                order.RequestedFuelVolume, _availableFuel[requestedFuel].CurrentVolume);
            _availableFuel[requestedFuel].Take(providedVolume);
            CheckCriticalFuelLevel(requestedFuel);
            var servedOrder = new ServedOrder(order, providedVolume, _fuelPrices[requestedFuel]);
            //OrderServed?.Invoke(this, servedOrder); //TODO: add event, track in statistics?
            _servedOrders.Add(servedOrder);
            return servedOrder;
        }

        private void CheckCriticalFuelLevel(FuelType fuelToCheck)
        {
            if (_availableFuel[fuelToCheck].CurrentVolume <= CriticalFuelLevel)
                CriticalFuelLevelReached?.Invoke(this, fuelToCheck);
        }
    }
}
