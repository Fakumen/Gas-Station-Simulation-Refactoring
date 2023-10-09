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

        public readonly Dictionary<Fuel, FuelContainer> AvailableFuel = new Dictionary<Fuel, FuelContainer>();
        public readonly int ScheduleRefillInterval;
        public readonly int CriticalFuelLevel;
        public readonly StationType StationType;
        public bool IsWaitingForGasolineTanker => AvailableFuel.Any(e => e.Value.ReservedVolume > 0);
        public bool IsRequireGasolineTanker
            => AvailableFuel.Any(e => e.Value.EmptyUnreservedSpace >= GasolineTanker.TankCapacity);
        public CarClientOrder CurrentCarOrder { get; private set; }
        public TruckClientOrder CurrentTruckOrder { get; private set; }
        public int TicksPassed { get; private set; } = 0;
        public event Action<GasStation> ScheduleRefillIntervalPassed;
        public event Action<GasStation, Fuel> CriticalFuelLevelReached;

        public GasStation(StationType stationType, Dictionary<Fuel, FuelContainer> availableFuel, int refillInterval = 24 * 60, int criticalFuelLevel = 1000)
        {
            StationType = stationType;
            ScheduleRefillInterval = refillInterval;
            CriticalFuelLevel = criticalFuelLevel;
            AvailableFuel = availableFuel;
        }

        /// <summary>
        /// Возвращает общий список нужного в цистернах топлива по одному элементу для каждой цистерны.
        /// </summary>
        public List<Fuel> GetFuelToRefillList()
        {
            var result = new List<Fuel>();
            var gasolineTankerCapacity = GasolineTanker.TankCapacity;
            foreach (var container in AvailableFuel)
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
            if (TicksPassed % ScheduleRefillInterval == 0 && TicksPassed != 0)
                ScheduleRefillIntervalPassed?.Invoke(this);
            CurrentCarOrder.WaitOneTick();
            CurrentTruckOrder.WaitOneTick();
            TicksPassed++;
        }

        public void ConfirmGasolineOrder()
        {
            if (!IsRequireGasolineTanker) throw new InvalidOperationException();
            GasolineTankersCalls++;
            foreach (var fuel in GetFuelToRefillList())
            {
                AvailableFuel[fuel].ReserveSpace(GasolineTanker.TankCapacity);
            }
        }

        public void Refill(Fuel fuelToRefill, int amount)
        {
            if (amount < 0) throw new ArgumentException();
            AvailableFuel[fuelToRefill].Fill(amount);
        }

        public void AddOrderInQueue(CarClientOrder order)
        {
            if (CurrentCarOrder != null) throw new InvalidOperationException();
            CurrentCarOrder = order;
            CarOrdersIntervalSum += order.OrderAppearTime;
            TotalCarOrders++;
            order.OrderAppeared += OnCarOrderAppeared;
        }
        public void AddOrderInQueue(TruckClientOrder order)
        {
            if (CurrentTruckOrder != null) throw new InvalidOperationException();
            CurrentTruckOrder = order;
            TruckOrdersIntervalSum += order.OrderAppearTime;
            TotalTruckOrders++;
            order.OrderAppeared += OnTruckOrderAppeared;
        }

        private void OnCarOrderAppeared()
        {
            CurrentCarOrder.OrderAppeared -= OnCarOrderAppeared;
            ServeOrder(CurrentCarOrder, out var cost);
            CurrentCarOrder = null;
            Revenue += cost;
            if (cost > 0)
                ServedCarClients++;
        }

        private void OnTruckOrderAppeared()
        {
            CurrentTruckOrder.OrderAppeared -= OnTruckOrderAppeared;
            ServeOrder(CurrentTruckOrder, out var cost);
            CurrentTruckOrder = null;
            Revenue += cost;
            if (cost > 0)
                ServedTruckClients++;
        }

        private void ServeOrder(ClientOrder order, out float orderCost)
        {
            if (order.TicksUntilOrderAppear > 0) throw new InvalidOperationException();
            var requestedFuel = order.GetRequestedFuel(AvailableFuel);
            var requestedVolume = order.GetRequestedVolume(AvailableFuel[requestedFuel].CurrentVolume);
            AvailableFuel[requestedFuel].Take(requestedVolume);
            orderCost = requestedVolume * requestedFuel.Cost;
            CheckCriticalFuelLevel(requestedFuel);
        }

        private void CheckCriticalFuelLevel(Fuel fuelToCheck)
        {
            if (AvailableFuel[fuelToCheck].CurrentVolume <= CriticalFuelLevel)
                CriticalFuelLevelReached?.Invoke(this, fuelToCheck);
        }
    }
}
