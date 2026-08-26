using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISMSimulator.Entity
{
    public class SimulatorScale
    {
        public string Type { get; set; }
        public ScaleInfo Value { get; set; }
    }

    public class ScaleInfo
    {
        public string Status { get; set; } = string.Empty;
        public int Weight { get; set; }
        public string Rawserial { get; set; }
        public decimal Rate_HZ { get; set; }
        public string Port { get; set; }
        public int Baud { get; set; }
        public string Parity { get; set; }
        public int Stop { get; set; }
        public int Bits { get; set; }
        public string ErrorMessage { get; set; }
    }
}
