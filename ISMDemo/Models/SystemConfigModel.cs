using Newtonsoft.Json;

namespace ISMDemo.Models
{
    public class SystemConfigModel
    {
        public bool IsExist { get; set; }

        public EthernetConfig? EthernetConfig { get; set; }

        public WirelessConfig? WirelessConfig { get; set; }
    }

    public class WirelessConfig
    {
        [JsonProperty(propertyName: "ssid")]
        public string? SSID { get; set; }

        [JsonProperty(propertyName: "securitykey")]
        public string? SecurityKey { get; set; }

        [JsonProperty(propertyName: "securitytype")]
        public string? SecurityType { get; set; }
    }

    public class EthernetConfig
    {
        [JsonProperty(propertyName: "dhcp")]
        public bool DHCP { get; set; }

        [JsonProperty(propertyName: "ip")]
        public string? IP { get; set; }

        [JsonProperty(propertyName: "subnetmask")]
        public string? SubnetMask { get; set; }

        [JsonProperty(propertyName: "gateway")]
        public string? Gateway { get; set; }
    }
}
