using System;
using System.Collections.Generic;

namespace GasStations
{
    public class ClientOrder : ISimulationEntity
    {
        private int _appearInterval;
        private int _ticksUntilOrderAppear;
        private Func<Random, IReadOnlyDictionary<FuelType, IReadOnlyFuelContainer>, FuelType> _fuelTypeSelector;

        public ClientOrder(
            ClientType clientType,
            Func<Random, int> appearIntervalGenerator,
            Func<Random, int> requestedFuelVolumeGenerator,
            Func<Random, IReadOnlyDictionary<FuelType, IReadOnlyFuelContainer>, FuelType> fuelTypeSelector)
        {
            ClientType = clientType;
            var randomizer = Simulation.Randomizer;
            _appearInterval = appearIntervalGenerator(randomizer);
            RequestedFuelVolume = requestedFuelVolumeGenerator(randomizer);
            _fuelTypeSelector = fuelTypeSelector;
            _ticksUntilOrderAppear = OrderAppearInterval;
        }

        public ClientType ClientType { get; }
        public FuelType RequestedFuel { get; private set; }
        public int RequestedFuelVolume { get; }
        public int OrderAppearInterval => _appearInterval;

        public event Action<ClientOrder> OrderAppeared;

        public FuelType GetRequestedFuel(
            IReadOnlyDictionary<FuelType, IReadOnlyFuelContainer> availableFuel)
        {
            if (RequestedFuel != FuelType.None)
                return RequestedFuel;
            RequestedFuel = _fuelTypeSelector(Simulation.Randomizer, availableFuel);
            if (RequestedFuel == FuelType.None)
                throw new InvalidProgramException();
            return RequestedFuel;
        }

        public void OnSimulationTickPassed()
        {
            _ticksUntilOrderAppear--;
            if (_ticksUntilOrderAppear == 0)
                OrderAppeared?.Invoke(this);
        }
    }
}
