using System;

namespace GasStations
{
    public interface IReportMaker
    {
        public void MakePerDayReport(SimulationStatisticsGatherer statistics);

        public void MakePerDayReport(
            SimulationStatisticsGatherer statistics,
            Predicate<StationStatisticsGatherer> stationDisplayPredicate);
    }
}
