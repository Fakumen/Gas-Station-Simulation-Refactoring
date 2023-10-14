using System;
using System.Linq;

namespace GasStations
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Console.SetWindowSize(220, 60);

            var stationsNetwork = new GasStationSystem(14, 16);
            var orderProvider = new OrderProvider();
            var simulation = new Simulation(stationsNetwork, orderProvider);

            var networkStatistics = new StationsNetworkStatisticsGatherer(stationsNetwork);

            var trackedStations = networkStatistics.GasStations.ToHashSet();
            simulation.DayPassed += OnDayPassed;

            simulation.RunSimulation(24 * 60 * 10);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n Симуляция окончена.");

            while (true)
                Console.ReadLine();

            void OnDayPassed()
            {
                //Report display
                ReportMaker.WriteDayTitle(simulation.PassedSimulationTicks);
                ReportMaker.GSStationsDetailedReport(networkStatistics, s => trackedStations.Contains(s));
                Console.WriteLine();
                ReportMaker.GSClientsRevenueReport(networkStatistics);
                ReportMaker.ClientOrdersAverageIntervalReport(networkStatistics);
                ReportMaker.TotalGasTankersReport(simulation.GasolineTankers);

                //Input handling
                Console.WriteLine("\n\tНажмите Enter, чтобы продолжить");
                var input = Console.ReadLine();
                if (input == "-debug stations")
                {
                    Console.Write("Station IDs to debug: ");
                    var idsInput = Console.ReadLine();
                    if (idsInput.ToLower() == "all")
                        trackedStations = networkStatistics.GasStations.ToHashSet();
                    else
                    {
                        var ids = idsInput.Split().Select(s => int.Parse(s));
                        var stations = networkStatistics.GasStations.ToList();
                        trackedStations = ids.Select(i => stations[i - 1]).ToHashSet();
                    }
                }
            }
        }
    }
}
