using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasStations
{
    public abstract class ClientOrder : IWaiter
    {
        public event Action OrderAppeared;
        public readonly int OrderAppearTime;
        private int ticksUntilOrderAppear;
        public int TicksUntilOrderAppear
        {
            get => ticksUntilOrderAppear;
            set
            {
                ticksUntilOrderAppear = value;
                if (value == 0)
                {
                    OrderAppeared?.Invoke();
                }
            }
        }

        public ClientOrder()
        {
            OrderAppearTime = GetInterval();
            TicksUntilOrderAppear = OrderAppearTime;
        }

        public abstract int GetInterval();
        public abstract FuelType GetRequestedFuel(Dictionary<FuelType, FuelContainer> availableFuel);
        public abstract int GetRequestedVolume(int maximumAvailableVolume);

        public void WaitOneTick()
        {
            TicksUntilOrderAppear--;
        }
    }

    public class CarClientOrder : ClientOrder
    {
        private readonly int interval = GasStationSystem.Random.Next(1, 6);
        private FuelType fuelType;
        private readonly int requestedVolume = GasStationSystem.Random.Next(10, 51);

        public override int GetInterval() => interval;

        public override FuelType GetRequestedFuel(Dictionary<FuelType, FuelContainer> availableFuel)
        {
            if (fuelType != FuelType.None)
                return fuelType;
            var fuelCollection = availableFuel.Keys;
            var count = fuelCollection.Count;
            var selectedIndex = GasStationSystem.Random.Next(count);
            var selectedFuel = fuelCollection.Skip(selectedIndex).First();
            fuelType = selectedFuel;
            return fuelType;
        }

        public override int GetRequestedVolume(int maximumAvailableVolume)
        {
            return Math.Min(requestedVolume, maximumAvailableVolume);
        }
    }

    public class TruckClientOrder : ClientOrder
    {
        private readonly int interval = GasStationSystem.Random.Next(1, 13);
        private FuelType fuelType;
        private readonly int requestedVolume = GasStationSystem.Random.Next(30, 301);

        public override int GetInterval() => interval;

        public override FuelType GetRequestedFuel(Dictionary<FuelType, FuelContainer> availableFuel)
        {
            if (fuelType != FuelType.None)
                return fuelType;
            var fuelCollection = availableFuel.Keys;
            if (fuelCollection.Any(f => f == FuelType.Diesel))
            {
                var randomVal = GasStationSystem.Random.Next(2);
                if (randomVal == 0)
                    fuelType = fuelCollection.Single(f => f == FuelType.Petrol92);
                else
                    fuelType = fuelCollection.Single(f => f == FuelType.Diesel);
                return fuelType;
            }
            fuelType = fuelCollection.Single(f => f == FuelType.Petrol92);
            return fuelType;
        }

        public override int GetRequestedVolume(int maximumAvailableVolume)
        {
            return Math.Min(requestedVolume, maximumAvailableVolume);
        }
    }
}
