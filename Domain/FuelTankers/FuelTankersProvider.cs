using GasStations.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations
{
    public class FuelTankersProvider : ISimulationEntity
    {
        private readonly List<OrderedFuelSection> _totalOrdersInCurrentTick = new();
        private readonly List<FuelTanker> _fuelTankers = new();

        public int TankerVolumeCapacity { get; } = 6000;
        public IReadOnlyList<FuelTanker> FuelTankers => _fuelTankers;
        public IEnumerable<FuelTanker> FreeFuelTankers 
            => FuelTankers.Where(t => !t.IsBusy && t.EmptyTanksCount > 0);

        public void OrderFuelSection(FuelStation orderedStation, FuelType fuelType)
        {
            _totalOrdersInCurrentTick.Add(new OrderedFuelSection(orderedStation, fuelType));
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
                    t => t.TanksCount < 3 || fuel.OrderedStation.StationType != StationType.Mini);
                if (appropriateTankerForStation == null)
                {
                    var tanksCount = fuel.OrderedStation.StationType switch
                    {
                        StationType.Stationary => MathExtensions.Clamp(leftOrderedFuel, 2, 3),
                        StationType.Mini => 2,
                        _ => throw new NotImplementedException(),
                    };
                    leftOrderedFuel -= tanksCount;
                    appropriateTankerForStation = new FuelTanker(tanksCount, TankerVolumeCapacity);
                    _fuelTankers.Add(appropriateTankerForStation);
                }
                //TODO: Simulates old (incorrect) behaviour
                //First free tanker is not last added and may not be allowed for the station
                FreeFuelTankers.First().LoadOrderedFuel(fuel);
                //TODO: Replace with:
                //appropriateTankerForStation.LoadOrderedFuel(fuel);
            }

            _totalOrdersInCurrentTick.Clear();
        }

        private void HandleTickByFuelTankers()
        {
            foreach (var tanker in FuelTankers)
            {
                tanker.OnSimulationTickPassed();
                if (!tanker.IsBusy && tanker.LoadedTanksCount > 0)
                    tanker.StartDelivery();
            }
        }
    }
}
