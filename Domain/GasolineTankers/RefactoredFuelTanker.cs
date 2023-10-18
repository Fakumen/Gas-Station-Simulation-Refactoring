using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations
{
    public class RefactoredFuelTanker : ISimulationEntity
    {
        private readonly Queue<IFuelTankerJob> _jobs = new();
        private GasStation _destinationStation;

        #region Old
        public const int ReturnToBaseTime = 90;
        public const int RefillTime = 40;
        public int? ArrivalTime { get; private set; }
        public int DrivesCount { get; private set; }
        public readonly HashSet<OrderedFuel> LoadedFuel = new(); //Count = 2 or 3
        public readonly int TanksCount;
        public int EmptyTanksCount => TanksCount - LoadedFuel.Count;

        public int TankCapacity { get; }
        public bool IsBusy => _jobs.Count > 0;
        #endregion

        public RefactoredFuelTanker(int tanksCount, int tankCapacity)
        {
            if (tanksCount != 2 && tanksCount != 3) throw new ArgumentException();
            TanksCount = tanksCount;
            TankCapacity = tankCapacity;
        }

        public void StartDelivery()
        {
            if (IsBusy) throw new InvalidOperationException();
            DriveToStation(LoadedFuel.First().OwnerStation);
        }

        public bool LoadOrderedFuel(OrderedFuel orderedFuel)
        {
            if (IsBusy) throw new InvalidProgramException();
            if (!IsBusy && EmptyTanksCount > 0)
            {
                LoadedFuel.Add(orderedFuel);
                return true;
            }
            return false;
        }

        private void DriveToStation(GasStation station)
        {
            _destinationStation = station;
            var driveToStationJob = new UniversalFuelTankerJob(Simulation.Randomizer.Next(60, 121));
            driveToStationJob.JobFinished += OnArrivedToStation;
            _jobs.Enqueue(driveToStationJob);
        }

        private void OnArrivedToStation(IFuelTankerJob driveToStationJob)
        {
            if (driveToStationJob != _jobs.Peek())
                throw new InvalidProgramException("Passed job is not current queued job");

            driveToStationJob.JobFinished -= OnArrivedToStation;
            var refillJob = new UniversalFuelTankerJob(RefillTime);
            refillJob.JobFinished += OnRefillFinished;
            _jobs.Enqueue(refillJob);
            _jobs.Dequeue();
        }

        private void OnRefillFinished(IFuelTankerJob refillJob)
        {
            if (refillJob != _jobs.Peek())
                throw new InvalidProgramException("Passed job is not current queued job");
            var owner = _destinationStation;
            var fuelForThisStation = LoadedFuel.Where(o => o.OwnerStation == owner).ToArray();
            if (fuelForThisStation.Length == 0)
                throw new InvalidProgramException();

            refillJob.JobFinished -= OnRefillFinished;
            foreach (var fuel in fuelForThisStation)
            {
                owner.Refill(fuel.FuelType, TankCapacity);
                LoadedFuel.Remove(fuel);
            }
            _destinationStation = null;

            if (LoadedFuel.Count > 0)
                DriveToStation(LoadedFuel.First().OwnerStation);
            else
            {
                var returnToBaseJob = new UniversalFuelTankerJob(ReturnToBaseTime);
                returnToBaseJob.JobFinished += j => _jobs.Dequeue();
                _jobs.Enqueue(returnToBaseJob);
                DrivesCount++;
            }
            _jobs.Dequeue();
        }

        public void OnSimulationTickPassed()
        {
            //Changes behaviour
            //TODO: Simulate initial behaviour
            if (_jobs.Count > 0)
                _jobs.Peek().OnSimulationTickPassed();
        }
    }
}
