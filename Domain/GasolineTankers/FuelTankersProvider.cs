using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations
{
    public class FuelTankersProvider : ISimulationEntity
    {
        private readonly List<OrderedFuel> _totalOrdersInCurrentTick = new();
        private readonly List<FuelTanker> _gasolineTankers = new();

        public int TankerVolumeCapacity { get; } = 6000;
        public IReadOnlyList<FuelTanker> GasolineTankers => _gasolineTankers;
        public IEnumerable<FuelTanker> FreeGasolineTankers => GasolineTankers.Where(t => !t.IsBusy && t.EmptyTanksCount > 0);

        public void OrderFuelSection(GasStation orderedStation, FuelType fuelType)
        {
            _totalOrdersInCurrentTick.Add(new OrderedFuel(orderedStation, fuelType));
        }

        public void OnSimulationTickPassed()
        {
            OrderGasolineTankers();
            HandleTickByGasolineTankers();
        }

        private void OrderGasolineTankers()
        {
            var orderedFuel = _totalOrdersInCurrentTick.ToList();

            var leftOrderedFuel = orderedFuel.Count;
            foreach (var fuel in orderedFuel)
            {
                var freeGasTanker = FreeGasolineTankers
                    .Where(t => fuel.OwnerStation.StationType == StationType.Stationary || t.TanksCount < 3) //Мини АЗС не обслуживают 3х+ секционные бензовозы
                    .FirstOrDefault();
                if (freeGasTanker == null) //Нет подходящих бензовозов
                {
                    var tanksCount = leftOrderedFuel;
                    if (fuel.OwnerStation.StationType == StationType.Stationary)
                    {
                        tanksCount = Math.Min(tanksCount, 3);
                        if (tanksCount < 2)
                            tanksCount = 2;
                    }
                    else if (fuel.OwnerStation.StationType == StationType.Mini)
                        tanksCount = 2;
                    leftOrderedFuel -= tanksCount;
                    _gasolineTankers.Add(new FuelTanker(tanksCount, TankerVolumeCapacity));
                }
                FreeGasolineTankers.First().OrderFuel(fuel.OwnerStation, fuel.FuelType);
            }

            _totalOrdersInCurrentTick.Clear();
        }

        private void HandleTickByGasolineTankers()
        {
            foreach (var gasTanker in GasolineTankers)
            {
                gasTanker.OnSimulationTickPassed();
                if (!gasTanker.IsBusy && gasTanker.LoadedFuel.Count > 0)
                    gasTanker.StartDelivery();
            }
        }
    }
}
