using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISMSimulator.Entity
{
    public class ResponseMessage
    {
        public string Topic { get; set; }
        public List<string> Message { get; set; } = new List<string>();
    }
}
