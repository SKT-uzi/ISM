
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using ISMDemo.Utilities;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ISMDemo.Filters
{    
    public class ActivityTrackingFilter : IActionFilter
    {
        private readonly ILogger<ActivityTrackingFilter> _logger;

        public ActivityTrackingFilter(ILogger<ActivityTrackingFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
 
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }
    }
}
