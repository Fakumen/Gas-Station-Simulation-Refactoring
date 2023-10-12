using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations
{
    public class VolumeContainer
    {
        //export statistics
        //make events
        #region Statistics
        public int Consumption { get; private set; } //Расход
        public int Income { get; private set; } //Приход
        #endregion

        public int MaximumVolumeCapacity { get; }
        public int FilledVolume { get; private set; }
        public int ReservedVolume { get; private set; }
        public int EmptyVolume => MaximumVolumeCapacity - FilledVolume;
        public int EmptyUnreservedSpace => EmptyVolume - ReservedVolume;

        public event Action<VolumeContainer, int> VolumeFilled;
        public event Action<VolumeContainer, int> VolumeConsumed;

        public VolumeContainer(int maximumCapacity)
        {
            if (maximumCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(maximumCapacity));
            MaximumVolumeCapacity = maximumCapacity;
            //remove auto fill
            FilledVolume = maximumCapacity;
        }

        public void Consume(int volume)
        {
            if (volume > FilledVolume)
                throw new ArgumentOutOfRangeException(nameof(volume));
            FilledVolume -= volume;
            Consumption += volume;
            VolumeConsumed?.Invoke(this, volume);
        }

        public void Fill(int volume)
        {
            if (volume > EmptyVolume)
                throw new ArgumentOutOfRangeException(nameof(volume));
            ReservedVolume -= volume;
            FilledVolume += volume;
            Income += volume;
            VolumeFilled?.Invoke(this, volume);
        }

        public void ReserveSpace(int volume)
        {
            if (volume + ReservedVolume > EmptyVolume) throw new ArgumentException();
            ReservedVolume += volume;
        }

        private void UnreserveSpace(int volume)
        {
            if (volume > ReservedVolume) throw new ArgumentException();
            ReservedVolume -= volume;
        }
    }
}
