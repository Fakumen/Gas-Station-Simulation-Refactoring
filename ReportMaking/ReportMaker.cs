using GasStations.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations
{
    public class ReportMaker : IReportMaker
    {
        public void MakePerDayReport(SimulationStatisticsGatherer statistics)
            => MakePerDayReport(statistics, s => true);

        public void MakePerDayReport(
            SimulationStatisticsGatherer statistics,
            Predicate<StationStatisticsGatherer> stationDisplayPredicate)
        {
            WriteDayTitle(statistics.TrackingSimulation.PassedSimulationTicks);
            ShowStationsDetailedStatistics(
                statistics.StationsNetworkStatistics, 
                stationDisplayPredicate,
                statistics.OrdersAppearStatistics);
            ConsoleWriter.WriteLine();
            ShowStationsServiceResults(
                statistics.StationsNetworkStatistics, 
                statistics.OrdersAppearStatistics);
            ShowAverageOrdersInterval(statistics.OrdersAppearStatistics);
            //TODO: dependency from TankersManagerStatistics
            ShowFuelTankersStatistics(
                statistics.TrackingSimulation.FuelTankersProvider.FuelTankers);
        }

        private static void WriteDayTitle(long ticksPassed)
        {
            ConsoleWriter.WriteLine("--------------------", ConsoleColor.Red);
            ConsoleWriter.WriteLine(
                $"День {ticksPassed / (24 * 60)} закончился. Тактов(минут) прошло: {ticksPassed}.\n",
                ConsoleColor.Red);
        }

        private static void ShowAverageOrdersInterval(
            OrdersAppearStatisticsGatherer ordersStatistics)
        {
            var clientTypes = EnumExtensions.GetValues<ClientType>();
            var avgOrderInterval = clientTypes
                .ToDictionary(
                c => c, 
                c => ordersStatistics.GetAverageOrdersIntervalByClientType(c));
            
            ConsoleWriter.WriteLine(
                $"Среднее время ожидания нового заказа: " +
                $"Для автомобилей: {avgOrderInterval[ClientType.Car]}(минут). " +
                $"Для грузовиков: {avgOrderInterval[ClientType.Truck]}(минут).");
        }

        private static void ShowStationsServiceResults(
            StationsNetworkStatisticsGatherer networkStatistics,
            OrdersAppearStatisticsGatherer ordersStatistics)
        {
            var stationTypes = EnumExtensions.GetValues<StationType>();
            var stationStatistics = stationTypes
                .ToDictionary(
                s => s,
                s => new 
                { 
                    SuccessfulOrders = networkStatistics.GetTotalSuccessfullyServedOrdersByStationType(s), 
                    TotalOrders = ordersStatistics.GetQueuedOrdersByStationType(s).Length, 
                    Revenue = networkStatistics.GetTotalRevenueByStationType(s)
                });

            var stationaryStatistics = stationStatistics[StationType.Stationary];
            var miniStatistics = stationStatistics[StationType.Mini];

            ConsoleWriter.WriteLine(
                $"Обслуженных клиентов " +
                $"на АЗС: {stationaryStatistics.SuccessfulOrders}, " +
                $"на ААЗС: {miniStatistics.SuccessfulOrders}.");
            ConsoleWriter.WriteLine(
                $"Необслуженных клиентов: " +
                $"На АЗС: {stationaryStatistics.TotalOrders - stationaryStatistics.SuccessfulOrders}, " +
                $"на ААЗС: {miniStatistics.TotalOrders - miniStatistics.SuccessfulOrders}.");
            ConsoleWriter.WriteLine(
                $"Выручка " +
                $"на АЗС: {stationaryStatistics.Revenue}(руб), " +
                $"на ААЗС: {miniStatistics.Revenue}(руб).");
        }

        private static void ShowStationsDetailedStatistics(
            StationsNetworkStatisticsGatherer networkStatistics, 
            Predicate<StationStatisticsGatherer> stationDisplayPredicate,
            OrdersAppearStatisticsGatherer ordersStatistics)
        {
            var stations = networkStatistics.Stations.ToList();
            for (var i = 0; i < stations.Count; i++)
            {
                var station = stations[i];
                if (!stationDisplayPredicate(station))
                    continue;
                var stationName = $"АЗС {i + 1} ({station.StationType}):\n";
                var stationColor = ConsoleColor.Yellow;
                if (station.StationType == StationType.Mini)
                {
                    stationName = $"А{stationName}";
                    stationColor = ConsoleColor.Magenta;
                }
                ConsoleWriter.Write($" {stationName, -20}", stationColor);

                ConsoleWriter.Write($" \tКлиентов обслуженно: {station.SuccessfullyServedClients} " +
                    $"из {ordersStatistics.GetQueuedOrdersByStation(station.StationModel).Count};");

                ConsoleWriter.Write($" \tВызовов бензовоза: {station.RefillRequestsCount};");

                ConsoleWriter.Write($"\n\tТопливо:   ");
                foreach (var fuelType in station.AvailableFuel.Keys)
                {
                    var container = station.AvailableFuel[fuelType];
                    var fuelInfo = $"[\"{fuelType}\": {container.FilledVolume}(+{container.ReservedVolume})]";
                    ConsoleWriter.Write($"{fuelInfo,-22}", ConsoleColor.Green);
                    Console.Write($"(+{container.VolumeIncome}/-{container.VolumeConsumption})\t");
                }
                if (station.HasReservedFuelVolumes)
                    ConsoleWriter.Write($"\t(Ожидает бензовоза)");

                ConsoleWriter.WriteLine();
            }
        }

        private static void ShowFuelTankersStatistics(IEnumerable<FuelTanker> tankers)
        {
            var tankersList = tankers.ToList();
            var tankersByTanks = tankersList
                .GroupBy(t => t.TanksCount)
                .ToDictionary(g => g.Key, g => g.ToArray());
            var necessaryKeys = new[] { 2, 3 };
            foreach (var tanksCount in necessaryKeys)
                if (!tankersByTanks.ContainsKey(tanksCount)) 
                    tankersByTanks.Add(tanksCount, new FuelTanker[0]);

            var tankersStatisticsByTanks = tankersByTanks
                .ToDictionary(
                kv => kv.Key, 
                kv => new
                {
                    TotalCount = kv.Value.Length,
                    DrivesCount = kv.Value.Sum(t => t.DrivesCount),
                    UnusedCount = kv.Value.Count(t => !t.IsBusy && t.LoadedTanksCount == 0)
                });

            var unusedTankers = tankersStatisticsByTanks.Values.Sum(s => s.UnusedCount);
            ConsoleWriter.WriteLine(
                $"Бензовозов в парке: {tankersList.Count}. " +
                $"Двухсекционных: {tankersStatisticsByTanks[2].TotalCount}. " +
                $"Трехсекционных: {tankersStatisticsByTanks[3].TotalCount}.");
            ConsoleWriter.WriteLine(
                $"Незанятых в данный момент бензовозов: {unusedTankers}/{tankersList.Count}. " +
                $"Двухсекционных: {tankersStatisticsByTanks[2].UnusedCount}. " +
                $"Трехсекционных: {tankersStatisticsByTanks[3].UnusedCount}.");
            ConsoleWriter.WriteLine(
                $"Всего рейсов " +
                $"двухсекционных бензовозов: {tankersStatisticsByTanks[2].DrivesCount}, " +
                $"трехсекционных: {tankersStatisticsByTanks[3].DrivesCount}.");
        }
    }
}
