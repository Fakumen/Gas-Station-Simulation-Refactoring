namespace GasStations
{
    public class ServedOrder
    {
        public ServedOrder(ClientOrder clientOrder, int providedVolume, float fuelPrice)
        {
            ClientOrder = clientOrder;
            ProvidedFuelVolume = providedVolume;
            OrderTimeFuelPrice = fuelPrice;
        }

        public ClientOrder ClientOrder { get; }
        public int ProvidedFuelVolume { get; }
        public float OrderTimeFuelPrice { get; }
        public float OrderRevenue => ProvidedFuelVolume * OrderTimeFuelPrice;
        public bool IsSuccessful => ProvidedFuelVolume > 0;
    }
}
