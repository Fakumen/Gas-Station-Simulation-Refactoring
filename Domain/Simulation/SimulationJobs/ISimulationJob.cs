using System;

namespace GasStations
{
    public interface ISimulationJob : ISimulationEntity
    {
        public int TotalDuration { get; }
        public int LeftDuration { get; }

        public event Action<ISimulationJob> JobFinished;
    }
}
