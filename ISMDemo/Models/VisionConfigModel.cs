using System.Text.Json.Serialization;

namespace ISMDemo.Models
{
    public class VisionConfigModel
    {
        public string Status { get; set; } = "NotExist";    //VisionConfigStatus  0:NotExist; 1:Initialzing; 2:Completed

        public string Password { get; set; } = string.Empty;

        public string LocationName { get; set; } = string.Empty;

        public VisionConfigNetworkModel Network { get; set; } = new VisionConfigNetworkModel();

        public VisionConfigCameraModel Camera { get; set; } = new VisionConfigCameraModel();

        public VisionConfigScaleModel Scale { get; set; } = new VisionConfigScaleModel();

        public ISMConfigEIDModel EID { get; set; } = new ISMConfigEIDModel();

        public int[] BaudRateList { get; set; } = Const.BAUD_RATE_LIST;
    }

    public class VisionConfigNetworkModel
    {
        public VisionConfigNetworkEthernetModel Ethernet { get; set; } = new VisionConfigNetworkEthernetModel();

        public VisionConfigNetworkWirelessModel Wireless { get; set; } = new VisionConfigNetworkWirelessModel();
    }

    public class VisionConfigNetworkEthernetModel
    {
        public string IPAssignmentMode { get; set; } = "auto";    //auto or manual

        public string IPAddress { get; set; } = string.Empty;

        public string SubnetMask { get; set; } = string.Empty;

        public string Gateway { get; set; } = string.Empty;
    }

    public class VisionConfigNetworkWirelessModel
    {
        public string SSID { get; set; } = string.Empty;        
        
        public string SecurityType { get; set; } = string.Empty;

        public string SecurityKey { get; set; } = string.Empty;

        public bool Secured { get; set; } = false;
    }
    
    public class VisionConfigCameraModel
    {
        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedDate { get; set; } = null;
    }

    public class VisionConfigScaleModel
    {
        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedDate { get; set; } = null;
    }

    public class ISMConfigEIDModel
    {
        public LFEIDModel LF { get; set; } = new LFEIDModel();

        public UHFEIDModel UHF { get; set; } = new UHFEIDModel();
    }
    public class LFEIDModel
    {
        public bool? IsPaired { get; set; } = null;
        public string BluetoothDeviceName { get; set; } = string.Empty;
        public string BluetoothMacAddress { get; set; } = string.Empty;

        public bool IsCompleted { get; set; } = false;

        public int BaudRate { get; set; } = 9600;

        public DateTime? CompletedDate { get; set; } = null;
    }

    public class UHFEIDModel
    {
        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedDate { get; set; } = null;
    }

    public class StepStatus
    {
        public bool CameraCompleted { get; set; } = false;

        public bool ScaleCompleted { get; set; } = false;

        public bool EIDCompleted { get; set; } = false;
    }
}