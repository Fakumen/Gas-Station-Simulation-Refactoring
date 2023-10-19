using System;

namespace GasStations
{
    public class FuelContainer : IReadOnlyFuelContainer
    {
        public FuelContainer(int maximumCapacity)
        {
            MaximumCapacity = maximumCapacity;
            FilledVolume = maximumCapacity;
        }

        public int MaximumCapacity { get; }
        public int FilledVolume { get; private set; }
        public int ReservedVolume { get; private set; }
        public int EmptyVolume => MaximumCapacity - FilledVolume;
        public int EmptyUnreservedVolume => EmptyVolume - ReservedVolume;

        public int VolumeConsumption { get; private set; }
        public int VolumeIncome { get; private set; }

        public void Consume(int volume)
        {
            if (volume > FilledVolume) throw new ArgumentException();
            FilledVolume -= volume;
            VolumeConsumption += volume;
        }

        public void Fill(int volume)
        {
            if (volume > EmptyVolume) throw new ArgumentException();
            ReservedVolume -= volume;
            FilledVolume += volume;
            VolumeIncome += volume;
        }

        public void ReserveVolume(int volume)
        {
            if (volume + ReservedVolume > EmptyVolume) throw new ArgumentException();
            ReservedVolume += volume;
        }
    }
}
