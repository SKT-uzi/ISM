using ISMDemo.Models;
using ISMDemo.Utilities;
using Newtonsoft.Json;

namespace ISMDemo.Business
{
    public class ConfigFileManager
    {
        // static ReaderWriterLockSlim LogWriteLock = new ReaderWriterLockSlim();

        public static VisionConfigModel? ReadFile()
        {
            var configModel = new VisionConfigModel();

            try
            {
                if (File.Exists(Configuration.VisionConfigFilePath))
                {
                    var fileContent = File.ReadAllText(Configuration.VisionConfigFilePath);
                    if (!string.IsNullOrEmpty(fileContent))
                    {
                        configModel = BaseHelper.DeserializeObject<VisionConfigModel>(fileContent);
                    }
                }
            }
            catch
            {
                configModel = new VisionConfigModel();
            }

            return configModel;
        }


        public static void SaveFile(VisionConfigModel c)
        {
            var configFilePath = Configuration.VisionConfigFilePath;

            string dir = Path.GetDirectoryName(configFilePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (var sw = new StreamWriter(configFilePath, false))
            {
                Task.Run(async () =>
                {
                    await sw.WriteAsync(JsonConvert.SerializeObject(c));
                    await sw.FlushAsync();
                }).Wait();
            }
        }
    }
}