using System;

namespace GasStations
{
    public interface IFuelTankerJob : ISimulationEntity
    {
        public int TotalDuration { get; }
        public int LeftDuration { get; }

        public event Action<IFuelTankerJob> JobFinished;
    }
}
