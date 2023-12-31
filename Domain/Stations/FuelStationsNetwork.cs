﻿using System.Collections.Generic;
using System.Linq;

namespace GasStations
{
    public class FuelStationsNetwork
    {
        private FuelTankersProvider _fuelTankersProvider;

        private readonly List<FuelStation> _stations = new();
        private readonly Dictionary<FuelType, float> _fuelPrices = new()
        {
            { FuelType.Petrol92, 45.6f },
            { FuelType.Petrol95, 48.2f },
            { FuelType.Petrol98, 50.3f },
            { FuelType.Diesel, 51.5f }
        };

        public FuelStationsNetwork(
            FuelTankersProvider fuelTankersProvider, 
            int stationaryStationsCount, int miniStationsCount)
        {
            _fuelTankersProvider = fuelTankersProvider;
            for (var i = 0; i < stationaryStationsCount; i++)
            {
                var avFuel = new Dictionary<FuelType, int>
                {
                    { FuelType.Petrol92, 30000 },
                    { FuelType.Petrol95, 16000 },
                    { FuelType.Petrol98, 16000 },
                    { FuelType.Diesel, 30000 }
                };
                var station = new FuelStation(
                    StationType.Stationary, _fuelPrices, avFuel, fuelTankersProvider.TankerVolumeCapacity);
                _stations.Add(station);
                station.FuelVolumesRefillRequested += OrderFuelToStation;
            }

            for (var i = 0; i < miniStationsCount; i++)
            {
                var avFuel = new Dictionary<FuelType, int>
                {
                    { FuelType.Petrol92, 16000 },
                    { FuelType.Petrol95, 15000 }
                };
                var station = new FuelStation(
                    StationType.Mini, _fuelPrices, avFuel, fuelTankersProvider.TankerVolumeCapacity);
                _stations.Add(station);
                station.FuelVolumesRefillRequested += OrderFuelToStation;
            }
        }

        public IReadOnlyList<FuelStation> Stations => _stations;

        private void OrderFuelToStation(
            FuelStation station, IReadOnlyDictionary<FuelType, int> fuelVolumes)
        {
            var minimalRefillVolume = _fuelTankersProvider.TankerVolumeCapacity;
            if (fuelVolumes.Any(s => s.Value < 0 || s.Value % minimalRefillVolume != 0))
                throw new System.ArgumentException();
            foreach (var fuel in fuelVolumes.Keys)
            {
                for (var i = 0; i < fuelVolumes[fuel] / minimalRefillVolume; i++)
                {
                    _fuelTankersProvider.OrderFuelSection(station, fuel);
                }
            }
        }
    }
}
