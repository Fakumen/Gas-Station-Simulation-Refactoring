using GasStations.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GasStations
{
    public class ReportMaker
    {
        public static void WriteDayTitle(long ticksPassed)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("--------------------");
            Console.WriteLine(
                $"День {ticksPassed / (24 * 60)} закончился. Тактов(минут) прошло: {ticksPassed}.\n");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void ClientOrdersAverageIntervalReport(
            StationsNetworkStatisticsGatherer networkStatistics)
        {
            var clientTypes = EnumExtensions.GetValues<ClientType>();
            var avgOrderInterval = new Dictionary<ClientType, float>();
            foreach (var client in clientTypes)
            {
                var stationsWithClientOrders = networkStatistics.GasStations
                    .Where(s => s.QueuedOrders.Any(o => o.ClientType == client))
                    .ToArray();
                avgOrderInterval[client] = stationsWithClientOrders.Sum(s => s.OrdersIntervalSum[client])
                    / (float)stationsWithClientOrders.Sum(s => s.TotalQueuedOrdersByClientType[client]);
            }
            
            Console.WriteLine(
                $"Среднее время ожидания нового заказа: " +
                $"Для автомобилей: {avgOrderInterval[ClientType.Car]}(минут). " +
                $"Для грузовиков: {avgOrderInterval[ClientType.Truck]}(минут).");
        }

        public static void GSClientsRevenueReport(StationsNetworkStatisticsGatherer networkStatistics)
        {
            var stationaryGS = networkStatistics.GasStations
                .Where(s => s.StationType == StationType.Stationary)
                .ToArray();
            var miniGS = networkStatistics.GasStations
                .Where(s => s.StationType == StationType.Mini)
                .ToArray();

            var stationaryGSOrdersCount = stationaryGS.Sum(s => s.TotalQueuedOrders);
            var totalStationaryGSRevenue = stationaryGS.Sum(s => s.StationRevenue);
            var stationaryGSServedClients = stationaryGS.Sum(s => s.SuccessfullyServedClients);

            var miniGSTotalOrdersCount = miniGS.Sum(s => s.TotalQueuedOrders);
            var totalMiniGSRevenue = miniGS.Sum(s => s.StationRevenue);
            var miniGSServedClients = miniGS.Sum(s => s.SuccessfullyServedClients);

            Console.WriteLine(
                $"Обслуженных клиентов на АЗС: {stationaryGSServedClients}, на ААЗС: {miniGSServedClients}.");
            Console.WriteLine(
                $"Необслуженных клиентов: На АЗС: {stationaryGSOrdersCount - stationaryGSServedClients}, на ААЗС: {miniGSTotalOrdersCount - miniGSServedClients}.");
            Console.WriteLine(
                $"Выручка на АЗС: {totalStationaryGSRevenue}(руб), на ААЗС: {totalMiniGSRevenue}(руб).");
        }

        public static void GSStationsDetailedReport(
            StationsNetworkStatisticsGatherer networkStatistics, 
            Predicate<StationStatisticsGatherer> stationDisplayPredicate)
        {
            var stations = networkStatistics.GasStations.ToList();
            for (var i = 0; i < stations.Count; i++)
            {
                var station = stations[i];
                if (!stationDisplayPredicate(station))
                    continue;
                var stationName = $"АЗС {i + 1} ({station.StationType}):\n";
                Console.ForegroundColor = ConsoleColor.Yellow;
                if (station.StationType == StationType.Mini)
                {
                    stationName = "А" + stationName;
                    Console.ForegroundColor = ConsoleColor.Magenta;
                }
                Console.Write($" {stationName, -20}");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write($" \tКлиентов обслуженно: {station.SuccessfullyServedClients} из {station.TotalQueuedOrders};");
                Console.Write($" \tВызовов бензовоза: {station.FuelTankersCalls};");
                Console.Write($"\n\tТопливо:   ");
                foreach (var f in station.AvailableFuel)
                {
                    var container = f.Value;
                    var fuelInfo = $"[\"{f.Key}\": {container.CurrentVolume}{"(+" + container.ReservedVolume + ")"}]";
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"{fuelInfo, -22}");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($"(+{container.Income}/-{container.Consumption})\t");
                }
                Console.Write($"\t{(station.IsWaitingForGasolineTanker ? "(Ожидает бензовоза)" : "")}");
                Console.WriteLine();
            }
        }

        public static void TotalGasTankersReport(GasStationSystem stationsNetwork)
        {
            var tankers = stationsNetwork.GasolineTankers;
            var tankers2 = tankers.Where(t => t.TanksCount == 2).ToArray();
            var tankers3 = tankers.Where(t => t.TanksCount == 3).ToArray();
            var tankers2TotalDrivesCount = tankers2.Sum(t => t.DrivesCount);
            var tankers3TotalDrivesCount = tankers3.Sum(t => t.DrivesCount);
            var unusedTankers = tankers.Count(t => !t.IsBusy && t.LoadedFuel.Count == 0);
            var unusedTankers2 = tankers2.Count(t => !t.IsBusy && t.LoadedFuel.Count == 0);
            var unusedTankers3 = tankers3.Count(t => !t.IsBusy && t.LoadedFuel.Count == 0);
            Console.WriteLine($"Бензовозов в парке: {tankers.Count}. Двухсекционных: {tankers2.Count()}. Трехсекционных: {tankers3.Count()}.");
            Console.WriteLine($"Незанятых в данный момент бензовозов: {unusedTankers}/{tankers.Count}. Двухсекционных: {unusedTankers2}. Трехсекционных: {unusedTankers3}.");
            Console.WriteLine($"Всего рейсов двухсекционных бензовозов: {tankers2TotalDrivesCount}, трехсекционных: {tankers3TotalDrivesCount}.");
        }
    }
}
