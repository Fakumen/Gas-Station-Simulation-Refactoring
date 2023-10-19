using GasStations.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations
{
    public class OrderProvider : ISimulationEntity
    {
        private Dictionary<GasStation, Dictionary<ClientType, ClientOrder>> _stationOrders = new();

        public event Action<GasStation, ClientOrder> OrderQueued;

        public void OnSimulationTickPassed()
        {
            foreach (var order in _stationOrders.Values.SelectMany(d => d.Values).ToArray())
            {
                order.OnSimulationTickPassed();
            }
        }

        [Obsolete(
            "Used to simulate behaviour of previous versions. " +
            "Usage of " + nameof(OnSimulationTickPassed) + " is recommended instead.")]
        public void OnSimulationTickPassedForStation(GasStation station)
        {
            foreach (var order in _stationOrders[station].Values.ToArray())
            {
                order.OnSimulationTickPassed();
            }
        }

        public void ProvideOrdersToStation(GasStation station)
        {
            if (!_stationOrders.ContainsKey(station))
                _stationOrders.Add(station, new());
            foreach (var clientType in EnumExtensions.GetValues<ClientType>())
            {
                if (_stationOrders[station].ContainsKey(clientType) 
                    && _stationOrders[station][clientType] != null)
                    continue;
                var newOrder = clientType switch
                {
                    ClientType.Car => new ClientOrder(
                        clientType,
                        r => r.Next(1, 6),
                        r => r.Next(10, 51),
                        (r, f) => f.Keys.TakeRandom(r)),
                    ClientType.Truck => new ClientOrder(
                        clientType,
                        r => r.Next(1, 13),
                        r => r.Next(30, 301),
                        (r, f) => f.Keys
                        .Where(f => f == FuelType.Petrol92 || f == FuelType.Diesel)
                        .TakeRandom(r)),
                    _ => throw new NotImplementedException("No order parameters for such ClientType"),
                };
                AddOrderInQueue(station, newOrder);
            }
        }

        private void AddOrderInQueue(GasStation station, ClientOrder order)
        {
            var client = order.ClientType;
            var stationOrders = _stationOrders[station];

            if (!stationOrders.ContainsKey(client))
                stationOrders.Add(client, null);

            if (stationOrders[client] != null)
                throw new InvalidOperationException();
            stationOrders[client] = order;
            order.OrderAppeared += OnOrderAppeared;
            OrderQueued?.Invoke(station, order);

            void OnOrderAppeared(ClientOrder order)
            {
                var client = order.ClientType;
                stationOrders[client].OrderAppeared -= OnOrderAppeared;
                station.ServeOrder(stationOrders[client]);
                stationOrders[client] = null;
            }
        }
    }
}
