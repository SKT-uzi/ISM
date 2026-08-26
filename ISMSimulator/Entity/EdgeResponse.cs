using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISMSimulator.Entity
{
    public class EdgeResponse<T>
    {
        public int ResponseCode { get; set; }
        public T  Response { get; set; }
    }
}
