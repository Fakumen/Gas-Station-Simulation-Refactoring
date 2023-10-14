using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations
{
    public class Simulation
    {
        public readonly static Random Randomizer = new(0);

        private readonly GasStationSystem _stationsNetwork;
        private readonly OrderProvider _orderProvider;

        #region FuelTrucksProvider
        private readonly List<OrderedFuel> _truckFuelOrdersInCurrentTick = new();
        private readonly List<GasolineTanker> _gasolineTankers = new();

        public IReadOnlyList<GasolineTanker> GasolineTankers => _gasolineTankers;
        public IEnumerable<GasolineTanker> FreeGasolineTankers => GasolineTankers.Where(t => !t.IsBusy && t.EmptyTanksCount > 0);
        #endregion

        public Simulation(
            GasStationSystem stationsNetwork,
            //FuelTankersProvider tankersProvider, //TODO: Move to Simulation
            OrderProvider orderProvider)
        {
            _stationsNetwork = stationsNetwork;
            _orderProvider = orderProvider;
            _stationsNetwork.RefillRequestedByStation += OnRefillRequested;
        }

        public long PassedSimulationTicks { get; private set; }

        public event Action DayPassed;

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

        private void HandleOneTick()
        {
            foreach (var station in _stationsNetwork.GasStations)
            {
                _orderProvider.ProvideOrdersToStation(station);
                station.WaitOneTick();
            }
            //Fuel Tankers
            _truckFuelOrdersInCurrentTick
                .Select(f => f.OwnerStation)
                .Distinct()
                .ToList()
                .ForEach(s => s.ConfirmGasolineOrder());//Statistics
            OrderGasolineTankers(_truckFuelOrdersInCurrentTick);
            _truckFuelOrdersInCurrentTick.Clear();
            foreach (var gasTanker in GasolineTankers)
            {
                gasTanker.WaitOneTick();
                if (!gasTanker.IsBusy && gasTanker.LoadedFuel.Count > 0)
                    gasTanker.StartDelivery();
            }
            //
            PassedSimulationTicks++;
        }

        private void OrderGasolineTankers(List<OrderedFuel> orderedFuel)
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
            }
        }

        private void OnRefillRequested(GasStation station, IReadOnlyDictionary<FuelType, int> fuelVolumes)
        {
            foreach (var fuel in fuelVolumes.Keys)
            {
                _truckFuelOrdersInCurrentTick.Add(new OrderedFuel(station, fuel));
            }
        }
    }
}
