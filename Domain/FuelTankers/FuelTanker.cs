using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations
{
    public class FuelTanker : ISimulationEntity
    {
        private readonly Queue<ISimulationJob> _jobs = new();
        private readonly HashSet<OrderedFuelSection> _loadedFuel = new();
        private FuelStation _destinationStation;

        public const int ReturnToBaseTime = 90 - 1; //"-1" to simulate old (incorrect) behaviour
        public const int RefillTime = 40 - 1; //"-1" to simulate old (incorrect) behaviour

        public FuelTanker(int tanksCount, int tankCapacity)
        {
            TanksCount = tanksCount;
            TankCapacity = tankCapacity;
        }

        public int TankCapacity { get; }
        public int TanksCount { get; }
        public int LoadedTanksCount => _loadedFuel.Count;
        public int EmptyTanksCount => TanksCount - LoadedTanksCount;
        public bool IsBusy => _jobs.Count > 0;
        public int DrivesCount { get; private set; }//Statistics

        public event Action<FuelTanker> DeliveryFinished;

        public void StartDelivery()
        {
            if (IsBusy || LoadedTanksCount == 0) 
                throw new InvalidOperationException();
            DriveToStation(_loadedFuel.First().OrderedStation);
        }

        public void LoadOrderedFuel(OrderedFuelSection orderedFuel)
        {
            if (IsBusy || EmptyTanksCount == 0) 
                throw new InvalidProgramException();
            _loadedFuel.Add(orderedFuel);
        }

        private void DriveToStation(FuelStation station)
        {
            _destinationStation = station;
            var driveToStationJob = new SimulationJob(Simulation.Randomizer.Next(60, 121));
            driveToStationJob.JobFinished += OnArrivedToStation;
            _jobs.Enqueue(driveToStationJob);
        }

        private void OnArrivedToStation(ISimulationJob driveToStationJob)
        {
            if (driveToStationJob != _jobs.Peek())
                throw new InvalidProgramException("Passed job is not current queued job");

            driveToStationJob.JobFinished -= OnArrivedToStation;
            var refillJob = new SimulationJob(RefillTime);
            refillJob.JobFinished += OnRefillFinished;
            _jobs.Enqueue(refillJob);
            _jobs.Dequeue();
        }

        private void OnRefillFinished(ISimulationJob refillJob)
        {
            if (refillJob != _jobs.Peek())
                throw new InvalidProgramException("Passed job is not current queued job");
            var owner = _destinationStation;
            var fuelForThisStation = _loadedFuel.Where(o => o.OrderedStation == owner).ToArray();
            if (fuelForThisStation.Length == 0)
                throw new InvalidProgramException();

            refillJob.JobFinished -= OnRefillFinished;
            foreach (var fuel in fuelForThisStation)
            {
                owner.Refill(fuel.FuelType, TankCapacity);
                _loadedFuel.Remove(fuel);
            }
            _destinationStation = null;

            if (LoadedTanksCount > 0)
                DriveToStation(_loadedFuel.First().OrderedStation);
            else
            {
                var returnToBaseJob = new SimulationJob(ReturnToBaseTime);
                returnToBaseJob.JobFinished += j => _jobs.Dequeue();
                _jobs.Enqueue(returnToBaseJob);
                DrivesCount++;
            }
            _jobs.Dequeue();
        }

        public void OnSimulationTickPassed()
        {
            if (_jobs.Count > 0)
                _jobs.Peek().OnSimulationTickPassed();
        }
    }
}
