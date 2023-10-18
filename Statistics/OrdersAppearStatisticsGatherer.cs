using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations
{
    //TODO: Return cashed statistics on request if nothing has changed since.
    public class OrdersAppearStatisticsGatherer
    {
        private readonly Dictionary<GasStation, List<ClientOrder>> _queuedOrders = new();

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
        public IReadOnlyList<ClientOrder> GetQueuedOrdersByStation(GasStation station)
            => _queuedOrders.ContainsKey(station)
            ? _queuedOrders[station]
            : new();

        [Obsolete("Counts QUEUED orders, not appeared! Used to simulate old behaviour.")]
        public IEnumerable<ClientOrder> GetQueuedOrdersByClientType(ClientType clientType)
            => _queuedOrders.Values.SelectMany(o => o).Where(o => o.ClientType == clientType);

        private void OnOrderQueued(GasStation station, ClientOrder order)
        {
            if (!_queuedOrders.ContainsKey(station))
                _queuedOrders.Add(station, new());
            _queuedOrders[station].Add(order);
        }
    }
}
