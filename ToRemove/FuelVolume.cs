using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasStations
{
    public struct FuelVolume
    {
        private int _volume;

        public FuelType FuelType { get; }
        public int Volume
        {
            get => _volume;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException();
                var previousVolume = _volume;
                var newVolume = value;
                _volume = value;
                if (previousVolume != newVolume)
                    VolumeChanged?.Invoke(new(previousVolume, newVolume));
            }
        }

        public Action<FuelVolumeChangedEventArgs> VolumeChanged { get; set; }
    }

    public record FuelVolumeChangedEventArgs
    {
        public FuelVolumeChangedEventArgs(int previousVolume, int newVolume)
        {
            PreviousVolume = previousVolume;
            NewVolume = newVolume;
        }

        public int PreviousVolume { get; }
        public int NewVolume { get; }
    }
}
