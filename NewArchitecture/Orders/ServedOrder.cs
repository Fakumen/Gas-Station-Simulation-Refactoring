namespace GasStations
{
    public class ServedOrder
    {
        public ServedOrder(ClientOrderInfo clientOrder, int providedFuel, float fuelPrice)
        {
            ClientOrder = clientOrder;
            ProvidedByServiceFuelVolume = providedFuel;
            OrderTimeFuelPrice = fuelPrice;
        }

        public ClientOrderInfo ClientOrder { get; }
        public int ProvidedByServiceFuelVolume { get; }
        public float OrderTimeFuelPrice { get; }
        public float OrderRevenue => ProvidedByServiceFuelVolume * OrderTimeFuelPrice;
        public bool IsSuccessful => ProvidedByServiceFuelVolume > 0;
    }
}
