using ISMDemo.Utilities;
using System.Globalization;
using ISMDemo.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;


namespace ISMDemo.Utilities
{
    class EdgeTwinInfo
    {
        public bool ChuteSideIsDebug { get; set; } = false;

        public bool ChuteSideIsDemo { get; set; } = false;

        public string ISMUserActivityBlobFileName { get; set; }

        public string ISMAccessCode { get; set; }

        public int ISMExpiredDuration { get; set; }

        public int ISMExpiredCheckingInterval { get; set; }

        public string ChuteSideSupportLine { get; set; }

        public int ISMEIDReaderExpiredDuration { get; set; }
    }

    public class Configuration
    {
        #region Public Properties
        public const string APP_NAME = "ChuteSideISMWebApp";
        private static readonly string clsFullName = typeof(Configuration).FullName ?? typeof(Configuration).Name;
        private static DateTime? dateLastLoadTwin = null;
        private static EdgeTwinInfo? twinInfo = null;

        public static readonly string DeviceID = Environment.GetEnvironmentVariable("IOTEDGE_DEVICEID") ?? string.Empty;
        public static readonly string ModuleName = Environment.GetEnvironmentVariable("IOTEDGE_MODULEID") ?? string.Empty;
        public static readonly string MQTTUser = Environment.GetEnvironmentVariable("IOTEDGE_MQTT_USER") ?? string.Empty;
        public static readonly string MQTTPassword = Environment.GetEnvironmentVariable("IOTEDGE_MQTT_PASSWORD") ?? string.Empty;
        public static readonly string DESSecurityKey = Environment.GetEnvironmentVariable("CHUTE_SIDE_APP_DES_SECURITYKEY") ?? string.Empty;
        public static readonly string[] SupportedCultures = BaseHelper.Split(Environment.GetEnvironmentVariable("CHUTE_SIDE_APP_SUPPORTED_CULTURES") ?? "en-us");
        public static readonly IList<CultureInfo> SupportedCultureList = GetSupportedCultureList();
        public static readonly RuntimeFileInfo runtimeFileInfo = new(Environment.GetEnvironmentVariable("IOTEDGE_RUNTIME_FILE_PATH") ?? string.Empty);
        public static readonly string VisionConfigFilePath = Environment.GetEnvironmentVariable("VISION_CONFIG_FILE_PATH") ?? string.Empty;
        public static readonly string ISMVirtualPath = Environment.GetEnvironmentVariable("ISM_VIRTUAL_PATH") ?? string.Empty;
        public static readonly LogHelper LogHelper = new LogHelper(APP_NAME, runtimeFileInfo.AppLogPath, runtimeFileInfo.ExceptionPath);
        public static string LastMQTTMsg = string.Empty;
        /// <summary>
        /// Loads the twin information.
        /// </summary>
        private static void LoadTwinInfo()
        {
            try
            {
                var twinJson = runtimeFileInfo.GetTwinJson(ref dateLastLoadTwin);
                if (string.IsNullOrEmpty(twinJson) == false)
                {
                    twinInfo = BaseHelper.DeserializeObject<EdgeTwinInfo>(twinJson);
                }

                if (twinInfo == null)
                    throw new Exception("Load Twin infomation failed");
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Gets the is debug.
        /// </summary>
        /// <value>
        /// The is debug.
        /// </value>
        public static bool IsDebug
        {
            get
            {
                LoadTwinInfo();
                return twinInfo?.ChuteSideIsDebug ?? false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is demo.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is demo; otherwise, <c>false</c>.
        /// </value>
        public static bool IsDemo
        {
            get
            {
                LoadTwinInfo();
                return twinInfo?.ChuteSideIsDemo ?? false;
            }
        }

        /// <summary>
        /// Gets the supported culture list.
        /// </summary>
        /// <returns></returns>
        private static IList<CultureInfo> GetSupportedCultureList()
        {
            try
            {
                var cultures = new List<CultureInfo>();
                foreach (string culture in SupportedCultures)
                {
                    cultures.Add(new CultureInfo(culture));
                }
                return cultures;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName,ex);
            }
        }

        public static string UserActivityBlobFilePath
        {
            get
            {
                LoadTwinInfo();
                var blobName = twinInfo?.ISMUserActivityBlobFileName ?? "ismuseractivitydata;#{guid}.json";
                blobName = blobName.Replace("{guid}", Guid.NewGuid().ToString());
                return Path.Combine(runtimeFileInfo.BlobPath, blobName);
            }
        }

        public static string ISMAccessCode
        {
            get
            {
                LoadTwinInfo();
                return twinInfo?.ISMAccessCode ?? string.Empty;
            }
        }

        /// <summary>Gets the duration of the expired.</summary>
        /// <value>The duration of the expired.</value>
        public static int ExpiredDuration
        {
            get
            {
                LoadTwinInfo();
                return twinInfo?.ISMExpiredDuration ?? 2;
            }
        }

        /// <summary>Gets the expired checking interval.</summary>
        /// <value>The expired checking interval.</value>
        public static int ExpiredCheckingInterval
        {
            get
            {
                LoadTwinInfo();
                return twinInfo?.ISMExpiredCheckingInterval ?? 30;
            }
        }

        /// <summary>
        /// Gets the support line.
        /// </summary>
        /// <value>
        /// The support line.
        /// </value>
        public static string SupportLine
        {
            get
            {
                LoadTwinInfo();
                return twinInfo?.ChuteSideSupportLine ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets the duration of the eid reader expired.
        /// </summary>
        /// <value>
        /// The duration of the eid reader expired.
        /// </value>
        public static int EIDReaderExpiredDuration
        {
            get
            {
                LoadTwinInfo();
                return twinInfo?.ISMEIDReaderExpiredDuration ?? 10;
            }
        }
        #endregion
    }
}