using System;

namespace GasStations
{
    public class SimulationJob : ISimulationJob
    {
        public SimulationJob(int durationInTicks)
        {
            if (durationInTicks <= 0)
                throw new ArgumentOutOfRangeException(nameof(durationInTicks));
            TotalDuration = durationInTicks;
            LeftDuration = durationInTicks;
        }

        public int TotalDuration { get; }
        public int LeftDuration { get; private set; }

        public event Action<ISimulationJob> JobFinished;

        public void OnSimulationTickPassed()
        {
            LeftDuration--;
            if (LeftDuration == 0)
                JobFinished?.Invoke(this);
        }
    }
}
