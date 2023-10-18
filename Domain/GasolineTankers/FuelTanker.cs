using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations
{
    public class FuelTanker : ISimulationEntity
    {
        private GasStation _destinationStation;

        public const int ReturnToBaseTime = 90;
        public const int RefillTime = 40;
        public int? ArrivalTime { get; private set; }
        public int DrivesCount { get; private set; }
        public readonly HashSet<OrderedFuel> LoadedFuel = new(); //Count = 2 or 3
        public readonly int TanksCount;
        public int EmptyTanksCount => TanksCount - LoadedFuel.Count;
        public event Action<GasStation> Arrived;
        public event Action<GasStation> Unloaded;
        public event Action<FuelTanker> ReturnedToBase;

        private int? ticksUntilArrival;
        private int? ticksUntilUnload;
        private int? ticksUntilReturnToBase;

        public int TankCapacity { get; }
        public bool IsBusy =>
            ticksUntilArrival != null && ticksUntilArrival > 0
            || ticksUntilUnload != null && ticksUntilUnload > 0
            || ticksUntilReturnToBase != null && ticksUntilReturnToBase > 0;

        public FuelTanker(int tanksCount, int tankCapacity)
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
            if (!IsBusy && EmptyTanksCount > 0)
            {
                LoadedFuel.Add(orderedFuel);
                return true;
            }
            return false;
        }

        private void DriveToStation(GasStation station)
        {
            if (IsBusy) throw new InvalidOperationException();
            _destinationStation = station;
            ticksUntilArrival = ArrivalTime = Simulation.Randomizer.Next(60, 121);
            Arrived += OnArrivedToStation;
        }

        private void OnArrivedToStation(GasStation station)
        {
            if (IsBusy) throw new InvalidOperationException();
            ticksUntilUnload = RefillTime;
            Unloaded += OnRefillFinished;
            Arrived -= OnArrivedToStation;
        }

        private void OnRefillFinished(GasStation refilledStation)
        {
            if (IsBusy) throw new InvalidOperationException();
            if (ticksUntilUnload == null) throw new InvalidOperationException();
            var owner = _destinationStation;
            var fuelForThisStation = LoadedFuel.Where(o => o.OwnerStation == owner).ToArray();
            if (fuelForThisStation.Length == 0)
                throw new InvalidProgramException();

            Unloaded -= OnRefillFinished;
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
                ticksUntilReturnToBase = ReturnToBaseTime;
                DrivesCount++;
            }
        }

        public void OnSimulationTickPassed()
        {
            //TODO: Simulate behaviour in refactored version
            //Changes behaviour if events fire only after ALL ticks decremented
            //Ticks from different jobs can spend it one tick pass!! That is incorrect behaviour.
            ticksUntilArrival--;
            if (ticksUntilArrival == 0)
            {
                Arrived?.Invoke(_destinationStation);
                ticksUntilArrival = null;
            }
            ticksUntilUnload--;
            if (ticksUntilUnload == 0)
            {
                Unloaded?.Invoke(_destinationStation);
                ticksUntilUnload = null;
            }
            ticksUntilReturnToBase--;
            if (ticksUntilReturnToBase == 0)
            {
                ReturnedToBase?.Invoke(this);
                ticksUntilReturnToBase = null;
            }
        }
    }
}
