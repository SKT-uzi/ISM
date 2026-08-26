using Newtonsoft.Json;

namespace ISMDemo.Utilities
{
    public class CommonHelper
    {
        public static string GetChuteSideAppUrl(HttpRequest httpReq) 
        {
            // Get Chute Side Web App url by current url schema
            var host = httpReq.Host.ToString()
                .Replace("internalSite", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("ismSite", string.Empty, StringComparison.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(Configuration.ISMVirtualPath) && Configuration.ISMVirtualPath.Length > 0)
            {
                host = host.Replace(Configuration.ISMVirtualPath, string.Empty, StringComparison.OrdinalIgnoreCase);
            }

#if DEBUG
            return $"http://localhost/";
#endif
            return string.IsNullOrEmpty(host) ? "/" : host;
        }
    }
}
