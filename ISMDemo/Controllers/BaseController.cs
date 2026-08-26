using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ISMDemo.Auth;
using ISMDemo.Models;
using ISMDemo.Utilities;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace ISMDemo.Controllers
{
    public abstract class BaseController<T> : Controller where T : BaseController<T>
    {
        protected string ClassFullName
        {
            get { return this.GetType().FullName ?? this.GetType().Name; }
        }

        /// <summary>
        /// Gets the access code.
        /// </summary>
        /// <value>
        /// The access code.
        /// </value>
        protected string AccessCode
        {
            get
            {
                return User?.Identity?.Name ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        protected int UserID
        {
            get
            {
                var identity = User.Identity as ClaimsIdentity;
                return Convert.ToInt32(identity?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            }
        }

        /// <summary>
        /// Gets the location identifier.
        /// </summary>
        /// <value>
        /// The location identifier.
        /// </value>
        protected int LocationID
        {
            get
            {
                var identity = User.Identity as ClaimsIdentity;
                return Convert.ToInt32(identity?.FindFirst(ClaimTypes.GroupSid)?.Value ?? "0");
            }
        }

        /// <summary>
        /// Gets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        protected string FirstName
        {
            get
            {
                var identity = User.Identity as ClaimsIdentity;
                return identity?.FindFirst(ClaimTypes.GivenName)?.Value ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        protected string LastName
        {
            get
            {
                var identity = User.Identity as ClaimsIdentity;
                return identity?.FindFirst(ClaimTypes.Surname)?.Value ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets the full name.
        /// </summary>
        /// <value>
        /// The full name.
        /// </value>
        protected string FullName
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }

        /// <summary>
        /// Gets the name of the feature.
        /// </summary>
        /// <value>
        /// The name of the feature.
        /// </value>
        protected string FeatureName
        {
            get
            {
                var identity = User.Identity as ClaimsIdentity;
                return identity?.FindFirst(ClaimTypes.Actor)?.Value ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets the local offset hours.
        /// </summary>
        /// <value>
        /// The local offset hours.
        /// </value>
        protected int LocalOffsetHours
        {
            get
            {
                var identity = User.Identity as ClaimsIdentity;
                return Convert.ToInt32(identity?.FindFirst(ClaimTypes.Country)?.Value ?? "0");
            }
        }

        /// <summary>
        /// Gets the server offset hours.
        /// </summary>
        /// <value>
        /// The server offset hours.
        /// </value>
        protected int ServerOffsetHours
        {
            get
            {
                //return -1 * TimeZoneInfo.Local.BaseUtcOffset.Hours;
                return -1 * TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).Hours;
            }
        }

        /// <summary>
        /// Handles the error.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="memberName">Name of the member.</param>
        protected void HandleError(Exception ex, [CallerMemberName] string memberName = "")
        {
            try
            {
                Configuration.LogHelper.WriteException(this.ClassFullName, ex, default, memberName);
                var errorViewModel = new ErrorViewModel
                {
                    RequestId = HttpContext.TraceIdentifier,
                    ErrorMessage = ex.Message,
                    ErrorDetail = string.IsNullOrWhiteSpace(ex.StackTrace) ? ex.Message : ex.StackTrace,
                    IsOriginalError = true
                };

                HttpContext.Session.Set(Const.SESSION_ERROR_NAME, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(errorViewModel)));
                HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
            catch (Exception ex2)
            {
                Configuration.LogHelper.WriteException(this.ClassFullName, ex2);
            }
        }

        #region Private Methods
        /// <summary>
        /// Gets the current unix time stamp.
        /// </summary>
        /// <returns></returns>
        protected static long GetCurrentUnixTimeStamp()
        {
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long times = Convert.ToInt64(ts.TotalMilliseconds);
            return times;
        }
        #endregion
    }
}
