﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations
{
    //TODO: Return cashed statistics on request if nothing has changed since.
    [Obsolete("Counts QUEUED orders, not appeared! Used to simulate old behaviour.")]
    public class OrdersAppearStatisticsGatherer
    {
        private readonly Dictionary<FuelStation, List<ClientOrder>> _queuedOrders = new();

        public OrdersAppearStatisticsGatherer(OrderProvider orderProvider)
        {
            if (orderProvider == null)
                throw new ArgumentNullException(nameof(orderProvider));
            orderProvider.OrderQueued += OnOrderQueued;
        }

        [Obsolete("Counts QUEUED orders, not appeared! Used to simulate old behaviour.")]
        public int GetOrdersAppearIntervalSumByClientType(ClientType clientType)
            => GetQueuedOrdersByClientType(clientType).Sum(o => o.OrderAppearInterval);

        [Obsolete("Counts QUEUED orders, not appeared! Used to simulate old behaviour.")]
        public int GetQueuedOrdersCountByClientType(ClientType clientType)
            => GetQueuedOrdersByClientType(clientType).Count();

        [Obsolete("Counts QUEUED orders, not appeared! Used to simulate old behaviour.")]
        public IReadOnlyList<ClientOrder> GetQueuedOrdersByStation(FuelStation station)
            => _queuedOrders.ContainsKey(station)
            ? _queuedOrders[station]
            : new();

        [Obsolete("Counts QUEUED orders, not appeared! Used to simulate old behaviour.")]
        public ClientOrder[] GetQueuedOrdersByClientType(ClientType clientType)
            => _queuedOrders.Values
            .SelectMany(o => o)
            .Where(o => o.ClientType == clientType)
            .ToArray();

        [Obsolete("Counts QUEUED orders, not appeared! Used to simulate old behaviour.")]
        public float GetAverageOrdersIntervalByClientType(ClientType clientType)
            => (float)GetOrdersAppearIntervalSumByClientType(clientType)
            / GetQueuedOrdersCountByClientType(clientType);

        [Obsolete("Counts QUEUED orders, not appeared! Used to simulate old behaviour.")]
        public ClientOrder[] GetQueuedOrdersByStationType(StationType stationType)
            => _queuedOrders
            .Where(kv => kv.Key.StationType == stationType)
            .SelectMany(kv => kv.Value)
            .ToArray();

        private void OnOrderQueued(FuelStation station, ClientOrder order)
        {
            if (!_queuedOrders.ContainsKey(station))
                _queuedOrders.Add(station, new());
            _queuedOrders[station].Add(order);
        }
    }
}
