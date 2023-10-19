using GasStations.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations
{
    public class FuelTankersProvider : ISimulationEntity
    {
        private readonly List<OrderedFuel> _totalOrdersInCurrentTick = new();
        private readonly List<FuelTanker> _fuelTankers = new();

        public int TankerVolumeCapacity { get; } = 6000;
        public IReadOnlyList<FuelTanker> FuelTankers => _fuelTankers;
        public IEnumerable<FuelTanker> FreeFuelTankers 
            => FuelTankers.Where(t => !t.IsBusy && t.EmptyTanksCount > 0);

        public void OrderFuelSection(GasStation orderedStation, FuelType fuelType)
        {
            _totalOrdersInCurrentTick.Add(new OrderedFuel(orderedStation, fuelType));
        }

        public void OnSimulationTickPassed()
        {
            DistributeFuelToTankers();
            HandleTickByFuelTankers();
        }

        private void DistributeFuelToTankers()
        {
            var leftOrderedFuel = _totalOrdersInCurrentTick.Count;
            foreach (var fuel in _totalOrdersInCurrentTick)
            {
                //Мини АЗС не обслуживают 3х+ секционные бензовозы
                var appropriateTankerForStation = FreeFuelTankers
                    .FirstOrDefault(
                    t => t.TanksCount < 3 || fuel.OwnerStation.StationType != StationType.Mini);
                if (appropriateTankerForStation == null)
                {
                    var tanksCount = fuel.OwnerStation.StationType switch
                    {
                        StationType.Stationary => MathExtensions.Clamp(leftOrderedFuel, 2, 3),
                        StationType.Mini => 2,
                        _ => throw new NotImplementedException(),
                    };
                    leftOrderedFuel -= tanksCount;
                    appropriateTankerForStation = new FuelTanker(tanksCount, TankerVolumeCapacity);
                    _fuelTankers.Add(appropriateTankerForStation);
                }
                appropriateTankerForStation.LoadOrderedFuel(fuel);
            }

            _totalOrdersInCurrentTick.Clear();
        }

        private void HandleTickByFuelTankers()
        {
            foreach (var gasTanker in FuelTankers)
            {
                gasTanker.OnSimulationTickPassed();
                if (!gasTanker.IsBusy && gasTanker.LoadedTanksCount > 0)
                    gasTanker.StartDelivery();
            }
        }
    }
}
