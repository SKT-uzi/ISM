using System.Diagnostics;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ISMDemo.Models;
using ISMDemo.Utilities;
using ISMDemo.Utilities.Localize;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Diagnostics;
using ISMDemo.Business;
using System.Net;
using static EnumDefinition;

namespace ISMDemo.Controllers
{
    [Route("")]
    [Route("[controller]")]
    public class HomeController : BaseController<HomeController>
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public HomeController(IStringLocalizer<Resource> localizer, IHttpContextAccessor httpContextAccessor)
        {
            this._contextAccessor = httpContextAccessor;
            LocalizerHelper.LocalizerResource = localizer;
        }

        [Route("Login")]
        [AllowAnonymous]
        public IActionResult Login()
        {            
            this._contextAccessor.HttpContext.Response.Cookies.Append("MQTTUserName", BaseHelper.Encrypt(Configuration.MQTTUser, Configuration.DESSecurityKey));
            this._contextAccessor.HttpContext.Response.Cookies.Append("MQTTPassword", BaseHelper.Encrypt(Configuration.MQTTPassword, Configuration.DESSecurityKey));
            
            return View();
        }

        [Route("")]
        [Route("Landing")]
        public IActionResult Landing()
        {
            var visionConfig = ConfigFileManager.ReadFile();
            //if (visionConfig != null && visionConfig.Status == EnumDefinition.VisionConfigStatus.Completed.ToString())
            //{                
            //    var chuteSideAppUrl = CommonHelper.GetChuteSideAppUrl(HttpContext.Request);
            //    return Redirect(chuteSideAppUrl);
            //}

            ViewData["ChuteSideAppUrl"] = CommonHelper.GetChuteSideAppUrl(HttpContext.Request);
            this._contextAccessor.HttpContext.Response.Cookies.Append("MQTTUserName", BaseHelper.Encrypt(Configuration.MQTTUser, Configuration.DESSecurityKey));
            this._contextAccessor.HttpContext.Response.Cookies.Append("MQTTPassword", BaseHelper.Encrypt(Configuration.MQTTPassword, Configuration.DESSecurityKey));

            // Log out of the user
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var isAuthenticated = identity?.IsAuthenticated ?? false;
            if (isAuthenticated)
            {
                HttpContext.SignOutAsync();
            }

            return View("Welcome");
        }

        [Route("Welcome")]
        public IActionResult Welcome()
        {
            ViewData["ChuteSideAppUrl"] = CommonHelper.GetChuteSideAppUrl(HttpContext.Request);
            this._contextAccessor.HttpContext.Response.Cookies.Append("MQTTUserName", BaseHelper.Encrypt(Configuration.MQTTUser, Configuration.DESSecurityKey));
            this._contextAccessor.HttpContext.Response.Cookies.Append("MQTTPassword", BaseHelper.Encrypt(Configuration.MQTTPassword, Configuration.DESSecurityKey));

            // Log out of the user
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var isAuthenticated = identity?.IsAuthenticated ?? false;
            if (isAuthenticated)
            {
                HttpContext.SignOutAsync();
            }

            return View();
        }

