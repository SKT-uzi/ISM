using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISMSimulator.Entity
{
    public class SimulatorMessage
    {
        public string Method { get; set; }
        public string Route { get; set; }
        public string Data { get; set; }
    }
}