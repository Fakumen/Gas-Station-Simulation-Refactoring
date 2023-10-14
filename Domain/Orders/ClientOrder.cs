using System;
using System.Collections.Generic;

namespace GasStations
{
    public class ClientOrder : IWaiter
    {
        private int _appearInterval;
        private int _ticksUntilOrderAppear;
        private Func<Random, IReadOnlyDictionary<FuelType, FuelContainer>, FuelType> _fuelTypeGenerator;

        public ClientOrder(
            ClientType clientType,
            Func<Random, int> appearIntervalGenerator,
            Func<Random, int> requestedFuelVolumeGenerator,
            Func<Random, IReadOnlyDictionary<FuelType, FuelContainer>, FuelType> fuelTypeGenerator)//availableFuel
        {
            ClientType = clientType;
            var randomizer = Simulation.Randomizer;
            _appearInterval = appearIntervalGenerator(randomizer);
            RequestedFuelVolume = requestedFuelVolumeGenerator(randomizer);
            _fuelTypeGenerator = fuelTypeGenerator;
            _ticksUntilOrderAppear = OrderAppearInterval;
        }

        public ClientType ClientType { get; }
        public FuelType RequestedFuel { get; private set; }
        public int RequestedFuelVolume { get; }
        public int OrderAppearInterval => _appearInterval;//TODO: hide and track via event

        public event Action<ClientOrder> OrderAppeared;

        public FuelType GetRequestedFuel(IReadOnlyDictionary<FuelType, FuelContainer> availableFuel)
        {
            if (RequestedFuel != FuelType.None)
                return RequestedFuel;
            RequestedFuel = _fuelTypeGenerator(Simulation.Randomizer, availableFuel);
            if (RequestedFuel == FuelType.None)
                throw new InvalidProgramException();
            return RequestedFuel;
        }

        public void WaitOneTick()
        {
            _ticksUntilOrderAppear--;
            if (_ticksUntilOrderAppear == 0)
                OrderAppeared?.Invoke(this);
        }
    }
}
