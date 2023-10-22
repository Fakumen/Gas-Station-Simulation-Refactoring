namespace GasStations
{
    public class OrderedFuelSection
    {
        public readonly FuelStation OrderedStation;
        public readonly FuelType FuelType;

        public OrderedFuelSection(FuelStation orderedStation, FuelType fuelType)
        {
            OrderedStation = orderedStation;
            FuelType = fuelType;
        }
    }
}
