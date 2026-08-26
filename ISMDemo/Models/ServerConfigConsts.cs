using ISMDemo.Utilities;

namespace ISMDemo.Models
{
    public class ServerConfigConsts
    {
        public ServerConfigConsts()
        {
            this.IsDebug = Configuration.IsDebug;
            this.IsDemo = Configuration.IsDemo;
            this.ExpiredCheckingInterval = Configuration.ExpiredCheckingInterval;
            this.ExpiredDuration = Configuration.ExpiredDuration;
            this.EIDReaderExpiredDuration = Configuration.EIDReaderExpiredDuration;
        }

        public bool IsDebug { get; set; }

        public bool IsDemo { get; set; }

        public string SupportLine { get; set; }

        public string SupportEmail { get; set; }

        public int ExpiredCheckingInterval { get; set; }

        public int ExpiredDuration { get; set; }

        public int EIDReaderExpiredDuration { get; set; }
    }
}
