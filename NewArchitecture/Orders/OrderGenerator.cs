using GasStations.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace GasStations
{
    public class OrderGenerator
    {
        private readonly PetrolNetworkSimulation _simulation;
        private readonly Dictionary<ClientType, ClientParametersGenerator> _parametersGenerators = new();
        private readonly Dictionary<IPetrolStation, Queue<ClientOrderInfo>> _stationOrders = new();
        private readonly Dictionary<ClientOrderInfo, int> _ordersTimeToAppear = new();

        public OrderGenerator(PetrolNetworkSimulation simulation)
        {
            _simulation = simulation;
            foreach (var clientType in EnumExtensions.GetValues<ClientType>())
            {
                var generator = clientType switch
                {
                    ClientType.Car => new ClientParametersGenerator(
                        r => r.Next(1, 6), 
                        (r, s) => EnumExtensions.FuelTypes
                        .Where(f => s.HasFuelTypeInService(f))
                        .TakeRandom(r),
                        (r, s) => r.Next(10, 51)),
                    ClientType.Truck => new ClientParametersGenerator(
                        r => r.Next(1, 13),
                        (r, s) => EnumExtensions.FuelTypes
                        .Where(f => s.HasFuelTypeInService(f))
                        .Where(f => f == FuelType.Petrol92 || f == FuelType.Diesel)
                        .TakeRandom(r),
                        (r, s) => r.Next(30, 301)),
                    _ => throw new NotImplementedException(),
                };
                _parametersGenerators.Add(clientType, generator);
            }
        }

        private ClientOrderInfo GenerateOrderForStation(IPetrolStation station, ClientType clientType)
        {
            var randomizer = _simulation.Parameters.Randomizer;
            var generator = _parametersGenerators[clientType];
            //var appearInterval = generator.GenerateAppearInterval(randomizer);
            var requestedVolume = generator.GenerateFuelVolume(randomizer, station);
            var fuelType = generator.GenerateFuelType(randomizer, station);
            return new ClientOrderInfo(clientType, fuelType, requestedVolume);
        }

        public void DispenseOrders(IEnumerable<IPetrolStation> stations)
        {
            var randomizer = _simulation.Parameters.Randomizer;
            foreach (var station in stations)
            {
                if (!_stationOrders.ContainsKey(station))
                    _stationOrders.Add(station, new());
                var grouppedOrders = _stationOrders[station]
                    .GroupBy(o => o.ClientType)
                    .ToDictionary(g => g.Key, g => g.ToArray());
                foreach (var clientType in EnumExtensions.ClientTypes)
                {
                    if (!grouppedOrders.ContainsKey(clientType))//no clients of such type
                    {
                        var timeToAppear = _parametersGenerators[clientType]
                            .GenerateAppearInterval(randomizer);
                        var newOrder = GenerateOrderForStation(station, clientType);
                        _ordersTimeToAppear.Add(newOrder, timeToAppear);
                        _stationOrders[station].Enqueue(newOrder);
                    }
                }
                while (_stationOrders[station].Count > 0 
                    && _ordersTimeToAppear[_stationOrders[station].Peek()] <= 0)
                {
                    station.ServeOrder(_stationOrders[station].Dequeue());
                }
            }
        }
    }
}
