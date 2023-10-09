using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasStations
{
    public class GasolineTanker : IWaiter
    {
        private int newRandomArrivalTime => GasStationSystem.Random.Next(60, 121);
        public const int TankCapacity = 6000;
        public const int ReturnToBaseTime = 90;
        public const int UnloadTime = 40;
        public int? ArrivalTime { get; private set; }
        public int DrivesCount { get; private set; }
        public GasStation CurrentStation { get; private set; }
        public readonly HashSet<OrderedFuel> LoadedFuel = new HashSet<OrderedFuel>(); //Count = 2 or 3
        public readonly int TanksCount;
        public int EmptyTanksCount => TanksCount - LoadedFuel.Count;
        public event Action<GasStation> Arrived;
        public event Action<GasStation> Unloaded;
        public event Action<GasolineTanker> ReturnedToBase;

        private int? ticksUntilArrival;
        private int? ticksUntilUnload;
        private int? ticksUntilReturnToBase;
        public int? TicksUntilArrival 
        { 
            get => ticksUntilArrival; 
            private set
            {
                ticksUntilArrival = value;
                if (value == 0)
                {
                    Arrived?.Invoke(CurrentStation);
                    ticksUntilArrival = null;
                }
            }
        }
        public int? TicksUntilUnload
        {
            get => ticksUntilUnload;
            private set
            {
                ticksUntilUnload = value;
                if (value == 0)
                {
                    Unloaded?.Invoke(CurrentStation);
                    ticksUntilUnload = null;
                }
            }
        }
        public int? TicksUntilReturnToBase
        {
            get => ticksUntilReturnToBase;
            private set
            {
                ticksUntilReturnToBase = value;
                if (value == 0)
                {
                    ReturnedToBase?.Invoke(this);
                    ticksUntilReturnToBase = null;
                }
            }
        }

        public bool IsBusy =>
            TicksUntilArrival != null && TicksUntilArrival > 0
            || TicksUntilUnload != null && TicksUntilUnload > 0
            || TicksUntilReturnToBase != null && TicksUntilReturnToBase > 0;

        public GasolineTanker(int tanksCount)
        {
            if (tanksCount != 2 && tanksCount != 3) throw new ArgumentException();
            TanksCount = tanksCount;
        }

        public void StartDelivery()
        {
            if (IsBusy) throw new InvalidOperationException();
            DriveToStation(LoadedFuel.First().OwnerStation);
        }

        public void OrderFuel(GasStation orderOwner, Fuel fuelType, out bool isSuccessful)
        {
            if (!IsBusy && EmptyTanksCount > 0)
            {
                LoadedFuel.Add(new OrderedFuel(orderOwner, fuelType));
                isSuccessful = true;
                return;
            }
            isSuccessful = false;
        }

        private void DriveToStation(GasStation station)
        {
            if (IsBusy) throw new InvalidOperationException();
            CurrentStation = station;
            TicksUntilArrival = ArrivalTime = newRandomArrivalTime;
            Arrived += OnGasolineTankerArrived;
        }

        private void OnGasolineTankerArrived(GasStation station)
        {
            if (IsBusy) throw new InvalidOperationException();
            TicksUntilUnload = UnloadTime;
            Unloaded += Unload;
            Arrived -= OnGasolineTankerArrived;
        }

        private void Unload(GasStation owner)
        {
            if (IsBusy) throw new InvalidOperationException();
            if (TicksUntilUnload == null) throw new InvalidOperationException();
            if (!LoadedFuel.Any(o => o.OwnerStation == owner)) throw new ArgumentException();
            var fuelForThisStation = LoadedFuel.Where(o => o.OwnerStation == owner).ToArray();
            foreach (var fuel in fuelForThisStation)
            {
                owner.Refill(fuel.FuelType, TankCapacity);
            }
            foreach (var usedFuel in fuelForThisStation)
                LoadedFuel.Remove(usedFuel);
            Unloaded -= Unload;
            CurrentStation = null;
            if (LoadedFuel.Count > 0)//Есть, что еще развозить
                DriveToStation(LoadedFuel.First().OwnerStation);
            else//Нечего больше развозить
            {
                //Бензовоз возвращается домой
                TicksUntilReturnToBase = ReturnToBaseTime;
                DrivesCount++;
            }
        }

        public void WaitOneTick()
        {
            TicksUntilArrival--;
            TicksUntilUnload--;
            TicksUntilReturnToBase--;
        }
    }
}
