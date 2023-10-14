using GasStations.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations
{
    public class OrderProvider
    {
        public void ProvideOrdersToStation(GasStation station)
        {
            foreach (var clientType in EnumExtensions.GetValues<ClientType>())
            {
                if (station.IsExpectingOrder(clientType))
                    continue;
                var newOrder = new ClientOrder(
                    clientType,
                    clientType switch
                    {
                        ClientType.Car => r => r.Next(1, 6),
                        ClientType.Truck => r => r.Next(1, 13),
                        _ => throw new NotImplementedException(),
                    },
                    clientType switch
                    {
                        ClientType.Car => r => r.Next(10, 51),
                        ClientType.Truck => r => r.Next(30, 301),
                        _ => throw new NotImplementedException(),
                    },
                    clientType switch
                    {
                        ClientType.Car => (r, f) => f.Keys.TakeRandom(r),
                        ClientType.Truck => TruckFuelSelector,
                        _ => throw new NotImplementedException(),
                    });
                station.AddOrderInQueue(newOrder);

                static FuelType TruckFuelSelector(
                    Random randomizer, IReadOnlyDictionary<FuelType, FuelContainer> availableFuel)
                {
                    var fuelForTrucks = availableFuel.Keys
                        .Where(f => f == FuelType.Petrol92 || f == FuelType.Diesel)//fuel for trucks
                        .ToArray();
                    if (fuelForTrucks.Length > 1)
                    {
                        return fuelForTrucks
                            .TakeRandom(randomizer);
                    }
                    return fuelForTrucks.Single(f => f == FuelType.Petrol92);
                }
            }
        }
    }
}
