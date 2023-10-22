using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations
{
    public class FuelStation : ISimulationEntity
    {
        private readonly Dictionary<FuelType, float> _fuelPrices;
        private readonly Dictionary<FuelType, FuelContainer> _availableFuel;
        private int _ticksPassed = 0;

        public int ScheduleRefillInterval { get; }
        public int CriticalFuelLevel { get; }
        public int MinimalRefillVolume { get; }
        public StationType StationType { get; }
        public IReadOnlyDictionary<FuelType, IReadOnlyFuelContainer> AvailableFuel 
            => _availableFuel.ToDictionary(kv => kv.Key, kv => (IReadOnlyFuelContainer)kv.Value);

        public bool HasReservedFuelVolumes => _availableFuel.Any(e => e.Value.ReservedVolume > 0);

        public event Action<FuelStation, ServedOrder> OrderServed;
        public event Action<FuelStation, IReadOnlyDictionary<FuelType, int>> FuelVolumesRefillRequested;

        public FuelStation(
            StationType stationType, 
            IReadOnlyDictionary<FuelType, float> fuelPrices,
            IReadOnlyDictionary<FuelType, int> fuelSectionVolumes, 
            int minimalRefillVolume,
            int refillInterval = 24 * 60, 
            int criticalFuelLevel = 1000)
        {
            StationType = stationType;
            _fuelPrices = fuelPrices.ToDictionary(kv => kv.Key, kv => kv.Value);
            _availableFuel = fuelSectionVolumes.ToDictionary(
                kv => kv.Key, 
                kv => new FuelContainer(kv.Value));
            ScheduleRefillInterval = refillInterval;
            CriticalFuelLevel = criticalFuelLevel;
            MinimalRefillVolume = minimalRefillVolume;
        }

        public void OnSimulationTickPassed()
        {
            if (_ticksPassed % ScheduleRefillInterval == 0 && _ticksPassed != 0)
            {
                CheckRefillNecessity();
            }
            _ticksPassed++;
        }

        public void Refill(FuelType fuel, int volume)
        {
            if (volume < 0) throw new ArgumentException();
            _availableFuel[fuel].Fill(volume);
        }

        public ServedOrder ServeOrder(ClientOrder order)
        {
            var requestedFuel = order.GetRequestedFuel(AvailableFuel);
            var providedVolume = Math.Min(
                order.RequestedFuelVolume, _availableFuel[requestedFuel].FilledVolume);
            _availableFuel[requestedFuel].Consume(providedVolume);
            CheckCriticalFuelLevel(requestedFuel);
            var servedOrder = new ServedOrder(order, providedVolume, _fuelPrices[requestedFuel]);
            OrderServed?.Invoke(this, servedOrder);
            return servedOrder;
        }

        private void CheckCriticalFuelLevel(FuelType fuelToCheck)
        {
            if (_availableFuel[fuelToCheck].FilledVolume <= CriticalFuelLevel)
                CheckRefillNecessity();
        }

        private void CheckRefillNecessity()
        {
            if (_availableFuel.Any(e => e.Value.EmptyUnreservedVolume >= MinimalRefillVolume))
            {
                var fuelToRefill = new Dictionary<FuelType, int>();
                foreach (var fuel in _availableFuel.Keys)
                {
                    var availableRefills = _availableFuel[fuel].EmptyUnreservedVolume / MinimalRefillVolume;
                    fuelToRefill.Add(fuel, MinimalRefillVolume * availableRefills);
                    _availableFuel[fuel].ReserveVolume(MinimalRefillVolume * availableRefills);
                }
                FuelVolumesRefillRequested?.Invoke(this, fuelToRefill);
            }
        }
    }
}
