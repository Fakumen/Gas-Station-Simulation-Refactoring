﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations
{
    public class GasStation : ISimulationEntity
    {
        private readonly Dictionary<FuelType, float> _fuelPrices;
        private readonly Dictionary<FuelType, FuelContainer> _availableFuel;
        private int _ticksPassed = 0;

        public int ScheduleRefillInterval { get; }
        public int CriticalFuelLevel { get; }
        public int MinimalRefillVolume { get; }
        public StationType StationType { get; }
        public IReadOnlyDictionary<FuelType, FuelContainer> AvailableFuel => _availableFuel;

        public bool IsWaitingForGasolineTanker => _availableFuel.Any(e => e.Value.ReservedVolume > 0);

        public event Action<GasStation, ServedOrder> OrderServed;
        public event Action<GasStation, IReadOnlyDictionary<FuelType, int>> FuelVolumesRefillRequested;

        public GasStation(
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
            var requestedFuel = order.GetRequestedFuel(_availableFuel);
            var providedVolume = Math.Min(
                order.RequestedFuelVolume, _availableFuel[requestedFuel].CurrentVolume);
            _availableFuel[requestedFuel].Take(providedVolume);
            CheckCriticalFuelLevel(requestedFuel);
            var servedOrder = new ServedOrder(order, providedVolume, _fuelPrices[requestedFuel]);
            OrderServed?.Invoke(this, servedOrder);
            return servedOrder;
        }

        private void CheckCriticalFuelLevel(FuelType fuelToCheck)
        {
            if (_availableFuel[fuelToCheck].CurrentVolume <= CriticalFuelLevel)
                CheckRefillNecessity();
        }

        private void CheckRefillNecessity()
        {
            if (_availableFuel.Any(e => e.Value.EmptyUnreservedSpace >= MinimalRefillVolume))
            {
                var fuelToRefill = new Dictionary<FuelType, int>();
                foreach (var fuel in _availableFuel.Keys)
                {
                    var availableRefills = _availableFuel[fuel].EmptyUnreservedSpace / MinimalRefillVolume;
                    fuelToRefill.Add(fuel, MinimalRefillVolume * availableRefills);
                    _availableFuel[fuel].ReserveVolume(MinimalRefillVolume * availableRefills);
                }
                FuelVolumesRefillRequested?.Invoke(this, fuelToRefill);
            }
        }
    }
}
