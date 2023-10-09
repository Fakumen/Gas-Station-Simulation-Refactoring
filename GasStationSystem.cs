using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasStations
{
    public static class GasStationSystem
    {
        public readonly static Random Random = new Random();

        public readonly static List<Fuel> FuelTypes = new List<Fuel>();

        public readonly static List<GasStation> StationaryGS = new List<GasStation>();
        public readonly static List<GasStation> MiniGS = new List<GasStation>();
        public readonly static List<GasolineTanker> GasolineTankers = new List<GasolineTanker>();
        public static List<OrderedFuel> TotalOrdersInCurrentTick = new List<OrderedFuel>();
        public static IEnumerable<GasolineTanker> FreeGasolineTankers => GasolineTankers.Where(t => !t.IsBusy && t.EmptyTanksCount > 0);
        public static IEnumerable<GasolineTanker> GasolineTankersWaitingDeliveryStart => GasolineTankers.Where(t => !t.IsBusy && t.EmptyTanksCount < t.TanksCount);

        private static void Initialize()
        {
            FuelTypes.Add(new Fuel("92", 45.6f));
            FuelTypes.Add(new Fuel("95", 48.2f));
            FuelTypes.Add(new Fuel("98", 50.3f));
            FuelTypes.Add(new Fuel("Дт", 51.5f));

            for (var i = 0; i < 14; i++)
            {
                var avFuel = new Dictionary<Fuel, FuelContainer>
                {
                    { FuelTypes[0], new FuelContainer(30000) },//92
                    { FuelTypes[1], new FuelContainer(16000) },//95
                    { FuelTypes[2], new FuelContainer(16000) },//98
                    { FuelTypes[3], new FuelContainer(30000) } //Дт
                };
                var station = new GasStation(StationType.Stationary, avFuel);
                StationaryGS.Add(station);
                station.CriticalFuelLevelReached += OnCriticalFuelLevelReached;
                station.ScheduleRefillIntervalPassed += OnScheduleRefillIntervalPassed;
            }

            for (var i = 0; i < 16; i++)
            {
                var avFuel = new Dictionary<Fuel, FuelContainer>
                {
                    { FuelTypes[0], new FuelContainer(16000) },//92
                    { FuelTypes[1], new FuelContainer(15000) } //95
                };
                var station = new GasStation(StationType.Mini, avFuel);
                MiniGS.Add(station);
                station.CriticalFuelLevelReached += OnCriticalFuelLevelReached;
                station.ScheduleRefillIntervalPassed += OnScheduleRefillIntervalPassed;
            }
        }

        private static void OnCriticalFuelLevelReached(GasStation station, Fuel criticalLevelFuel)
        {
            if (station.IsRequireGasolineTanker)
            {
                foreach (var fuel in station.GetFuelToRefillList())
                {
                    TotalOrdersInCurrentTick.Add(new OrderedFuel(station, fuel));
                }
                station.ConfirmGasolineOrder();
            }
        }

        private static void OnScheduleRefillIntervalPassed(GasStation station)
        {
            
            if (station.IsRequireGasolineTanker)
            {
                foreach (var fuel in station.GetFuelToRefillList())
                {
                    TotalOrdersInCurrentTick.Add(new OrderedFuel(station, fuel));
                }
                station.ConfirmGasolineOrder();
            }
        }

        public static void OrderGasolineTankers(List<OrderedFuel> orderedFuel)
        {
            var leftOrderedFuel = orderedFuel.Count;
            foreach (var fuel in orderedFuel)
            {
                var freeGasTanker = FreeGasolineTankers
                    .Where(t => fuel.OwnerStation.StationType == StationType.Stationary || t.TanksCount < 3) //Мини АЗС не обслуживают 3х+ секционные бензовозы
                    .FirstOrDefault();
                if (freeGasTanker == null) //Нет подходящих бензовозов
                {
                    var tanksCount = leftOrderedFuel;
                    if (fuel.OwnerStation.StationType == StationType.Stationary)
                    {
                        tanksCount = Math.Min(tanksCount, 3);
                        if (tanksCount < 2)
                            tanksCount = 2;
                    }
                    else if (fuel.OwnerStation.StationType == StationType.Mini)
                        tanksCount = 2;
                    leftOrderedFuel -= tanksCount;
                    GasolineTankers.Add(new GasolineTanker(tanksCount));
                }
                FreeGasolineTankers.First().OrderFuel(fuel.OwnerStation, fuel.FuelType, out var success);
                //
                //if (fuel.OwnerStation.StationType == StationType.Mini)
                //{
                //    var requiredTanker = FreeGasolineTankers.Where(t => t.TanksCount < 3).FirstOrDefault();
                //    if (requiredTanker == null)
                //        GasolineTankers.Add(new GasolineTanker(2));
                //    leftOrderedFuel -= 2;
                //}
                //else if (fuel.OwnerStation.StationType == StationType.Stationary)
                //{
                //    var tanksCount = leftOrderedFuel;
                //    tanksCount = Math.Min(tanksCount, 3);
                //    if (tanksCount < 2)
                //        tanksCount = 2;
                //    var requiredTanker = FreeGasolineTankers.Where(t => t.TanksCount == tanksCount).FirstOrDefault();
                //    if (requiredTanker == null)
                //        GasolineTankers.Add(new GasolineTanker(2));
                //}
            }
        }

        public static void HandleOneTick()
        {
            foreach (var station in StationaryGS.Concat(MiniGS))
            {
                if (station.CurrentCarOrder == null)
                {
                    var newOrder = new CarClientOrder();
                    station.AddOrderInQueue(newOrder);
                }
                if (station.CurrentTruckOrder == null)
                {
                    var newOrder = new TruckClientOrder();
                    station.AddOrderInQueue(newOrder);
                }
                station.WaitOneTick();
            }
            OrderGasolineTankers(TotalOrdersInCurrentTick);
            TotalOrdersInCurrentTick.Clear();
            foreach (var gasTanker in GasolineTankers)
            {
                gasTanker.WaitOneTick();
                if (!gasTanker.IsBusy && gasTanker.LoadedFuel.Count > 0)
                    gasTanker.StartDelivery();
            }
        }

        public static void RunSimulation(int simulationTimeInTicks)
        {
            Initialize();
            var stations = StationaryGS.Concat(MiniGS).ToArray();
            var stationIDs = Enumerable.Range(1, stations.Count());
            for (var i = 1; i <= simulationTimeInTicks; i++)
            {
                HandleOneTick();
                if ((i) % (24 * 60) == 0 && i != 0)//Отчет между сутками
                {
                    ReportMaker.WriteDayTitle(i);
                    ReportMaker.GSDetailedReport(stationIDs); 
                    Console.WriteLine();
                    ReportMaker.GSClientsRevenueReport();
                    ReportMaker.ClientOrdersAverageIntervalReport(); 
                    ReportMaker.TotalGasTankersReport();
                    Console.WriteLine("\n\tНажмите Enter, чтобы продолжить");
                    var input = Console.ReadLine();
                    if (input == "-debug stations")
                    {
                        Console.Write("Station IDs to debug: ");
                        var idsInput = Console.ReadLine();
                        if (idsInput.ToLower() == "all")
                            stationIDs = Enumerable.Range(1, stations.Count());
                        else
                        {
                            var ids = idsInput.Split().Select(s => int.Parse(s));
                            stationIDs = ids;
                        }
                    }
                }
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n Симуляция окончена.");
        }
    }
}
