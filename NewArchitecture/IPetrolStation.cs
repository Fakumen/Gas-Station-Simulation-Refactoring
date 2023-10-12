using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations
{
    public interface IPetrolStation
    {
        public StationType StationType { get; }
        public IReadOnlyDictionary<FuelType, float> ServicedFuelPrices { get; }
        public int CriticalFuelVolume { get; }//Dont require in interface?
        //public IReadOnlyList<ServedOrder> ServedOrders { get; } record elsewhere?
        //public event Action<IPetrolStation, ServedOrder> OrderServed;

        //public event Action<IPetrolStation, FuelType> CriticalFuelVolumeReached;//fuel amount?

        public bool HasFuelTypeInService(FuelType fuelType);
        public ServedOrder ServeOrder(ClientOrderInfo order);
    }

    public class PetrolStation : IPetrolStation
    {
        private readonly Dictionary<FuelType, float> _fuelPrices = new();
        private readonly Dictionary<FuelType, VolumeContainer> _fuelContainers = new();
        private readonly Dictionary<VolumeContainer, FuelType> _containerFuelTypes = new();

        public PetrolStation(
            StationType stationType,
            IReadOnlyDictionary<FuelType, float> fuelPrices, 
            IReadOnlyDictionary<FuelType, int> servicedFuelVolumes,
            int criticalFuelVolume)
        {
            StationType = stationType;
            CriticalFuelVolume = criticalFuelVolume;
            foreach (var fuelVolume in servicedFuelVolumes)
            {
                var fuelType = fuelVolume.Key;

                _fuelPrices.Add(fuelType, fuelPrices[fuelType]);

                var container = new VolumeContainer(fuelVolume.Value);
                container.Fill(container.EmptyVolume);
                container.VolumeFilled += OnFuelFilled;
                container.VolumeConsumed += OnFuelConsumed;
                _fuelContainers.Add(fuelType, container);
                _containerFuelTypes.Add(container, fuelType);
            }
        }

        public StationType StationType { get; }
        public IReadOnlyDictionary<FuelType, float> ServicedFuelPrices => _fuelPrices;
        public int CriticalFuelVolume { get; }

        public event Action<IPetrolStation, FuelType> CriticalFuelVolumeReached;

        public bool HasFuelTypeInService(FuelType fuelType) 
            => _fuelContainers.ContainsKey(fuelType);

        public ServedOrder ServeOrder(ClientOrderInfo order)
        {
            var fuelType = order.RequestedFuelType;
            if (!HasFuelTypeInService(fuelType))
                throw new InvalidOperationException("Client requested fuel type that is not in service.");
            var fuelToConsume = Math.Min(_fuelContainers[fuelType].FilledVolume, order.RequestedFuelVolume);
            _fuelContainers[fuelType].Consume(fuelToConsume);
            return new ServedOrder(order, fuelToConsume, ServicedFuelPrices[fuelType]);
        }

        private void OnFuelFilled(VolumeContainer container, int volume)
        {

        }

        private void OnFuelConsumed(VolumeContainer container, int volume)
        {
            var fuelType = _containerFuelTypes[container];
            if (container.FilledVolume <= CriticalFuelVolume)//TODO: check ordered reserve?
                CriticalFuelVolumeReached?.Invoke(this, fuelType);
        }
    }
}
