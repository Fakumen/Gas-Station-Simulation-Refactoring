﻿using System;

namespace GasStations
{
    public class Simulation
    {
        public readonly static Random Randomizer = new(0);

        private readonly FuelStationsNetwork _stationsNetwork;
        private readonly FuelTankersProvider _fuelTankersProvider;
        private readonly OrderProvider _orderProvider;

        public Simulation(
            FuelStationsNetwork stationsNetwork,
            FuelTankersProvider fuelTankersProvider,
            OrderProvider orderProvider)
        {
            _stationsNetwork = stationsNetwork;
            _fuelTankersProvider = fuelTankersProvider;
            _orderProvider = orderProvider;
        }

        public FuelStationsNetwork StationsNetwork => _stationsNetwork;
        public FuelTankersProvider FuelTankersProvider => _fuelTankersProvider;
        public OrderProvider OrderProvider => _orderProvider;
        public long PassedSimulationTicks { get; private set; }

        public event Action DayPassed;

        public void RunSimulation(long simulationTimeInTicks)
        {
            for (var i = 1; i <= simulationTimeInTicks; i++)//Why it skips 1st tick?
            {
                HandleOneSimulationTick();
                if ((i) % (24 * 60) == 0 && i != 0)//Отчет между сутками
                {
                    DayPassed?.Invoke();
                }
            }
        }

        private void HandleOneSimulationTick()
        {
            foreach (var station in _stationsNetwork.Stations)
            {
                _orderProvider.ProvideOrdersToStation(station);
                station.OnSimulationTickPassed();
                _orderProvider.OnSimulationTickPassedForStation(station);//Simulates old behaviour
            }
            //TODO: Replace with:
            //_orderProvider.OnSimulationTickPassed();
            _fuelTankersProvider.OnSimulationTickPassed();
            PassedSimulationTicks++;
        }
    }
}
