using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasStations
{
    public class FuelContainer
    {
        #region Statistics
        public int Consumption { get; private set; } //Расход
        public int Income { get; private set; } //Приход
        #endregion

        public readonly int MaximumCapacity;
        public int CurrentVolume { get; private set; }
        public int ReservedVolume { get; private set; }
        public int EmptySpace => MaximumCapacity - CurrentVolume;
        public int EmptyUnreservedSpace => EmptySpace - ReservedVolume;

        public FuelContainer(int maximumCapacity)
        {
            MaximumCapacity = maximumCapacity;
            CurrentVolume = maximumCapacity;
        }

        public void Take(int volume)
        {
            if (volume > CurrentVolume) throw new ArgumentException();
            CurrentVolume -= volume;
            Consumption += volume;
        }

        public void Fill(int volume)
        {
            if (volume > EmptySpace) throw new ArgumentException();
            ReservedVolume -= volume;
            CurrentVolume += volume;
            Income += volume;
        }

        public void ReserveSpace(int volume)
        {
            if (volume + ReservedVolume > EmptySpace) throw new ArgumentException();
            ReservedVolume += volume;
        }

        private void UnreserveSpace(int volume)
        {
            if (volume > ReservedVolume) throw new ArgumentException();
            ReservedVolume -= volume;
        }
    }
}
