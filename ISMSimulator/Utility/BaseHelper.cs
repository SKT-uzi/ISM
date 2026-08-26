using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;


namespace ISMSimulator.Utility
{
    public class BaseHelper
    {
        private static readonly string clsFullName = typeof(BaseHelper).FullName ?? typeof(BaseHelper).Name;


        /// <summary>
        /// Equalses the ignore case.
        /// </summary>
        /// <param name="value1">The value1.</param>
        /// <param name="value2">The value2.</param>
        /// <returns></returns>
        public static bool EqualsIgnoreCase(string value1, string value2)
        {
            try
            {
                if (value1 == null)
                    value1 = string.Empty;

                if (value2 == null)
                    value2 = string.Empty;

                return value1.Trim().Equals(value2.Trim(), StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex);
            }
        }

        /// <summary>
        /// Contains the value.
        /// </summary>
        /// <param name="valueList">The value list.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static bool ContainIgnoreCase(List<string> valueList, string value)
        {
            try
            {
                if (value == null)
                    return false;

                foreach (var item in valueList)
                {
                    if (item.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex);
            }
        }

        /// <summary>
        /// Contains the value.
        /// </summary>
        /// <param name="valueList">The value list.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static bool ContainIgnoreCase(string[] valueList, string value)
        {
            try
            {
                if (value == null)
                    return false;

                foreach (var item in valueList)
                {
                    if (item.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex);
            }
        }

        /// <summary>
        /// Indexes the of ignore case.
        /// </summary>
        /// <param name="valueList">The value list.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static int IndexOfIgnoreCase(List<string> valueList, string value)
        {
            try
            {
                if (value == null)
                    return -1;

                for (var i = 0; i < valueList.Count; i++)
                {
                    var item = valueList[i];
                    if (item.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase))
                        return i;
                }
                return -1;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex);
            }
        }

