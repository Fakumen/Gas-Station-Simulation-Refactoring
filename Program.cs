using GasStations.Infrastructure;
using System;
using System.Linq;

namespace GasStations
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Console.SetWindowSize(220, 60);

            var orderProvider = new OrderProvider();
            var tankersProvider = new FuelTankersProvider();
            var stationsNetwork = new GasStationSystem(tankersProvider, 14, 16);
            var simulation = new Simulation(stationsNetwork, tankersProvider, orderProvider);

            var statistics = new SimulationStatisticsGatherer(simulation);
            IReportMaker reportMaker = new ReportMaker();
            var trackedStations = stationsNetwork.GasStations.ToHashSet();

            simulation.DayPassed += OnDayPassed;

            simulation.RunSimulation(24 * 60 * 10);

            ConsoleWriter.WriteLine("\n Симуляция окончена.", ConsoleColor.Red);

            while (true)
                Console.ReadLine();

            void OnDayPassed()
            {
                //Report display
                reportMaker.MakePerDayReport(statistics, s => trackedStations.Contains(s.StationModel));

                //Input handling
                ConsoleWriter.WriteLine("\n\tНажмите Enter, чтобы продолжить");
                var input = Console.ReadLine();
                if (input == "-debug stations")
                {
                    ConsoleWriter.Write("Station IDs to debug: ");
                    var idsInput = Console.ReadLine();
                    if (idsInput.ToLower() == "all")
                        trackedStations = stationsNetwork.GasStations.ToHashSet();
                    else
                    {
                        var ids = idsInput.Split().Select(s => int.Parse(s));
                        var stations = stationsNetwork.GasStations.ToList();
                        trackedStations = ids.Select(i => stations[i - 1]).ToHashSet();
                    }
                }
            }
        }
    }
}
