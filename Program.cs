using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasStations
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.SetWindowSize(220, 60);
            GasStationSystem.RunSimulation(24 * 60 * 10);
            while (true)
                Console.ReadLine();
        }
    }
}
