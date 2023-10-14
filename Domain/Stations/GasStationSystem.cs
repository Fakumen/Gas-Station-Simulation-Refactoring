using System;
using System.Collections.Generic;

namespace GasStations
{
    public class GasStationSystem
    {
        private readonly List<GasStation> _stations = new();
        private readonly Dictionary<FuelType, float> _fuelPrices = new()
        {
            { FuelType.Petrol92, 45.6f },
            { FuelType.Petrol95, 48.2f },
            { FuelType.Petrol98, 50.3f },
            { FuelType.Diesel, 51.5f }
        };

        public GasStationSystem(
            int stationaryStationsCount, int miniStationsCount)
        {
            for (var i = 0; i < stationaryStationsCount; i++)
            {
                var avFuel = new Dictionary<FuelType, int>
                {
                    { FuelType.Petrol92, 30000 },
                    { FuelType.Petrol95, 16000 },
                    { FuelType.Petrol98, 16000 },
                    { FuelType.Diesel, 30000 }
                };
                var station = new GasStation(StationType.Stationary, _fuelPrices, avFuel);
                _stations.Add(station);
                station.FuelVolumeToRefillRequested += FuelRefillRequestedByStation;
            }

            for (var i = 0; i < miniStationsCount; i++)
            {
                var avFuel = new Dictionary<FuelType, int>
                {
                    { FuelType.Petrol92, 16000 },
                    { FuelType.Petrol95, 15000 }
                };
                var station = new GasStation(StationType.Mini, _fuelPrices, avFuel);
                _stations.Add(station);
                station.FuelVolumeToRefillRequested += FuelRefillRequestedByStation;
            }
        }

        public IReadOnlyList<GasStation> GasStations => _stations;

        public event Action<GasStation, IReadOnlyDictionary<FuelType, int>> RefillRequestedByStation;

        private void FuelRefillRequestedByStation(
            GasStation station, IReadOnlyDictionary<FuelType, int> fuelVolumes)
        {
            RefillRequestedByStation?.Invoke(station, fuelVolumes);
        }
    }
}
