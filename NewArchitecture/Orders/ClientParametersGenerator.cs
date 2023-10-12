using System;

namespace GasStations
{
    public class ClientParametersGenerator
    {
        private readonly Func<Random, int> _intervalGenerator;
        private readonly Func<Random, IPetrolStation, FuelType> _fuelTypeGenerator;
        private readonly Func<Random, IPetrolStation, int> _fuelVolumeGenerator;

        public ClientParametersGenerator(
            Func<Random, int> intervalGenerator, 
            Func<Random, IPetrolStation, FuelType> fuelTypeGenerator, 
            Func<Random, IPetrolStation, int> fuelVolumeGenerator)
        {
            _intervalGenerator = intervalGenerator;
            _fuelTypeGenerator = fuelTypeGenerator;
            _fuelVolumeGenerator = fuelVolumeGenerator;
        }

        public int GenerateAppearInterval(Random randomizer)
            => _intervalGenerator(randomizer);

        public FuelType GenerateFuelType(Random randomizer, IPetrolStation station)
            => _fuelTypeGenerator(randomizer, station);

        public int GenerateFuelVolume(Random randomizer, IPetrolStation station)
            => _fuelVolumeGenerator(randomizer, station);
    }
}
