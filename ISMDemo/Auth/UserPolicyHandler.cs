using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ISMDemo.Business;
using ISMDemo.Models;
using ISMDemo.Utilities;


namespace ISMDemo.Auth
{
    public class UserPolicyHandler : AuthorizationHandler<UserPolicyRequirement>
    {
        //private ITempDataDictionaryFactory _tempDictionaryFactory;

        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserPolicyHandler(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
        {
            this._serviceProvider = serviceProvider;
            this._httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Makes a decision if authorization is allowed based on a specific requirement.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to evaluate.</param>
        /// <returns>Task object</returns>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserPolicyRequirement requirement)
        {
            try
            {
                //Get user authenticated
                var identity = context.User.Identity as ClaimsIdentity;
                var isAuthenticated = identity?.IsAuthenticated ?? false;

                if (Configuration.IsDemo)
                {
                    context.Fail();
                    if (isAuthenticated)
                    {
                        this._httpContextAccessor?.HttpContext?.SignOutAsync();
                    }
                    return Task.CompletedTask;
                }                

                //Get vision config file
                var visionConfig = ConfigFileManager.ReadFile();
                if (visionConfig == null 
                    || visionConfig.Status == EnumDefinition.VisionConfigStatus.NotExist.ToString() 
                    || string.IsNullOrWhiteSpace(visionConfig.Password))
                {
                    if (File.Exists(Configuration.VisionConfigFilePath))
                    {
                        //backup old file
                        try
                        {
                            var fileInfo = new FileInfo(Configuration.VisionConfigFilePath);
                            var bkFilePath = Path.Combine(fileInfo.DirectoryName, $"bk_{(Environment.TickCount / 1000).ToString()}_{fileInfo.Name}");
                            File.Copy(Configuration.VisionConfigFilePath, bkFilePath);
                        }
                        catch { }
                    }

                    context.Fail();
                    if (isAuthenticated)
                    {
                        this._httpContextAccessor?.HttpContext?.SignOutAsync();
                    }
                    return Task.CompletedTask;
                }

                if (!visionConfig.Password.Equals(Configuration.ISMAccessCode, StringComparison.OrdinalIgnoreCase))
                {
                    context.Fail();
                    if (isAuthenticated)
                    {
                        this._httpContextAccessor?.HttpContext?.SignOutAsync();
                    }
                    return Task.CompletedTask;
                }

                var isFirstTime = visionConfig.Status != EnumDefinition.VisionConfigStatus.Completed.ToString();

                if (!isAuthenticated)
                {
                    context.Fail();
                }
                else
                {
                    var encryptedPwd = identity?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
                    if (!visionConfig.Password.Equals(encryptedPwd))
                    {
                        context.Fail();
                        this._httpContextAccessor?.HttpContext?.SignOutAsync();
                    }
                    else
                    {
                        if (isFirstTime)
                        {
                            this._httpContextAccessor?.HttpContext?.Session.Set(Const.SESSION_CONFIG_MODE_NAME, Encoding.UTF8.GetBytes(Const.CONST_CONFIG_INIT));
                        }
                        else
                        {
                            this._httpContextAccessor?.HttpContext?.Session.Set(Const.SESSION_CONFIG_MODE_NAME, Encoding.UTF8.GetBytes(Const.CONST_CONFIG_UPDATE));
                        }

                        context.Succeed(requirement);
                    }
                }
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Configuration.LogHelper.WriteException(this.GetType().FullName ?? this.GetType().Name, ex);
                context.Fail();
                return Task.CompletedTask;
            }
        }
    }
}