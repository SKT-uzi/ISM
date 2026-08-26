using ISMDemo.Utilities;

namespace ISMDemo.Models
{
    public class RuntimeFileInfo
    {
        private static readonly string clsFullName = typeof(RuntimeFileInfo).FullName ?? typeof(RuntimeFileInfo).Name;

        public string RootPath { get; set; }

        public string BlobPath { get; set; }

        public string InputPath { get; set; }

        public string OutputPath { get; set; }

        public string AppLogPath { get; set; }

        public string ExceptionPath { get; set; }

        public string DataTransferStatusPath { get; set; }

        public string DBPath { get; set; }

        public string TwinPath { get; set; }

        public RuntimeFileInfo(string rootPath)
        {
            RootPath = rootPath;
            BlobPath = Path.Combine(rootPath, "blob");
            InputPath = Path.Combine(rootPath, "input");
            OutputPath = Path.Combine(rootPath, "output");
            AppLogPath = Path.Combine(rootPath, "applogs");
            ExceptionPath = Path.Combine(rootPath, "exception");
            DataTransferStatusPath = Path.Combine(rootPath, "datatransferstatus");
            DBPath = Path.Combine(rootPath, "db");
            TwinPath = Path.Combine(DBPath, "twin.json");
        }

        public void InitFolder()
        {
            try
            {
                if (!Directory.Exists(RootPath))
                {
                    Directory.CreateDirectory(RootPath);
                }

                if (!Directory.Exists(BlobPath))
                {
                    Directory.CreateDirectory(BlobPath);
                }

                if (!Directory.Exists(InputPath))
                {
                    Directory.CreateDirectory(InputPath);
                }

                if (!Directory.Exists(OutputPath))
                {
                    Directory.CreateDirectory(OutputPath);
                }

                if (!Directory.Exists(AppLogPath))
                {
                    Directory.CreateDirectory(AppLogPath);
                }

                if (!Directory.Exists(ExceptionPath))
                {
                    Directory.CreateDirectory(ExceptionPath);
                }

                if (!Directory.Exists(DataTransferStatusPath))
                {
                    Directory.CreateDirectory(DataTransferStatusPath);
                }

                if (!Directory.Exists(DBPath))
                {
                    Directory.CreateDirectory(DBPath);
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex, "", "InitFolder");
            }
        }

        public string GetTwinJson(ref DateTime? datelastLoadTwin)
        {
            try
            {
                if (!File.Exists(TwinPath))
                {
                    return string.Empty;
                }

                DateTime lastWriteTime = File.GetLastWriteTime(TwinPath);
                if (datelastLoadTwin.HasValue)
                {
                    DateTime value = lastWriteTime;
                    DateTime? dateTime = datelastLoadTwin;
                    if (value <= dateTime)
                    {
                        return string.Empty;
                    }
                }

                datelastLoadTwin = lastWriteTime;
                return File.ReadAllText(TwinPath);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex, "", "GetTwinJson");
            }
        }

        public string GetDataTransferStatusFilePath(string dataType, string fileName)
        {
            try
            {
                BaseHelper.SplitFileName(fileName, out string name, out string ext);
                return Path.Combine(DataTransferStatusPath, $"{dataType}_{name}_DataTransferStatus{ext}");
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex, "", "GetDataTransferStatusFilePath");
            }
        }

        public string GetDataTransferEdgeStatusFilePath(string dataType, string fileName)
        {
            try
            {
                BaseHelper.SplitFileName(fileName, out string name, out string ext);
                return Path.Combine(DataTransferStatusPath, $"{dataType}_{name}_DataTransferEdgeStatus{ext}");
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex, "", "GetDataTransferEdgeStatusFilePath");
            }
        }

        public string[] GetUploadBlobFileList()
        {
            try
            {
                return Directory.GetFiles(BlobPath);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex, "", "GetUploadBlobFileList");
            }
        }

        public string GetEscapeUploadBlobFilePath(string blobName)
        {
            try
            {
                return Path.Combine(BlobPath, blobName.Replace("/", ";#"));
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex, "", "GetEscapeUploadBlobFilePath");
            }
        }

        public string GetDescapeUploadBlobName(string fileName)
        {
            try
            {
                return fileName.Replace(";#", "/");
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex, "", "GetDescapeUploadBlobName");
            }
        }

        public string GetTelemetryFileDataType(string fileName)
        {
            try
            {
                return fileName.Split('_')[1];
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex, "", "GetTelemetryFileDataType");
            }
        }

        public string[] GetOutputTelemetryFileList(string dataType = "")
        {
            try
            {
                return (from ss in Directory.GetFiles(OutputPath, "telemetry_" + dataType.ToLower() + "*.json")
                        orderby new FileInfo(ss).CreationTime
                        select ss).ToArray();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex, "", "GetOutputTelemetryFileList");
            }
        }

        public string GetOutputTelemetryFilePath(string dataType)
        {
            try
            {
                string value = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                string value2 = Guid.NewGuid().ToString();
                return Path.Combine(OutputPath, $"telemetry_{dataType.ToLower()}_{value}_{value2}.json");
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex, "", "GetOutputTelemetryFilePath");
            }
        }

        public string[] GetInputTelemetryFileList(string dataType = "")
        {
            try
            {
                return (from ss in Directory.GetFiles(InputPath, "telemetry_" + dataType.ToLower() + "*.json")
                        orderby new FileInfo(ss).CreationTime
                        select ss).ToArray();
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex, "", "GetInputTelemetryFileList");
            }
        }

        public string GetInputTelemetryFilePath(string dataType)
        {
            try
            {
                string value = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                string value2 = Guid.NewGuid().ToString();
                return Path.Combine(InputPath, $"telemetry_{dataType.ToLower()}_{value}_{value2}.json");
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex, "", "GetInputTelemetryFilePath");
            }
        }

        public string GetCallbackMethodName(string dataType)
        {
            return "Callback" + dataType;
        }

        public string GetCallbackDataType(string callBackName)
        {
            return callBackName.Replace("Callback", string.Empty);
        }
    }
}
