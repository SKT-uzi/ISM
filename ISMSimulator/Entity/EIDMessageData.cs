using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISMSimulator.Entity
{
    public class EIDMessageData
    {
        public string Type { get; set; }
        public EIDValue Value { get; set; }
    }

    public class EIDValue {
        public string Status { get; set; }
        public string Reader { get; set; }
        public string ErrorMessage { get; set; }
    }
}
