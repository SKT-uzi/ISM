using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ISMDemo.Utilities
{

    public class BaseHelper
    {
        private static readonly string clsFullName = typeof(BaseHelper).FullName ?? typeof(BaseHelper).Name;

        public static string ConvertStringToBase64(string str)
        {
            try
            {
                if (string.IsNullOrEmpty(str))
                {
                    return string.Empty;
                }

                return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "ConvertStringToBase64");
            }
        }

        public static string ConvertBase64ToString(string base64Str)
        {
            try
            {
                if (string.IsNullOrEmpty(base64Str))
                {
                    return string.Empty;
                }

                byte[] array = Convert.FromBase64String(base64Str);
                return Encoding.UTF8.GetString(array, 0, array.Length);
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "ConvertBase64ToString");
            }
        }

        public static bool EqualsIgnoreCase(string value1, string value2)
        {
            try
            {
                if (value1 == null)
                {
                    value1 = string.Empty;
                }

                if (value2 == null)
                {
                    value2 = string.Empty;
                }

                return value1.Trim().Equals(value2.Trim(), StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "EqualsIgnoreCase");
            }
        }

        public static bool ContainIgnoreCase(List<string> valueList, string value)
        {
            try
            {
                if (value == null)
                {
                    return false;
                }

                foreach (string value2 in valueList)
                {
                    if (value2.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "ContainIgnoreCase");
            }
        }

        public static bool ContainIgnoreCase(string[] valueList, string value)
        {
            try
            {
                if (value == null)
                {
                    return false;
                }

                for (int i = 0; i < valueList.Length; i++)
                {
                    if (valueList[i].Equals(value.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "ContainIgnoreCase");
            }
        }

        public static int IndexOfIgnoreCase(List<string> valueList, string value)
        {
            try
            {
                if (value == null)
                {
                    return -1;
                }

                for (int i = 0; i < valueList.Count; i++)
                {
                    if (valueList[i].Equals(value.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        return i;
                    }
                }

                return -1;
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "IndexOfIgnoreCase");
            }
        }

        public static int IndexOfIgnoreCase(string[] valueList, string value)
        {
            try
            {
                if (value == null)
                {
                    return -1;
                }

                for (int i = 0; i < valueList.Length; i++)
                {
                    if (valueList[i].Equals(value.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        return i;
                    }
                }

                return -1;
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "IndexOfIgnoreCase");
            }
        }

        public static string[] Split(string value, string split = ",", StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                {
                    return new string[0];
                }

                return value.Split(new string[1] { split }, options);
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "Split");
            }
        }

        public static string Join(List<int> list, string seperator)
        {
            try
            {
                if (list == null)
                {
                    return string.Empty;
                }

                if (list.Count == 0)
                {
                    return string.Empty;
                }

                return string.Join(seperator, list);
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "Join");
            }
        }

        public static string Join(List<string> list, string seperator)
        {
            try
            {
                if (list == null)
                {
                    return string.Empty;
                }

                if (list.Count == 0)
                {
                    return string.Empty;
                }

                return string.Join(seperator, list);
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "Join");
            }
        }

        public static DateTime FromUnixTimeStamp(double unixTimeStamp)
        {
            try
            {
                return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTimeStamp);
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "FromUnixTimeStamp");
            }
        }

        public static double ToUnixTimeStamp(DateTime dtDateTime)
        {
            try
            {
                return Math.Round(dtDateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds, 1);
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "ToUnixTimeStamp");
            }
        }

        public static MemoryStream GetImageStream(string imageBase64String)
        {
            try
            {
                if (!imageBase64String.StartsWith("data:image/png;base64,"))
                {
                    throw new Exception("imagedata format not data:image/png;base64,");
                }

                return new MemoryStream(Convert.FromBase64String(imageBase64String.Substring("data:image/png;base64,".Length)));
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "GetImageStream");
            }
        }

        public static string GetExcelColKey(uint colIndex)
        {
            try
            {
                uint num = colIndex / 26;
                uint num2 = colIndex % 26;
                if (num2 == 0)
                {
                    num2 = 26u;
                    if (num != 0)
                    {
                        num--;
                    }
                }

                string obj = ((num != 0) ? Convert.ToString((char)(num + 64)) : string.Empty);
                string text = Convert.ToString((char)(num2 + 64));
                return obj + text;
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "GetExcelColKey");
            }
        }

        public static uint GetExcelColIndex(string colKey)
        {
            try
            {
                char[] array = colKey.ToCharArray();
                if (array.Length == 2)
                {
                    return (uint)((array[0] - 64) * 26 + array[1] - 64);
                }

                if (array.Length == 1)
                {
                    return (uint)(array[0] - 64);
                }

                return 0u;
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "GetExcelColIndex");
            }
        }

        public static string UrlEncode(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                {
                    return string.Empty;
                }

                return HttpUtility.UrlEncode(value.Trim());
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "UrlEncode");
            }
        }

        public static string UrlDecode(string encodedValue)
        {
            try
            {
                if (string.IsNullOrEmpty(encodedValue))
                {
                    return string.Empty;
                }

                return HttpUtility.UrlDecode(encodedValue.Trim());
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "UrlDecode");
            }
        }

        public static string UrlPathEncode(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    return string.Empty;
                }

                return HttpUtility.UrlPathEncode(url);
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "UrlPathEncode");
            }
        }

        public static string HtmlEncode(string value)
        {
            try
            {
                return HttpUtility.HtmlEncode(value.Trim());
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "HtmlEncode");
            }
        }

        public static string HtmlDecode(string value)
        {
            try
            {
                return HttpUtility.HtmlDecode(value.Trim());
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "HtmlDecode");
            }
        }

        public static void SplitFileName(string fileName, out string name, out string ext)
        {
            try
            {
                int num = fileName.LastIndexOf(".");
                if (num > 0)
                {
                    ext = fileName.Substring(num);
                    name = fileName.Substring(0, num);
                }
                else if (num < 0)
                {
                    name = fileName;
                    ext = string.Empty;
                }
                else
                {
                    name = string.Empty;
                    ext = fileName;
                }
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "SplitFileName");
            }
        }

        public static string GetFileName(string fileFullName)
        {
            try
            {
                string text = fileFullName;
                int num = text.LastIndexOf("/");
                if (num > 0)
                {
                    text = text.Substring(num + 1);
                }

                num = text.LastIndexOf("\\");
                if (num > 0)
                {
                    text = text.Substring(num + 1);
                }

                return text;
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "GetFileName");
            }
        }

        public static string GetFileExtension(string fileName)
        {
            try
            {
                if (fileName.LastIndexOf(".") >= 0)
                {
                    return fileName.Substring(fileName.LastIndexOf("."));
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "GetFileExtension");
            }
        }

        public static string GetContentTypeByExtension(string extension)
        {
            return extension.ToLower() switch
            {
                ".323" => "text/h323",
                ".3g2" => "video/3gpp2",
                ".3gp2" => "video/3gpp2",
                ".3gp" => "video/3gpp",
                ".3gpp" => "video/3gpp",
                ".aac" => "audio/aac",
                ".aaf" => "application/octet-stream",
                ".aca" => "application/octet-stream",
                ".accdb" => "application/msaccess",
                ".accde" => "application/msaccess",
                ".accdt" => "application/msaccess",
                ".acx" => "application/internet-property-stream",
                ".adt" => "audio/vnd.dlna.adts",
                ".adts" => "audio/vnd.dlna.adts",
                ".afm" => "application/octet-stream",
                ".ai" => "application/postscript",
                ".aif" => "audio/x-aiff",
                ".aifc" => "audio/aiff",
                ".aiff" => "audio/aiff",
                ".appcache" => "text/cache-manifest",
                ".application" => "application/x-ms-application",
                ".art" => "image/x-jg",
                ".asd" => "application/octet-stream",
                ".asf" => "video/x-ms-asf",
                ".asi" => "application/octet-stream",
                ".asm" => "text/plain",
                ".asr" => "video/x-ms-asf",
                ".asx" => "video/x-ms-asf",
                ".atom" => "application/atom+xml",
                ".au" => "audio/basic",
                ".avi" => "video/x-msvideo",
                ".axs" => "application/olescript",
                ".bas" => "text/plain",
                ".bcpio" => "application/x-bcpio",
                ".bin" => "application/octet-stream",
                ".bmp" => "image/bmp",
                ".c" => "text/plain",
                ".cab" => "application/vnd.ms-cab-compressed",
                ".calx" => "application/vnd.ms-office.calx",
                ".cat" => "application/vnd.ms-pki.seccat",
                ".cdf" => "application/x-cdf",
                ".chm" => "application/octet-stream",
                ".class" => "application/x-java-applet",
                ".clp" => "application/x-msclip",
                ".cmx" => "image/x-cmx",
                ".cnf" => "text/plain",
                ".config" => "application/xml",
                ".cod" => "image/cis-cod",
                ".cpio" => "application/x-cpio",
                ".cpp" => "text/plain",
                ".crd" => "application/x-mscardfile",
                ".crl" => "application/pkix-crl",
                ".crt" => "application/x-x509-ca-cert",
                ".csh" => "application/x-csh",
                ".css" => "text/css",
                ".csv" => "application/octet-stream",
                ".cur" => "application/octet-stream",
                ".dcr" => "application/x-director",
                ".deploy" => "application/octet-stream",
                ".der" => "application/x-x509-ca-cert",
                ".dib" => "image/bmp",
                ".dir" => "application/x-director",
                ".disco" => "text/xml",
                ".dlm" => "text/dlm",
                ".doc" => "application/msword",
                ".docm" => "application/vnd.ms-word.document.macroEnabled.12",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".dot" => "application/msword",
                ".dotm" => "application/vnd.ms-word.template.macroEnabled.12",
                ".dotx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
                ".dsp" => "application/octet-stream",
                ".dtd" => "text/xml",
                ".dvi" => "application/x-dvi",
                ".dvr-ms" => "video/x-ms-dvr",
                ".dwf" => "drawing/x-dwf",
                ".dwp" => "application/octet-stream",
                ".dxr" => "application/x-director",
                ".eml" => "message/rfc822",
                ".emz" => "application/octet-stream",
                ".eot" => "application/vnd.ms-fontobject",
                ".eps" => "application/postscript",
                ".etx" => "text/x-setext",
                ".evy" => "application/envoy",
                ".fdf" => "application/vnd.fdf",
                ".fif" => "application/fractals",
                ".fla" => "application/octet-stream",
                ".flr" => "x-world/x-vrml",
                ".flv" => "video/x-flv",
                ".gif" => "image/gif",
                ".gtar" => "application/x-gtar",
                ".gz" => "application/x-gzip",
                ".h" => "text/plain",
                ".hdf" => "application/x-hdf",
                ".hdml" => "text/x-hdml",
                ".hhc" => "application/x-oleobject",
                ".hhk" => "application/octet-stream",
                ".hhp" => "application/octet-stream",
                ".hlp" => "application/winhlp",
                ".hqx" => "application/mac-binhex40",
                ".hta" => "application/hta",
                ".htc" => "text/x-component",
                ".htm" => "text/html",
                ".html" => "text/html",
                ".htt" => "text/webviewhtml",
                ".hxt" => "text/html",
                ".ical" => "text/calendar",
                ".icalendar" => "text/calendar",
                ".ico" => "image/x-icon",
                ".ics" => "text/calendar",
                ".ief" => "image/ief",
                ".ifb" => "text/calendar",
                ".iii" => "application/x-iphone",
                ".inf" => "application/octet-stream",
                ".ins" => "application/x-internet-signup",
                ".isp" => "application/x-internet-signup",
                ".IVF" => "video/x-ivf",
                ".jar" => "application/java-archive",
                ".java" => "application/octet-stream",
                ".jck" => "application/liquidmotion",
                ".jcz" => "application/liquidmotion",
                ".jfif" => "image/pjpeg",
                ".jpb" => "application/octet-stream",
                ".jpe" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".jpg" => "image/jpeg",
                ".js" => "application/javascript",
                ".json" => "application/json",
                ".jsx" => "text/jscript",
                ".latex" => "application/x-latex",
                ".lit" => "application/x-ms-reader",
                ".lpk" => "application/octet-stream",
                ".lsf" => "video/x-la-asf",
                ".lsx" => "video/x-la-asf",
                ".lzh" => "application/octet-stream",
                ".m13" => "application/x-msmediaview",
                ".m14" => "application/x-msmediaview",
                ".m1v" => "video/mpeg",
                ".m2ts" => "video/vnd.dlna.mpeg-tts",
                ".m3u" => "audio/x-mpegurl",
                ".m4a" => "audio/mp4",
                ".m4v" => "video/mp4",
                ".man" => "application/x-troff-man",
                ".manifest" => "application/x-ms-manifest",
                ".map" => "text/plain",
                ".markdown" => "text/markdown",
                ".md" => "text/markdown",
                ".mdb" => "application/x-msaccess",
                ".mdp" => "application/octet-stream",
                ".me" => "application/x-troff-me",
                ".mht" => "message/rfc822",
                ".mhtml" => "message/rfc822",
                ".mid" => "audio/mid",
                ".midi" => "audio/mid",
                ".mix" => "application/octet-stream",
                ".mmf" => "application/x-smaf",
                ".mno" => "text/xml",
                ".mny" => "application/x-msmoney",
                ".mov" => "video/quicktime",
                ".movie" => "video/x-sgi-movie",
                ".mp2" => "video/mpeg",
                ".mp3" => "audio/mpeg",
                ".mp4" => "video/mp4",
                ".mp4v" => "video/mp4",
                ".mpa" => "video/mpeg",
                ".mpe" => "video/mpeg",
                ".mpeg" => "video/mpeg",
                ".mpg" => "video/mpeg",
                ".mpp" => "application/vnd.ms-project",
                ".mpv2" => "video/mpeg",
                ".ms" => "application/x-troff-ms",
                ".msi" => "application/octet-stream",
                ".mso" => "application/octet-stream",
                ".mvb" => "application/x-msmediaview",
                ".mvc" => "application/x-miva-compiled",
                ".nc" => "application/x-netcdf",
                ".nsc" => "video/x-ms-asf",
                ".nws" => "message/rfc822",
                ".ocx" => "application/octet-stream",
                ".oda" => "application/oda",
                ".odc" => "text/x-ms-odc",
                ".ods" => "application/oleobject",
                ".oga" => "audio/ogg",
                ".ogg" => "video/ogg",
                ".ogv" => "video/ogg",
                ".ogx" => "application/ogg",
                ".one" => "application/onenote",
                ".onea" => "application/onenote",
                ".onetoc" => "application/onenote",
                ".onetoc2" => "application/onenote",
                ".onetmp" => "application/onenote",
                ".onepkg" => "application/onenote",
                ".osdx" => "application/opensearchdescription+xml",
                ".otf" => "font/otf",
                ".p10" => "application/pkcs10",
                ".p12" => "application/x-pkcs12",
                ".p7b" => "application/x-pkcs7-certificates",
                ".p7c" => "application/pkcs7-mime",
                ".p7m" => "application/pkcs7-mime",
                ".p7r" => "application/x-pkcs7-certreqresp",
                ".p7s" => "application/pkcs7-signature",
                ".pbm" => "image/x-portable-bitmap",
                ".pcx" => "application/octet-stream",
                ".pcz" => "application/octet-stream",
                ".pdf" => "application/pdf",
                ".pfb" => "application/octet-stream",
                ".pfm" => "application/octet-stream",
                ".pfx" => "application/x-pkcs12",
                ".pgm" => "image/x-portable-graymap",
                ".pko" => "application/vnd.ms-pki.pko",
                ".pma" => "application/x-perfmon",
                ".pmc" => "application/x-perfmon",
                ".pml" => "application/x-perfmon",
                ".pmr" => "application/x-perfmon",
                ".pmw" => "application/x-perfmon",
                ".png" => "image/png",
                ".pnm" => "image/x-portable-anymap",
                ".pnz" => "image/png",
                ".pot" => "application/vnd.ms-powerpoint",
                ".potm" => "application/vnd.ms-powerpoint.template.macroEnabled.12",
                ".potx" => "application/vnd.openxmlformats-officedocument.presentationml.template",
                ".ppam" => "application/vnd.ms-powerpoint.addin.macroEnabled.12",
                ".ppm" => "image/x-portable-pixmap",
                ".pps" => "application/vnd.ms-powerpoint",
                ".ppsm" => "application/vnd.ms-powerpoint.slideshow.macroEnabled.12",
                ".ppsx" => "application/vnd.openxmlformats-officedocument.presentationml.slideshow",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptm" => "application/vnd.ms-powerpoint.presentation.macroEnabled.12",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".prf" => "application/pics-rules",
                ".prm" => "application/octet-stream",
                ".prx" => "application/octet-stream",
                ".ps" => "application/postscript",
                ".psd" => "application/octet-stream",
                ".psm" => "application/octet-stream",
                ".psp" => "application/octet-stream",
                ".pub" => "application/x-mspublisher",
                ".qt" => "video/quicktime",
                ".qtl" => "application/x-quicktimeplayer",
                ".qxd" => "application/octet-stream",
                ".ra" => "audio/x-pn-realaudio",
                ".ram" => "audio/x-pn-realaudio",
                ".rar" => "application/octet-stream",
                ".ras" => "image/x-cmu-raster",
                ".rf" => "image/vnd.rn-realflash",
                ".rgb" => "image/x-rgb",
                ".rm" => "application/vnd.rn-realmedia",
                ".rmi" => "audio/mid",
                ".roff" => "application/x-troff",
                ".rpm" => "audio/x-pn-realaudio-plugin",
                ".rtf" => "application/rtf",
                ".rtx" => "text/richtext",
                ".scd" => "application/x-msschedule",
                ".sct" => "text/scriptlet",
                ".sea" => "application/octet-stream",
                ".setpay" => "application/set-payment-initiation",
                ".setreg" => "application/set-registration-initiation",
                ".sgml" => "text/sgml",
                ".sh" => "application/x-sh",
                ".shar" => "application/x-shar",
                ".sit" => "application/x-stuffit",
                ".sldm" => "application/vnd.ms-powerpoint.slide.macroEnabled.12",
                ".sldx" => "application/vnd.openxmlformats-officedocument.presentationml.slide",
                ".smd" => "audio/x-smd",
                ".smi" => "application/octet-stream",
                ".smx" => "audio/x-smd",
                ".smz" => "audio/x-smd",
                ".snd" => "audio/basic",
                ".snp" => "application/octet-stream",
                ".spc" => "application/x-pkcs7-certificates",
                ".spl" => "application/futuresplash",
                ".spx" => "audio/ogg",
                ".src" => "application/x-wais-source",
                ".ssm" => "application/streamingmedia",
                ".sst" => "application/vnd.ms-pki.certstore",
                ".stl" => "application/vnd.ms-pki.stl",
                ".sv4cpio" => "application/x-sv4cpio",
                ".sv4crc" => "application/x-sv4crc",
                ".svg" => "image/svg+xml",
                ".svgz" => "image/svg+xml",
                ".swf" => "application/x-shockwave-flash",
                ".t" => "application/x-troff",
                ".tar" => "application/x-tar",
                ".tcl" => "application/x-tcl",
                ".tex" => "application/x-tex",
                ".texi" => "application/x-texinfo",
                ".texinfo" => "application/x-texinfo",
                ".tgz" => "application/x-compressed",
                ".thmx" => "application/vnd.ms-officetheme",
                ".thn" => "application/octet-stream",
                ".tif" => "image/tiff",
                ".tiff" => "image/tiff",
                ".toc" => "application/octet-stream",
                ".tr" => "application/x-troff",
                ".trm" => "application/x-msterminal",
                ".ts" => "video/vnd.dlna.mpeg-tts",
                ".tsv" => "text/tab-separated-values",
                ".ttc" => "application/x-font-ttf",
                ".ttf" => "application/x-font-ttf",
                ".tts" => "video/vnd.dlna.mpeg-tts",
                ".txt" => "text/plain",
                ".u32" => "application/octet-stream",
                ".uls" => "text/iuls",
                ".ustar" => "application/x-ustar",
                ".vbs" => "text/vbscript",
                ".vcf" => "text/x-vcard",
                ".vcs" => "text/plain",
                ".vdx" => "application/vnd.ms-visio.viewer",
                ".vml" => "text/xml",
                ".vsd" => "application/vnd.visio",
                ".vss" => "application/vnd.visio",
                ".vst" => "application/vnd.visio",
                ".vsto" => "application/x-ms-vsto",
                ".vsw" => "application/vnd.visio",
                ".vsx" => "application/vnd.visio",
                ".vtx" => "application/vnd.visio",
                ".wasm" => "application/wasm",
                ".wav" => "audio/wav",
                ".wax" => "audio/x-ms-wax",
                ".wbmp" => "image/vnd.wap.wbmp",
                ".wcm" => "application/vnd.ms-works",
                ".wdb" => "application/vnd.ms-works",
                ".webm" => "video/webm",
                ".webp" => "image/webp",
                ".wks" => "application/vnd.ms-works",
                ".wm" => "video/x-ms-wm",
                ".wma" => "audio/x-ms-wma",
                ".wmd" => "application/x-ms-wmd",
                ".wmf" => "application/x-msmetafile",
                ".wml" => "text/vnd.wap.wml",
                ".wmlc" => "application/vnd.wap.wmlc",
                ".wmls" => "text/vnd.wap.wmlscript",
                ".wmlsc" => "application/vnd.wap.wmlscriptc",
                ".wmp" => "video/x-ms-wmp",
                ".wmv" => "video/x-ms-wmv",
                ".wmx" => "video/x-ms-wmx",
                ".wmz" => "application/x-ms-wmz",
                ".woff" => "application/font-woff",
                ".woff2" => "font/woff2",
                ".wps" => "application/vnd.ms-works",
                ".wri" => "application/x-mswrite",
                ".wrl" => "x-world/x-vrml",
                ".wrz" => "x-world/x-vrml",
                ".wsdl" => "text/xml",
                ".wtv" => "video/x-ms-wtv",
                ".wvx" => "video/x-ms-wvx",
                ".x" => "application/directx",
                ".xaf" => "x-world/x-vrml",
                ".xaml" => "application/xaml+xml",
                ".xap" => "application/x-silverlight-app",
                ".xbap" => "application/x-ms-xbap",
                ".xbm" => "image/x-xbitmap",
                ".xdr" => "text/plain",
                ".xht" => "application/xhtml+xml",
                ".xhtml" => "application/xhtml+xml",
                ".xla" => "application/vnd.ms-excel",
                ".xlam" => "application/vnd.ms-excel.addin.macroEnabled.12",
                ".xlc" => "application/vnd.ms-excel",
                ".xlm" => "application/vnd.ms-excel",
                ".xls" => "application/vnd.ms-excel",
                ".xlsb" => "application/vnd.ms-excel.sheet.binary.macroEnabled.12",
                ".xlsm" => "application/vnd.ms-excel.sheet.macroEnabled.12",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".xlt" => "application/vnd.ms-excel",
                ".xltm" => "application/vnd.ms-excel.template.macroEnabled.12",
                ".xltx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.template",
                ".xlw" => "application/vnd.ms-excel",
                ".xml" => "text/xml",
                ".xof" => "x-world/x-vrml",
                ".xpm" => "image/x-xpixmap",
                ".xps" => "application/vnd.ms-xpsdocument",
                ".xsd" => "text/xml",
                ".xsf" => "text/xml",
                ".xsl" => "text/xml",
                ".xslt" => "text/xml",
                ".xsn" => "application/octet-stream",
                ".xtp" => "application/octet-stream",
                ".xwd" => "image/x-xwindowdump",
                ".z" => "application/x-compress",
                ".zip" => "application/x-zip-compressed",
                _ => "application/octet-stream",
            };
        }

        public static string FormatFileName(string fileName)
        {
            try
            {
                fileName = fileName.Replace("/", string.Empty);
                fileName = fileName.Replace("\\", string.Empty);
                fileName = fileName.Replace(":", string.Empty);
                fileName = fileName.Replace("*", string.Empty);
                fileName = fileName.Replace("?", string.Empty);
                fileName = fileName.Replace("\"", string.Empty);
                fileName = fileName.Replace("<", string.Empty);
                fileName = fileName.Replace(">", string.Empty);
                fileName = fileName.Replace("|", string.Empty);
                return fileName;
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "FormatFileName");
            }
        }

        public static Dictionary<string, List<string>> GetRootFileListBySearchPattern(string[] searchPatternList, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            try
            {
                Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
                foreach (string text in searchPatternList)
                {
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    FileInfo fileInfo = new FileInfo(text);
                    if (string.IsNullOrEmpty(fileInfo.DirectoryName) || !Directory.Exists(fileInfo.DirectoryName))
                    {
                        continue;
                    }

                    string[] files = Directory.GetFiles(fileInfo.DirectoryName, fileInfo.Name, searchOption);
                    if (files.Length != 0)
                    {
                        if (!dictionary.ContainsKey(fileInfo.DirectoryName))
                        {
                            dictionary.Add(fileInfo.DirectoryName, new List<string>());
                        }

                        dictionary[fileInfo.DirectoryName].AddRange(files);
                    }
                }

                return dictionary;
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "GetRootFileListBySearchPattern");
            }
        }

        public static List<string> GetRootFolderListBySearchPattern(string[] searchPatternList)
        {
            try
            {
                List<string> list = new List<string>();
                foreach (string text in searchPatternList)
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        FileInfo fileInfo = new FileInfo(text);
                        if (!string.IsNullOrEmpty(fileInfo.DirectoryName) && Directory.Exists(fileInfo.DirectoryName) && !list.Contains(fileInfo.DirectoryName))
                        {
                            list.Add(fileInfo.DirectoryName);
                        }
                    }
                }

                return list;
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "GetRootFolderListBySearchPattern");
            }
        }

        public static void RemoveChildEmptyFolder(string rootFolderPath, Action<string> actionDelete)
        {
            try
            {
                string[] directories = Directory.GetDirectories(rootFolderPath);
                foreach (string text in directories)
                {
                    RemoveChildEmptyFolder(text, actionDelete);
                    string[] files = Directory.GetFiles(text);
                    string[] directories2 = Directory.GetDirectories(text);
                    if (files.Length == 0 && directories2.Length == 0)
                    {
                        Directory.Delete(text);
                        actionDelete(text);
                    }
                }
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "RemoveChildEmptyFolder");
            }
        }

        public static string GetExMessageBody(Exception ex)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(ex.Message);
                Exception innerException = ex.InnerException;
                int num = 1;
                while (innerException != null)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.Append(innerException.Message);
                    innerException = innerException.InnerException;
                    if (num++ > 3)
                    {
                        break;
                    }
                }

                return stringBuilder.ToString();
            }
            catch
            {
                return ex.Message + "," + ex.InnerException?.Message;
            }
        }

        public static Exception CreateException(string? className, Exception ex, string customMsg = "", [CallerMemberName] string memberName = "")
        {
            try
            {
                string exMessageBody = GetExMessageBody(ex);
                if (!string.IsNullOrEmpty(customMsg))
                {
                    return new Exception($"{className}.{memberName} Exception:{exMessageBody} \r\n CustomMsg:{customMsg}");
                }

                return new Exception($"{className}.{memberName} Exception:{exMessageBody}");
            }
            catch
            {
                return ex;
            }
        }

        public static T? DeserializeObject<T>(string value, bool isCamelCase = false)
        {
            try
            {
                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    Formatting = Formatting.Indented
                };
                if (isCamelCase)
                {
                    jsonSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                }

                return JsonConvert.DeserializeObject<T>(value, jsonSerializerSettings);
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "DeserializeObject");
            }
        }

        public static string SerializeObject(object obj, bool isCamelCase = false)
        {
            try
            {
                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    Formatting = Formatting.Indented
                };
                if (isCamelCase)
                {
                    jsonSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                }

                return JsonConvert.SerializeObject(obj, jsonSerializerSettings);
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "SerializeObject");
            }
        }

        public static string Decrypt(string encryptedString, string key)
        {
            try
            {
                if (string.IsNullOrEmpty(encryptedString) || string.IsNullOrEmpty(key))
                {
                    return string.Empty;
                }

                using MD5 mD = MD5.Create();
                byte[] array = Convert.FromBase64String(encryptedString);
                using TripleDES tripleDES = TripleDES.Create();
                tripleDES.Key = mD.ComputeHash(Encoding.ASCII.GetBytes(key));
                tripleDES.Mode = CipherMode.ECB;
                byte[] bytes = tripleDES.CreateDecryptor().TransformFinalBlock(array, 0, array.Length);
                return Encoding.ASCII.GetString(bytes);
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "Decrypt");
            }
        }

        public static string Encrypt(string plainText, string key)
        {
            try
            {
                if (string.IsNullOrEmpty(plainText))
                {
                    return string.Empty;
                }

                using MD5 mD = MD5.Create();
                byte[] bytes = Encoding.ASCII.GetBytes(plainText);
                using TripleDES tripleDES = TripleDES.Create();
                tripleDES.Key = mD.ComputeHash(Encoding.ASCII.GetBytes(key));
                tripleDES.Mode = CipherMode.ECB;
                return Convert.ToBase64String(tripleDES.CreateEncryptor().TransformFinalBlock(bytes, 0, bytes.Length));
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "Encrypt");
            }
        }

        public static string EncryptMD5(string plainText)
        {
            try
            {
                if (string.IsNullOrEmpty(plainText))
                {
                    return string.Empty;
                }

                using MD5 mD = MD5.Create();
                return BitConverter.ToString(mD.ComputeHash(Encoding.UTF8.GetBytes(plainText))).ToLower().Replace("-", "");
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "EncryptMD5");
            }
        }

        public static string GetSymmetricKey(string sasKey, string deviceID)
        {
            try
            {
                string result = string.Empty;
                using (HMACSHA256 hMACSHA = new HMACSHA256(Convert.FromBase64String(sasKey)))
                {
                    result = Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(deviceID)));
                }

                return result;
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "GetSymmetricKey");
            }
        }

        public static X509Certificate2? GetX509Certificate(string x509CertPath, string x509CertPassword)
        {
            try
            {
                X509Certificate2Collection x509Certificate2Collection = new X509Certificate2Collection();
                x509Certificate2Collection.Import(x509CertPath, x509CertPassword, X509KeyStorageFlags.UserKeySet);
                X509Certificate2 x509Certificate = null;
                foreach (X509Certificate2 item in x509Certificate2Collection)
                {
                    if (x509Certificate == null && item.HasPrivateKey)
                    {
                        x509Certificate = item;
                    }
                    else
                    {
                        item.Dispose();
                    }
                }

                return x509Certificate;
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex, "", "GetX509Certificate");
            }
        }

        public static string PostRequest<T>(string requestURL, T requestBody, int timeoutSeconds = 0, Dictionary<string, string>? dicHeader = null)
        {
            Dictionary<string, string> dicHeader2 = dicHeader;
            T requestBody2 = requestBody;
            string requestURL2 = requestURL;
            try
            {
                return Task.Run(async delegate
                {
                    using HttpClient client = new HttpClient();
                    if (timeoutSeconds != 0)
                    {
                        client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                    }

                    if (dicHeader2 != null)
                    {
                        foreach (KeyValuePair<string, string> item in dicHeader2)
                        {
                            client.DefaultRequestHeaders.Add(item.Key, item.Value);
                        }
                    }

                    StringContent content = new StringContent((requestBody2 == null) ? "" : SerializeObject(requestBody2), Encoding.UTF8, "application/json");
                    return await (await client.PostAsync(requestURL2, content)).Content.ReadAsStringAsync();
                }).Result;
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex.InnerException ?? ex, "", "PostRequest");
            }
        }

        public static string GetRequest(string requestURL, int timeoutSeconds = 0, Dictionary<string, string>? dicHeader = null)
        {
            Dictionary<string, string> dicHeader2 = dicHeader;
            string requestURL2 = requestURL;
            try
            {
                return Task.Run(async delegate
                {
                    using HttpClient client = new HttpClient();
                    if (timeoutSeconds != 0)
                    {
                        client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                    }

                    if (dicHeader2 != null)
                    {
                        foreach (KeyValuePair<string, string> item in dicHeader2)
                        {
                            client.DefaultRequestHeaders.Add(item.Key, item.Value);
                        }
                    }

                    return await (await client.GetAsync(requestURL2)).Content.ReadAsStringAsync();
                }).Result;
            }
            catch (Exception ex)
            {
                throw CreateException(clsFullName, ex.InnerException ?? ex, "", "GetRequest");
            }
        }
    }
}

