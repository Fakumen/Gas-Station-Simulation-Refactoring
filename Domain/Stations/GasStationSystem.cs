using GasStations.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations
{
    public class GasStationSystem
    {
        private readonly List<OrderedFuel> _totalOrdersInCurrentTick = new();
        private readonly List<GasStation> _stations = new();
        private readonly List<GasolineTanker> _gasolineTankers = new();
        private readonly Dictionary<FuelType, float> _fuelPrices = new()
        {
            { FuelType.Petrol92, 45.6f },
            { FuelType.Petrol95, 48.2f },
            { FuelType.Petrol98, 50.3f },
            { FuelType.Diesel, 51.5f }
        };

        public GasStationSystem(int stationaryStationsCount, int miniStationsCount)
        {
            for (var i = 0; i < stationaryStationsCount; i++)
            {
                var avFuel = new Dictionary<FuelType, int>
                {
                    { FuelType.Petrol92, 30000 },
                    { FuelType.Petrol95, 16000 },
                    { FuelType.Petrol98, 16000 },
                    { FuelType.Diesel, 30000 }
                };
                var station = new GasStation(StationType.Stationary, _fuelPrices, avFuel);
                _stations.Add(station);
                station.CriticalFuelLevelReached += OnCriticalFuelLevelReached;
                station.ScheduleRefillIntervalPassed += OnScheduleRefillIntervalPassed;
            }

            for (var i = 0; i < miniStationsCount; i++)
            {
                var avFuel = new Dictionary<FuelType, int>
                {
                    { FuelType.Petrol92, 16000 },
                    { FuelType.Petrol95, 15000 }
                };
                var station = new GasStation(StationType.Mini, _fuelPrices, avFuel);
                _stations.Add(station);
                station.CriticalFuelLevelReached += OnCriticalFuelLevelReached;
                station.ScheduleRefillIntervalPassed += OnScheduleRefillIntervalPassed;
            }
        }

        public IReadOnlyList<GasStation> GasStations => _stations;

        public IReadOnlyList<GasolineTanker> GasolineTankers => _gasolineTankers;
        public IEnumerable<GasolineTanker> FreeGasolineTankers => GasolineTankers.Where(t => !t.IsBusy && t.EmptyTanksCount > 0);

        //TODO: Move to Simulation parameters
        #region Simulation Parameters
        public readonly static Random Random = new(0);
        public long PassedSimulationTicks { get; private set; }
        public event Action DayPassed;
        #endregion

        private void OnCriticalFuelLevelReached(GasStation station, FuelType criticalLevelFuel)
        {
            if (station.IsRequireGasolineTanker)
            {
                foreach (var fuel in station.GetFuelToRefillList())
                {
                    _totalOrdersInCurrentTick.Add(new OrderedFuel(station, fuel));
                }
                station.ConfirmGasolineOrder();
            }
        }

        private void OnScheduleRefillIntervalPassed(GasStation station)
        {
            
            if (station.IsRequireGasolineTanker)
            {
                foreach (var fuel in station.GetFuelToRefillList())
                {
                    _totalOrdersInCurrentTick.Add(new OrderedFuel(station, fuel));
                }
                station.ConfirmGasolineOrder();
            }
        }

        public void OrderGasolineTankers(List<OrderedFuel> orderedFuel)
        {
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
                    _gasolineTankers.Add(new GasolineTanker(tanksCount));
                }
                FreeGasolineTankers.First().OrderFuel(fuel.OwnerStation, fuel.FuelType, out var success);
                //
                //if (fuel.OwnerStation.StationType == StationType.Mini)
                //{
                //    var requiredTanker = FreeGasolineTankers.Where(t => t.TanksCount < 3).FirstOrDefault();
                //    if (requiredTanker == null)
                //        GasolineTankers.Add(new GasolineTanker(2));
                //    leftOrderedFuel -= 2;
                //}
                //else if (fuel.OwnerStation.StationType == StationType.Stationary)
                //{
                //    var tanksCount = leftOrderedFuel;
                //    tanksCount = Math.Min(tanksCount, 3);
                //    if (tanksCount < 2)
                //        tanksCount = 2;
                //    var requiredTanker = FreeGasolineTankers.Where(t => t.TanksCount == tanksCount).FirstOrDefault();
                //    if (requiredTanker == null)
                //        GasolineTankers.Add(new GasolineTanker(2));
                //}
            }
        }

        private void HandleOneTick()
        {
            foreach (var station in GasStations)
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
                }
                station.WaitOneTick();
            }
            OrderGasolineTankers(_totalOrdersInCurrentTick);
            _totalOrdersInCurrentTick.Clear();
            foreach (var gasTanker in GasolineTankers)
            {
                gasTanker.WaitOneTick();
                if (!gasTanker.IsBusy && gasTanker.LoadedFuel.Count > 0)
                    gasTanker.StartDelivery();
            }
            PassedSimulationTicks++;

            static FuelType TruckFuelSelector(
                Random randomizer, IReadOnlyDictionary<FuelType, FuelContainer> availableFuel)
            {
                var fuelForTrucks = availableFuel.Keys
                    .Where(f => f == FuelType.Petrol92 || f == FuelType.Diesel)
                    .ToArray();
                if (fuelForTrucks.Any(f => f == FuelType.Diesel))
                {
                    return fuelForTrucks
                        .TakeRandom(randomizer);
                }
                return fuelForTrucks.Single(f => f == FuelType.Petrol92);
            }
        }

        public void RunSimulation(long simulationTimeInTicks)
        {
            for (var i = 1; i <= simulationTimeInTicks; i++)//Why it skips 1st tick?
            {
                HandleOneTick();
                if ((i) % (24 * 60) == 0 && i != 0)//Отчет между сутками
                {
                    DayPassed?.Invoke();
                }
            }
        }
    }
}
