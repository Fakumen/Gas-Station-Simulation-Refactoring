using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasStations
{
    public class ClientOrder : IWaiter
    {
        private FuelType _fuelType;
        private int _appearInterval;
        private int _requestedVolume;
        private int ticksUntilOrderAppear;

        private Func<Random, IReadOnlyDictionary<FuelType, FuelContainer>, FuelType> _fuelTypeGenerator;

        public ClientType ClientType { get; }
        public int OrderAppearInterval => _appearInterval;
        public int TicksUntilOrderAppear
        {
            get => ticksUntilOrderAppear;
            set
            {
                ticksUntilOrderAppear = value;
                if (value < 0)
                    throw new ArgumentOutOfRangeException();
                if (value == 0)
                {
                    OrderAppeared?.Invoke(this);
                }
            }
        }

        public ClientOrder(
            ClientType clientType, 
            Func<Random, int> appearIntervalGenerator,
            Func<Random, int> requestedFuelVolumeGenerator,
            Func<Random, IReadOnlyDictionary<FuelType, FuelContainer>, FuelType> fuelTypeGenerator)//availableFuel
        {
            ClientType = clientType;
            var randomizer = GasStationSystem.Random;
            _appearInterval = appearIntervalGenerator(randomizer);
            _requestedVolume = requestedFuelVolumeGenerator(randomizer);
            _fuelTypeGenerator = fuelTypeGenerator;
            TicksUntilOrderAppear = OrderAppearInterval;
        }

        public event Action<ClientOrder> OrderAppeared;

        public FuelType GetRequestedFuel(IReadOnlyDictionary<FuelType, FuelContainer> availableFuel)
        {
            if (_fuelType != FuelType.None)
                return _fuelType;
            _fuelType = _fuelTypeGenerator(GasStationSystem.Random, availableFuel);
            return _fuelType;
        }

        public int GetRequestedVolume(int maximumAvailableVolume)
            => Math.Min(_requestedVolume, maximumAvailableVolume);

        public void WaitOneTick()
        {
            TicksUntilOrderAppear--;
        }
    }
}
