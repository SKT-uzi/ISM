using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace ISMDemo.Utilities
{
    public class CheckLogFrequency
    {
        public DateTime CheckDate { get; set; }

        public bool CheckResult { get; set; }
    }
    public class LogHelper
    {
        private static object lockObject = new object();

        private readonly ConcurrentDictionary<string, CheckLogFrequency> DicLogFrequecy = new ConcurrentDictionary<string, CheckLogFrequency>();

        private string AppName { get; set; }

        private string AppLogPath { get; set; }

        private string ExceptionPath { get; set; }

        public LogHelper(string appName, string appLogPath, string exceptionPath)
        {
            AppName = appName;
            AppLogPath = appLogPath;
            ExceptionPath = exceptionPath;
        }

        public void CheckFrequency(string frequencyKey, int frequencyMinute = 1)
        {
            try
            {
                DicLogFrequecy.TryGetValue(frequencyKey, out CheckLogFrequency value);
                if (value == null)
                {
                    value = new CheckLogFrequency
                    {
                        CheckDate = DateTime.Now.AddMinutes(frequencyMinute),
                        CheckResult = true
                    };
                    DicLogFrequecy.TryAdd(frequencyKey, value);
                }
                else if (value.CheckDate < DateTime.Now)
                {
                    value.CheckResult = true;
                    value.CheckDate = DateTime.Now.AddMinutes(frequencyMinute);
                }
                else
                {
                    value.CheckResult = false;
                }
            }
            catch
            {
            }
        }

        public void WriteLog(string message, string frequencyKey = "")
        {
            try
            {
                if (!string.IsNullOrEmpty(frequencyKey))
                {
                    DicLogFrequecy.TryGetValue(frequencyKey, out CheckLogFrequency value);
                    if (value != null && !value.CheckResult)
                    {
                        return;
                    }
                }

                string filePath = GetFilePath(LogLevel.Information);
                string formatContent = GetFormatContent(message);
                Console.WriteLine($"[{AppName}]{LogLevel.Information.ToString()}, {formatContent}");
                AppendText(filePath, formatContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine("WriteLog,ex:" + ex.Message);
            }
        }

        public void WriteException(string className, Exception ex, string customMsg = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
        {
            try
            {
                string filePath = GetFilePath(LogLevel.Error);
                string exMessageBody = BaseHelper.GetExMessageBody(ex);
                string formatContent = GetFormatContent($"{className}.{memberName}:{exMessageBody}");
                AppendText(filePath, formatContent);
                string filePath2 = GetFilePath(LogLevel.Information);
                AppendText(filePath2, formatContent);
                if (!string.IsNullOrEmpty(customMsg))
                {
                    AppendText(filePath, "customMsg:" + customMsg);
                    AppendText(filePath2, "customMsg:" + customMsg);
                }
            }
            catch (Exception ex2)
            {
                Console.WriteLine("WriteException Ex:" + ex2.Message);
            }
        }

        private string GetFilePath(LogLevel level)
        {
            try
            {
                string text = DateTime.Now.ToString("yyyy-MM-dd");
                string text2 = level.ToString();
                string text3 = Path.Combine(AppLogPath, AppName.ToLower());
                if (!Directory.Exists(text3))
                {
                    Directory.CreateDirectory(text3);
                }

                return Path.Combine(text3, text + "_" + text2 + ".txt");
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(GetType().FullName, ex, "", "GetFilePath");
            }
        }

        private string GetFormatContent(string content)
        {
            try
            {
                return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + content + "\r\n";
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(GetType().FullName, ex, "", "GetFormatContent");
            }
        }

        private void AppendText(string filePath, string content)
        {
            try
            {
                lock (lockObject)
                {
                    File.AppendAllText(filePath, content);
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(GetType().FullName, ex, "", "AppendText");
            }
        }
    }
}
