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
        public abstract Fuel GetRequestedFuel(Dictionary<Fuel, FuelContainer> availableFuel);
        public abstract int GetRequestedVolume(int maximumAvailableVolume);

        public void WaitOneTick()
        {
            TicksUntilOrderAppear--;
        }
    }

    public class CarClientOrder : ClientOrder
    {
        private readonly int interval = GasStationSystem.Random.Next(1, 6);
        private Fuel fuelType;
        private readonly int requestedVolume = GasStationSystem.Random.Next(10, 51);

        public override int GetInterval() => interval;

        public override Fuel GetRequestedFuel(Dictionary<Fuel, FuelContainer> availableFuel)
        {
            if (fuelType != null)
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
        private Fuel fuelType;
        private readonly int requestedVolume = GasStationSystem.Random.Next(30, 301);

        public override int GetInterval() => interval;

        public override Fuel GetRequestedFuel(Dictionary<Fuel, FuelContainer> availableFuel)
        {
            if (fuelType != null)
                return fuelType;
            var fuelCollection = availableFuel.Keys;
            if (fuelCollection.Any(f => f.FuelTypeName == "Дт"))
            {
                var randomVal = GasStationSystem.Random.Next(2);
                if (randomVal == 0)
                    fuelType = fuelCollection.Single(f => f.FuelTypeName == "92");
                else
                    fuelType = fuelCollection.Single(f => f.FuelTypeName == "Дт");
                return fuelType;
            }
            fuelType = fuelCollection.Single(f => f.FuelTypeName == "92");
            return fuelType;
        }

        public override int GetRequestedVolume(int maximumAvailableVolume)
        {
            return Math.Min(requestedVolume, maximumAvailableVolume);
        }
    }
}
