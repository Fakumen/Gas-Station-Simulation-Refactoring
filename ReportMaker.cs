using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasStations
{
    public class ReportMaker
    {
        //public static void TotalUnservedClientsReport()
        //{
        //    var stationaryGSOrdersCount = GasStationSystem.StationaryGS.Sum(s => s.TotalCarOrders) + GasStationSystem.StationaryGS.Sum(s => s.TotalTruckOrders);
        //    var miniGSTotalOrdersCount = GasStationSystem.MiniGS.Sum(s => s.TotalCarOrders) + GasStationSystem.MiniGS.Sum(s => s.TotalTruckOrders);
        //    var stationaryGSServedClients = GasStationSystem.StationaryGS.Sum(s => s.ServedClients);
        //    var miniGSServedClients = GasStationSystem.MiniGS.Sum(s => s.ServedClients);
        //    Console.WriteLine($"Необслуженных клиентов: На АЗС: {stationaryGSOrdersCount - stationaryGSServedClients}. На ААЗС: {miniGSTotalOrdersCount - miniGSServedClients}.");
        //}

        public static void WriteDayTitle(int ticksPassed)
        {

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("--------------------");
            Console.WriteLine($"День {ticksPassed / (24 * 60)} закончился. Тактов(минут) прошло: {ticksPassed}.\n");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void ClientOrdersAverageIntervalReport()
        {
            var carOrderIntervalSum = GasStationSystem.StationaryGS.Sum(s => s.CarOrdersIntervalSum) + GasStationSystem.MiniGS.Sum(s => s.CarOrdersIntervalSum);
            var truckOrderIntervalSum = GasStationSystem.StationaryGS.Sum(s => s.TruckOrdersIntervalSum) + GasStationSystem.MiniGS.Sum(s => s.TruckOrdersIntervalSum);
            var carOrdersCount = GasStationSystem.StationaryGS.Sum(s => s.TotalCarOrders) + GasStationSystem.MiniGS.Sum(s => s.TotalCarOrders);
            var truckOrdersCount = GasStationSystem.StationaryGS.Sum(s => s.TotalTruckOrders) + GasStationSystem.MiniGS.Sum(s => s.TotalTruckOrders);
            Console.WriteLine($"Среднее время ожидания нового заказа: Для автомобилей: {(float)carOrderIntervalSum / carOrdersCount}(минут). Для грузовиков: {(float)truckOrderIntervalSum / truckOrdersCount}(минут).");
        }

        public static void GSClientsRevenueReport()
        {
            var stationaryGSOrdersCount = GasStationSystem.StationaryGS.Sum(s => s.TotalCarOrders) + GasStationSystem.StationaryGS.Sum(s => s.TotalTruckOrders);
            var miniGSTotalOrdersCount = GasStationSystem.MiniGS.Sum(s => s.TotalCarOrders) + GasStationSystem.MiniGS.Sum(s => s.TotalTruckOrders);
            var totalStationaryGSRevenue = GasStationSystem.StationaryGS.Sum(s => s.Revenue);
            var totalMiniGSRevenue = GasStationSystem.MiniGS.Sum(s => s.Revenue);
            var stationaryGSServedClients = GasStationSystem.StationaryGS.Sum(s => s.ServedClients);
            var miniGSServedClients = GasStationSystem.MiniGS.Sum(s => s.ServedClients);
            Console.WriteLine($"Обслуженных клиентов на АЗС: {stationaryGSServedClients}, на ААЗС: {miniGSServedClients}.");
            Console.WriteLine($"Необслуженных клиентов: На АЗС: {stationaryGSOrdersCount - stationaryGSServedClients}, на ААЗС: {miniGSTotalOrdersCount - miniGSServedClients}.");
            Console.WriteLine($"Выручка на АЗС: {totalStationaryGSRevenue}(руб), на ААЗС: {totalMiniGSRevenue}(руб).");
        }

        public static void GSDetailedReport()
        {
            var stations = GasStationSystem.StationaryGS.Concat(GasStationSystem.MiniGS);
            GSDetailedReport(Enumerable.Range(1, stations.Count()));
        }

        public static void GSDetailedReport(params int[] stationIDs) => GSDetailedReport(stationIDs as IEnumerable<int>);

        public static void GSDetailedReport(IEnumerable<int> stationIDS)
        {
            var stations = GasStationSystem.StationaryGS.Concat(GasStationSystem.MiniGS).ToArray();
            foreach (var j in stationIDS)
            {
                var i = j - 1;
                var station = stations[i];
                var stationName = $"АЗС {i + 1} ({station.StationType.ToString()}):\n";
                Console.ForegroundColor = ConsoleColor.Yellow;
                if (station.StationType == StationType.Mini)
                {
                    stationName = "А" + stationName;
                    Console.ForegroundColor = ConsoleColor.Magenta;
                }
                Console.Write($" {stationName, -20}");
                Console.ForegroundColor = ConsoleColor.Gray;
                //Console.Write($" \tТактов пройдено: {station.TicksPassed};");
                Console.Write($" \tКлиентов обслуженно: {station.ServedClients} из {station.TotalOrders};");
                Console.Write($" \tВызовов бензовоза: {station.GasolineTankersCalls};");
                Console.Write($"\n\tТопливо:   ");
                foreach (var f in station.AvailableFuel)
                {
                    var container = f.Value;
                    var fuelInfo = $"[\"{f.Key.FuelTypeName}\": {container.CurrentVolume}{"(+" + container.ReservedVolume + ")"}]";
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"{fuelInfo, -22}");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($"(+{container.Income}/-{container.Consumption})\t");
                }
                Console.Write($"\t{(station.IsWaitingForGasolineTanker ? "(Ожидает бензовоза)" : "")}");
                Console.WriteLine();
            }
        }

        public static void TotalGasTankersReport()
        {
            var tankers = GasStationSystem.GasolineTankers;
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
