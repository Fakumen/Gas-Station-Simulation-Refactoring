using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasStations
{
    public interface IWaiter
    {
        //bool IsBusy { get; }
        void WaitOneTick();
    }
}
