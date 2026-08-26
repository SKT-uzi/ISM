using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISMSimulator.Entity
{
    public class EdgeRequest<T>
    {
        [JsonProperty("request")]
        public T Request { get; set; }

        public EdgeRequest(T request)
        {
            this.Request = request;
        }

    }
}
