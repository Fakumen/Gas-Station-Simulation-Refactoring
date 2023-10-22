namespace GasStations
{
    public interface IReadOnlyFuelContainer
    {
        public int MaximumCapacity { get; }
        public int FilledVolume { get; }
        public int ReservedVolume { get; }
        public int EmptyVolume { get; }
        public int EmptyUnreservedVolume { get; }

        public int VolumeConsumption { get; }
        public int VolumeIncome { get; }
    }
}