        [HttpPost]
        [Route("UserLogin")]
        [AllowAnonymous]
        public IActionResult UserLogin([FromForm] string password, int localOffsetHours)
        {
            try 
            { 
                if (string.IsNullOrEmpty(password))
                {
                    return Content("Password_Error_PwdIsEmpty");
                }

                #region Rebuild corrupted file or create new file or update the latest management password
                var visionConfig = ConfigFileManager.ReadFile();
                if (visionConfig == null || visionConfig.Status == EnumDefinition.VisionConfigStatus.NotExist.ToString())
                {
                    visionConfig = new VisionConfigModel();
                    visionConfig.Status = EnumDefinition.VisionConfigStatus.Initializing.ToString();
                    visionConfig.Password = Configuration.ISMAccessCode;
                    ConfigFileManager.SaveFile(visionConfig);
                }
                else
                {
                    if (!visionConfig.Password.Equals(Configuration.ISMAccessCode, StringComparison.OrdinalIgnoreCase))
                    {
                        visionConfig.Password = Configuration.ISMAccessCode;
                        ConfigFileManager.SaveFile(visionConfig);
                    }

                    if (visionConfig.BaudRateList == null || visionConfig.BaudRateList.Length == 0)
                    {
                        visionConfig.BaudRateList = Const.BAUD_RATE_LIST;
                        ConfigFileManager.SaveFile(visionConfig);
                    }

                }
                #endregion

                var encrypedPwd = BaseHelper.Encrypt(password, Configuration.DESSecurityKey);

                if (visionConfig == null || visionConfig.Status == EnumDefinition.VisionConfigStatus.NotExist.ToString())
                {
                    return Content("Common_Error_ConfigFileNotExist");
                }
                if (!visionConfig.Password.Equals(encrypedPwd, StringComparison.OrdinalIgnoreCase))
                {
                    return Content("Login_Desc_TryAgain");
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, encrypedPwd),
                    new Claim(ClaimTypes.Country, localOffsetHours.ToString())
                };
                var userIdentity = new ClaimsIdentity(claims, "AccessCode");
                var principal = new ClaimsPrincipal(userIdentity);
                HttpContext.SignInAsync(principal);

                var returnContent = visionConfig != null
                    && visionConfig.Status == VisionConfigStatus.Completed.ToString() ? visionConfig.Status.ToUpper() : "INIT";
                return Content(returnContent);
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
                return RedirectToAction("Error");
            }
        }

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpPost]
        [Route("SignOut")]
        [AllowAnonymous]
        public new IActionResult SignOut()
        {
            //Get user authenticated
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var isAuthenticated = identity?.IsAuthenticated ?? false;
            if (isAuthenticated)
            {
                HttpContext.SignOutAsync();
            }            
            return Content("");
        }

        [Route("SignOutForInit")]
        [Authorize]
        public new IActionResult SignOutForInit()
        {
            //Get user authenticated
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var isAuthenticated = identity?.IsAuthenticated ?? false;
            if (isAuthenticated)
            {
                HttpContext.SignOutAsync();
            }
            return RedirectToAction("Login");
        }

        [HttpPost]
        [Route("SetLanguage")]
        [AllowAnonymous]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                Const.CONST_CULTURE_COOKIE_NAME,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            Request.Cookies.TryGetValue(CookieRequestCultureProvider.DefaultCookieName, out var chuteSideCookie);
            if (string.IsNullOrWhiteSpace(chuteSideCookie))
            {
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                );
            }

            return LocalRedirect(returnUrl);
        }

        [HttpPost]
        [Route("WriteISMActionLog")]
        public IActionResult WriteActionLog([FromForm] string logMessage)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(logMessage) && Configuration.LastMQTTMsg != logMessage)
                {
                    Configuration.LogHelper.WriteLog(logMessage);
                    Configuration.LastMQTTMsg = logMessage;
                }
            }
            catch (Exception ex)
            {
                Configuration.LogHelper.WriteException(this.GetType().FullName ?? this.GetType().Name, ex, default, string.Empty);
            }

            return Content("OK");
        }

        [HttpPost]
        [Route("WriteViewDashboardLog")]
        public IActionResult WriteViewDashboardLog()
        {
            try
            {
            }
            catch (Exception ex)
            {
                Configuration.LogHelper.WriteException(this.GetType().FullName ?? this.GetType().Name, ex, default, string.Empty);
            }

            return Content("OK");
        }

        [Route("Error")]
        [AllowAnonymous]
        public IActionResult Error()
        {
            ErrorViewModel? errorViewModel;

            if (!Configuration.IsDebug)
            {
                errorViewModel = new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    ErrorMessage = LocalizerHelper.LocalizerResource["Common_ErrorMsg_StandardMessage"],
                    ErrorDetail = $"{LocalizerHelper.LocalizerResource["Common_ErrorMsg_StandardContent_1"]}<br/>{LocalizerHelper.LocalizerResource["Common_ErrorMsg_StandardContent_2"]}",
                    IsOriginalError = false
                };
            }
            else
            {
                var errorBytes = HttpContext.Session.Get(Const.SESSION_ERROR_NAME);
                if (errorBytes != null)
                {
                    errorViewModel = JsonConvert.DeserializeObject<ErrorViewModel>(Encoding.UTF8.GetString(errorBytes));
                    errorViewModel ??= new ErrorViewModel();

                    if (string.IsNullOrWhiteSpace(errorViewModel.ErrorMessage))
                    {
                        errorViewModel.ErrorMessage = LocalizerHelper.LocalizerResource["Common_ErrorMsg_BadRequest"];
                    }

                    if (string.IsNullOrWhiteSpace(errorViewModel.ErrorDetail))
                    {
                        errorViewModel.ErrorDetail = LocalizerHelper.LocalizerResource["Common_ErrorMsg_BadRequest"];
                    }
                }
                else
                {
                    var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
                    if (exceptionHandlerPathFeature != null && exceptionHandlerPathFeature.Error != null)
                    {
                        Configuration.LogHelper.WriteException($"{Configuration.APP_NAME}.Error", exceptionHandlerPathFeature.Error);
                    }
                    
                    var errorMessage = exceptionHandlerPathFeature?.Error?.Message ?? string.Empty;
                    var errorStackTrace = exceptionHandlerPathFeature?.Error?.StackTrace ?? string.Empty;
                    errorViewModel = new ErrorViewModel
                    {
                        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                        ErrorMessage = !string.IsNullOrEmpty(errorMessage) ? errorMessage : LocalizerHelper.LocalizerResource["Common_ErrorMsg_BadRequest"],
                        ErrorDetail = !string.IsNullOrEmpty(errorStackTrace) ? errorStackTrace : LocalizerHelper.LocalizerResource["Common_ErrorMsg_BadRequest"],
                        IsOriginalError = !string.IsNullOrEmpty(errorMessage)
                    };
                }

                errorViewModel ??= new ErrorViewModel
                {
                    RequestId = HttpContext.TraceIdentifier,
                    ErrorMessage = LocalizerHelper.LocalizerResource["Common_ErrorMsg_BadRequest"],
                    ErrorDetail = LocalizerHelper.LocalizerResource["Common_ErrorMsg_BadRequest"],
                    IsOriginalError = false
                };
            }

            return View(errorViewModel);
        }

        [Route("AccessDenied")]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            if (UserID == 0)
            {
                return RedirectToAction("Login");
            }
            else
            {
                return View();
            }
        }

        [Route("PageNotFound")]
        [AllowAnonymous]
        public IActionResult PageNotFound()
        {
            return View();
        }

        [Route("Unauthorized")]
        [AllowAnonymous]
        public IActionResult Unauthorized()
        {
            if (UserID == 0)
            {
                return RedirectToAction("Login");
            }
            else
            {
                return View();
            }
        }

        [HttpGet("Dashboard/{LocationName?}")]
        public IActionResult Dashboard(string? locationName)
        {
            try
            {
                #region Rebuild corrupted file or create new file or update the latest management password
                var visionConfig = ConfigFileManager.ReadFile();
                if (visionConfig == null || visionConfig.Status == EnumDefinition.VisionConfigStatus.NotExist.ToString())
                {
                    visionConfig = new VisionConfigModel();
                    visionConfig.Status = EnumDefinition.VisionConfigStatus.Initializing.ToString();
                    visionConfig.Password = Configuration.ISMAccessCode;
                    ConfigFileManager.SaveFile(visionConfig);
                }
                else
                {
                    if (!visionConfig.Password.Equals(Configuration.ISMAccessCode, StringComparison.OrdinalIgnoreCase))
                    {
                        visionConfig.Password = Configuration.ISMAccessCode;
                        ConfigFileManager.SaveFile(visionConfig);
                    }

                    if (visionConfig.BaudRateList == null || visionConfig.BaudRateList.Length == 0)
                    {
                        visionConfig.BaudRateList = Const.BAUD_RATE_LIST;
                        ConfigFileManager.SaveFile(visionConfig);
                    }
                }
                #endregion
                if (visionConfig == null || visionConfig.Status == EnumDefinition.VisionConfigStatus.NotExist.ToString())
                {
                    return Content("Common_Error_ConfigFileNotExist");
                }

                ViewData["VisionConfigNetworkSection"] = JsonConvert.SerializeObject(visionConfig.Network);

                locationName = WebUtility.HtmlDecode(locationName);
                if (visionConfig.LocationName != locationName)
                {
                    visionConfig.LocationName = locationName;
                    ConfigFileManager.SaveFile(visionConfig);
                }

                ViewData["LocationName"] = locationName;
                ViewData["DeviceID"] = Configuration.DeviceID;

                // Log out of the user
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                var isAuthenticated = identity?.IsAuthenticated ?? false;
                if (isAuthenticated)
                {
                    HttpContext.SignOutAsync();
                }

                this._contextAccessor.HttpContext.Response.Cookies.Append("MQTTUserName", BaseHelper.Encrypt(Configuration.MQTTUser, Configuration.DESSecurityKey));
                this._contextAccessor.HttpContext.Response.Cookies.Append("MQTTPassword", BaseHelper.Encrypt(Configuration.MQTTPassword, Configuration.DESSecurityKey));

                return View();
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
                return Content("");
            }
        }
    }
}