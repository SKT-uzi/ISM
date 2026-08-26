using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISMSimulator.Entity
{
    public class EdgeDeviceInfo
    {
        public string ID { get; set; }
        public string DisplayName { get; set; }
        public bool Simulated { get; set; }
        public bool Provisioned { get; set; }
        public string Etag { get; set; }
        public string Template { get; set; }
        public bool Enabled { get; set; }
    }

    public class GetDeviceInfoResult
    {
        public List<EdgeDeviceInfo> Value { get; set; }
    }
}