        /// <summary>
        /// Indexes the of ignore case.
        /// </summary>
        /// <param name="valueList">The value list.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static int IndexOfIgnoreCase(string[] valueList, string value)
        {
            try
            {
                if (value == null)
                    return -1;

                for (var i = 0; i < valueList.Length; i++)
                {
                    var item = valueList[i];
                    if (item.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase))
                        return i;
                }
                return -1;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex);
            }
        }

        /// <summary>
        /// Splits the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="split">The split.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static string[] Split(string value, string split = ",", StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                    return new string[] { };

                return value.Split(new string[] { split }, options);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex);
            }
        }

        /// <summary>
        /// Joins the specified list.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="seperator">The seperator.</param>
        /// <returns></returns>
        public static string Join(List<int> list, string seperator)
        {
            try
            {
                if (list == null)
                    return string.Empty;

                if (list.Count == 0)
                    return string.Empty;

                return string.Join(seperator, list);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex);
            }
        }

        /// <summary>
        /// Joins the specified list.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="seperator">The seperator.</param>
        /// <returns></returns>
        public static string Join(List<string> list, string seperator)
        {
            try
            {
                if (list == null)
                    return string.Empty;

                if (list.Count == 0)
                    return string.Empty;

                return string.Join(seperator, list);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex);
            }
        }

        /// <summary>
        /// Splits the name of the file. 
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="name">The name.</param>
        /// <param name="ext">The ext.</param>
        public static void SplitFileName(string fileName, out string name, out string ext)
        {
            try
            {
                var index = fileName.LastIndexOf(".");
                if (index > 0)
                {
                    ext = fileName.Substring(index);
                    name = fileName.Substring(0, index);
                }
                else if (index < 0)
                {
                    //none ext
                    name = fileName;
                    ext = string.Empty;
                }
                else
                {
                    //none name
                    name = string.Empty;
                    ext = fileName;
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex);
            }
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <param name="fileFullName">Full name of the file.</param>
        /// <returns></returns>
        public static string GetFileName(string fileFullName)
        {
            try
            {
                var fileName = fileFullName;
                var index = fileName.LastIndexOf("/");
                if (index > 0)
                    fileName = fileName.Substring(index + 1);

                index = fileName.LastIndexOf("\\");
                if (index > 0)
                    fileName = fileName.Substring(index + 1);

                return fileName;
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex);
            }
        }

        /// <summary>
        /// Gets the ex message body.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="customMsg">The custom MSG.</param>
        /// <returns></returns>
        public static string GetExMessageBody(Exception ex)
        {
            try
            {
                var sbMsg = new StringBuilder();
                sbMsg.Append(ex.Message);
                var tempEx = ex.InnerException;
                var deep = 1;
                while (tempEx != null)
                {
                    sbMsg.AppendLine();
                    sbMsg.Append(tempEx.Message);
                    tempEx = tempEx.InnerException;
                    if (deep++ > 3)
                        break;
                }

                return sbMsg.ToString();
            }
            catch (Exception ex1)
            {
                throw BaseHelper.CreateException(clsFullName, ex1);
            }
        }

        /// <summary>
        /// Creates the exception.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="ex">The ex.</param>
        /// <param name="customMsg">The custom MSG.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <returns></returns>
        public static Exception CreateException(string className, Exception ex, string customMsg = "", [CallerMemberName] string memberName = "")
        {
            try
            {
                var exBody = BaseHelper.GetExMessageBody(ex);
                if (string.IsNullOrEmpty(customMsg) == false)
                    return new Exception($"{className}.{memberName} Exception:{exBody} \r\n CustomMsg:{customMsg}");

                return new Exception($"{className}.{memberName} Exception:{exBody}");
            }
            catch (Exception ex1)
            {
                throw BaseHelper.CreateException(clsFullName, ex1);
            }
        }

        /// <summary>
        /// Converts the unix time stamp.
        /// </summary>
        /// <param name="unixTimeStamp>The unix time stamp.</param>
        /// <returns></returns>
        public static DateTime FromUnixTimeStamp(double unixTimeStamp)
        {
            try
            {
                var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                return dtDateTime.AddSeconds(unixTimeStamp);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex);
            }
        }

        /// <summary>
        /// Converts to unixtimestamp.
        /// </summary>
        /// <param name="dtDateTime">The dt date time.</param>
        /// <returns></returns>
        public static double ToUnixTimeStamp(DateTime dtDateTime)
        {
            try
            {
                var date = dtDateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1));
                return Math.Round(date.TotalSeconds, 1);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex);
            }
        }

        /// <summary>
        /// Checks the network avaraible.
        /// </summary>
        /// <param name="hostName">Name of the host.</param>
        /// <returns></returns>
        public static bool CheckNetworkAvailable(string hostName)
        {
            try
            {
                var ping = new Ping();
                var reply = ping.Send(hostName, 3000);
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Deserializes the object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="isCamelCase">if set to <c>true</c> [is camel case].</param>
        /// <returns></returns>
        public static T DeserializeObject<T>(string value, bool isCamelCase = false)
        {
            try
            {
                var setting = new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    Formatting = Formatting.Indented
                };

                if (isCamelCase)
                    setting.ContractResolver = new CamelCasePropertyNamesContractResolver();

                return JsonConvert.DeserializeObject<T>(value, setting);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex);
            }
        }

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="isCamelCase">if set to <c>true</c> [is camel case].</param>
        /// <returns></returns>
        public static string SerializeObject(object obj, bool isCamelCase = false)
        {
            try
            {
                var setting = new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    Formatting = Formatting.Indented
                };

                if (isCamelCase)
                    setting.ContractResolver = new CamelCasePropertyNamesContractResolver();
                return JsonConvert.SerializeObject(obj, setting);
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex);
            }
        }


        /// <summary>
        /// Decrypts the specified encrypted string.
        /// </summary>
        /// <param name="encryptedString">The encrypted string.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string Decrypt(string encryptedString, string key)
        {
            try
            {
                if (string.IsNullOrEmpty(encryptedString) || string.IsNullOrEmpty(key))
                    return string.Empty;

                var inputBytes = Convert.FromBase64String(encryptedString);
                var hashmd5 = new MD5CryptoServiceProvider();
                var pwdhash = hashmd5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(key));

                // Create a new TripleDES service provider 
                var tdesProvider = new TripleDESCryptoServiceProvider();
                tdesProvider.Key = pwdhash;
                tdesProvider.Mode = CipherMode.ECB;
                return ASCIIEncoding.ASCII.GetString(tdesProvider.CreateDecryptor().TransformFinalBlock(inputBytes, 0, inputBytes.Length));
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex);
            }
        }

        /// <summary>
        /// Encrypts the specified plain text.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string Encrypt(string plainText, string key)
        {
            try
            {
                if (string.IsNullOrEmpty(plainText))
                    return string.Empty;

                var inputBytes = ASCIIEncoding.ASCII.GetBytes(plainText);
                var hashmd5 = new MD5CryptoServiceProvider();
                var pwdhash = hashmd5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(key));

                // Create a new TripleDES service provider 
                var tdesProvider = new TripleDESCryptoServiceProvider();
                tdesProvider.Key = pwdhash;
                tdesProvider.Mode = CipherMode.ECB;
                return Convert.ToBase64String(tdesProvider.CreateEncryptor().TransformFinalBlock(inputBytes, 0, inputBytes.Length));
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(clsFullName, ex);
            }
        }

        /// <summary>
        /// Save Json File
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="jObject"></param>
        public static void SaveJsonFile(string filepath, JObject jObject)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filepath))
            {
                file.Write(jObject.ToString());
            }
        }

        /// <summary>
        /// Remove Special Charactor
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static string RemoveSpecialCharactor(string text)
        {
            return text.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("}{", "},{");
        }

        /// <summary>
        /// ConvertJsonString
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static  string ConvertJsonString(string json)
        {
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(json);
            JsonTextReader jtr = new JsonTextReader(tr);
            if (jtr.Value != null && string.IsNullOrEmpty(jtr.Value.ToString()) == false)
            {
                object obj = serializer.Deserialize(jtr);
                if (obj != null)
                {
                    StringWriter textWriter = new StringWriter();
                    JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                    {
                        Formatting = Formatting.Indented,
                        Indentation = 2,
                        IndentChar = ' '
                    };
                    serializer.Serialize(jsonWriter, obj);
                    return textWriter.ToString();
                }
                else
                {
                    return json;
                }
            }
            else {
                return json;
            }
        }
    }
}
