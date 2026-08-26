using System.IO.Compression;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using ISMDemo.Auth;
using ISMDemo.Utilities;
using MQTTnet.AspNetCore;
using MQTTnet.Protocol;
using System.Security.Claims;
using ISMDemo.Business;
using ISMDemo.Models;
using Microsoft.Extensions.Options;


try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddLocalization();
    builder.Services.Configure<RequestLocalizationOptions>(options =>
    {
        var defaultCulture = Configuration.SupportedCultures[1];
        options.DefaultRequestCulture = new RequestCulture(culture: defaultCulture, uiCulture: defaultCulture);
        options.SupportedCultures = Configuration.SupportedCultureList;
        options.SupportedUICultures = Configuration.SupportedCultureList;
    });

    builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
    {
        options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddAuthentication("Cookies").AddCookie(delegate (CookieAuthenticationOptions options)
    {
        options.Cookie.Name = "FedAuth-NetCore-ISMApp";
        options.Cookie.HttpOnly = false;
        options.ExpireTimeSpan = TimeSpan.FromDays(365);
        options.SlidingExpiration = false;
        //options.Cookie.SameSite = SameSiteMode.None;
        options.AccessDeniedPath = "/AccessDenied";
        options.Events = new CookieAuthenticationEvents()
        {
            OnRedirectToLogin = (context) =>
            {
                if (context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                }
                else
                {
                    context.HttpContext.Response.Redirect($"/{Configuration.ISMVirtualPath}/Login");
                }
                return Task.CompletedTask;
            }
        };

        options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
    });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("UserAuthorize", policy => policy.Requirements.Add(new UserPolicyRequirement()));
    });

    builder.Services.AddSingleton<IAuthorizationHandler, UserPolicyHandler>();
    builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));
    builder.Services.AddSession();
    builder.Services.Configure(delegate (GzipCompressionProviderOptions options)
    {
        options.Level = CompressionLevel.Optimal;

        try
        {
            // Inital ISM Config file
            var visionConfig = ConfigFileManager.ReadFile();

            // If the ism config is empty and system config is not empty, copy the network config
            if (visionConfig == null || visionConfig.Status == EnumDefinition.VisionConfigStatus.NotExist.ToString())
            {
                visionConfig = new VisionConfigModel();
                //var systemConfig = ConfigFileManager.ReadSystemConfigFile();
                //if (systemConfig != null && systemConfig.IsExist)
                //{
                //    // Copy ethernet config
                //    if (systemConfig.EthernetConfig != null)
                //    {
                //        var ipAssignmentMode = systemConfig.EthernetConfig.DHCP ? "auto" : "manual";
                //        visionConfig.Network.Ethernet.IPAssignmentMode = ipAssignmentMode;
                //        // Only when the ip assignment mode is manual, the ip address/subnet mask/gateway config are useful
                //        if (ipAssignmentMode == "manual")
                //        {
                //            visionConfig.Network.Ethernet.IPAddress = systemConfig.EthernetConfig.IP ?? string.Empty;
                //            visionConfig.Network.Ethernet.SubnetMask = systemConfig.EthernetConfig.SubnetMask ?? string.Empty;
                //            visionConfig.Network.Ethernet.Gateway = systemConfig.EthernetConfig.Gateway ?? string.Empty;
                //        }
                //    }

                //    // Copy wireless config
                //    if (systemConfig.WirelessConfig != null)
                //    {
                //        visionConfig.Network.Wireless.SSID = systemConfig.WirelessConfig.SSID ?? string.Empty;
                //        var securityType = systemConfig.WirelessConfig.SecurityType ?? string.Empty;
                //        visionConfig.Network.Wireless.SecurityType = securityType;
                //        if (securityType.ToLower() != "open")
                //        {
                //            visionConfig.Network.Wireless.Secured = true;
                //            visionConfig.Network.Wireless.SecurityKey = systemConfig.WirelessConfig.SecurityKey ?? string.Empty;
                //        }
                //        else
                //        {
                //            visionConfig.Network.Wireless.Secured = false;
                //        }
                //    }

                //    visionConfig.Status = EnumDefinition.VisionConfigStatus.Completed.ToString();
                //}
                //else
                //{
                visionConfig.Status = EnumDefinition.VisionConfigStatus.Initializing.ToString();
                //}

                visionConfig.Password = Configuration.ISMAccessCode;
                ConfigFileManager.SaveFile(visionConfig);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"{Configuration.APP_NAME} builder.Services.Configure failed" + ex.Message);
        }
    });

    builder.Services.AddResponseCompression(delegate (ResponseCompressionOptions options)
    {
        options.EnableForHttps = true;
        options.Providers.Add<GzipCompressionProvider>();
    });

    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
    builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation().AddNewtonsoftJson();

    //add MQTT web socket support
    builder.Services.AddMqttServer((optionBuilder) =>
    {
        optionBuilder.WithPersistentSessions(true);
    }).AddConnections();

    builder.Services.AddMemoryCache();

    var app = builder.Build();

    app.UseExceptionHandler("/Error");
    app.Use(async (context, next) =>
    {
        await next();
        if (context.Response.StatusCode == StatusCodes.Status404NotFound)
        {
            context.Request.Path = $"/{Configuration.ISMVirtualPath}/PageNotFound";
            await next();
        }
    });
    app.UseHsts();
    app.UsePathBase($"/{Configuration.ISMVirtualPath}");
    app.UseHttpsRedirection();  
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseSession();
    app.UseResponseCompression();
    app.UseAuthentication();
    app.UseAuthorization();

    var cultureOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
    cultureOptions.Value.DefaultRequestCulture = new RequestCulture(Configuration.SupportedCultures[0]);
    cultureOptions.Value.SupportedCultures = Configuration.SupportedCultureList;
    cultureOptions.Value.SupportedUICultures = Configuration.SupportedCultureList;
    var cookieProvider = cultureOptions.Value.RequestCultureProviders.OfType<CookieRequestCultureProvider>().First();
    cookieProvider.CookieName = Const.CONST_CULTURE_COOKIE_NAME;
    app.UseRequestLocalization(cultureOptions.Value);

    app.UseEndpoints(endpoints =>
    {
        //mqtt
        endpoints.MapMqtt($"/mqtt");
        endpoints.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Welcome}");
    });

    //add mqtt web socket support
    app.UseMqttServer(server =>
    {
        server.ClientDisconnectedAsync += async args =>
        {
            Configuration.LogHelper.WriteLog($"{args.ClientId} Disconnected, DisconnectType:{args.DisconnectType}");
            await Task.Yield();
        };

        server.ClientConnectedAsync += async args =>
        {
            Configuration.LogHelper.WriteLog($"{args.ClientId} Connected, UserName:{args.UserName}");
            await Task.Yield();
        };

        server.ClientSubscribedTopicAsync += async args =>
        {
            Configuration.LogHelper.WriteLog($"{args.ClientId} Subscribed {args.TopicFilter.Topic}");
            await Task.Yield();
        };

        server.ClientUnsubscribedTopicAsync += async args =>
        {
            Configuration.LogHelper.WriteLog($"{args.ClientId} Unsubscribed {args.TopicFilter}");
            await Task.Yield();
        };

        server.ValidatingConnectionAsync += async args =>
        {
            try
            {
                var clientID = args.ClientId;
                var userName = args.Username;
                var password = args.Password;
                if (clientID.Length < 10)
                {
                    args.ReasonCode = MqttConnectReasonCode.ClientIdentifierNotValid;
                    return;
                }

                var context = app.Services.GetService<IHttpContextAccessor>();
                var identity = context?.HttpContext?.User?.Identity as ClaimsIdentity;
                var isAuth = identity?.IsAuthenticated ?? false;
                if (isAuth)
                {
                    args.ReasonCode = MqttConnectReasonCode.Success;
                    return;
                }

                if (string.IsNullOrWhiteSpace(userName))
                {
                    userName = BaseHelper.Decrypt(context?.HttpContext?.Request?.Cookies["MQTTUserName"], Configuration.DESSecurityKey);
                }

                if (userName != Configuration.MQTTUser)
                {
                    args.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                    return;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    password = BaseHelper.Decrypt(context?.HttpContext?.Request?.Cookies["MQTTPassword"], Configuration.DESSecurityKey);
                }

                if (password != Configuration.MQTTPassword)
                {
                    args.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                    return;
                }
                args.ReasonCode = MqttConnectReasonCode.Success;
            }
            catch (Exception ex)
            {
                Configuration.LogHelper.WriteException($"{Configuration.APP_NAME}.ValidatingConnectionAsync", ex);
                args.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                Console.Error.WriteLine($"{Configuration.APP_NAME} server.ValidatingConnectionAsync failed," + ex.Message);
            }
            finally
            {
                await Task.Yield();
            }
        };

    });

    //Console.Error.WriteLine($"{Configuration.APP_NAME} failed for chutesideISMwebapp test1");
    //throw new Exception($"{Configuration.APP_NAME} failed for chutesideISMwebapp test2");

    app.Run();

    Configuration.LogHelper.WriteLog($"{Configuration.APP_NAME} start");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"{DateTime.Now}:{Configuration.APP_NAME} start failed, ex:{ex.Message}");
    Environment.Exit(0);
}