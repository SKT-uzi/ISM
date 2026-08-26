using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ISMDemo.Models;
using ISMDemo.Utilities;
using ISMDemo.Utilities.Localize;
using Newtonsoft.Json;
using static EnumDefinition;
using ISMDemo.Business;

namespace ISMDemo.Controllers
{
    [Route("[controller]")]
    [Authorize("UserAuthorize")]
    public class SettingController : BaseController<SettingController>
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public SettingController(IStringLocalizer<Resource> localizer, IHttpContextAccessor httpContextAccessor)
        {
            this._contextAccessor = httpContextAccessor;
            LocalizerHelper.LocalizerResource = localizer;
        }

        [HttpGet("Network")]
        public IActionResult Network()
        {
            try
            {                
                var visionConfig = ConfigFileManager.ReadFile();

                if (visionConfig != null)
                {                    
                    ViewData["VisionConfigNetworkSection"] = JsonConvert.SerializeObject(visionConfig.Network);
                }
                else
                {
                    ViewData["VisionConfigNetworkSection"] = JsonConvert.SerializeObject(new VisionConfigModel().Network);
                }
                
                var stepStatus = new StepStatus
                {
                    CameraCompleted = false,
                    ScaleCompleted = false,
                    EIDCompleted = false
                };
                var isConfigInit = visionConfig?.Status != VisionConfigStatus.Completed.ToString();
                if (isConfigInit)
                {
                    if (visionConfig.Camera.IsCompleted)
                    {
                        stepStatus.CameraCompleted = true;
                    }
                    if (visionConfig.Scale.IsCompleted)
                    {
                        stepStatus.ScaleCompleted = true;
                    }
                    if (visionConfig.EID.LF.IsCompleted || visionConfig.EID.UHF.IsCompleted)
                    {
                        stepStatus.EIDCompleted = true;
                    }
                }    

                ViewData["StepStatus"] = JsonConvert.SerializeObject(stepStatus);

                return View();
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
                return Content("");
            }
        }

        [HttpPost]
        [Route("InitNetwork")]
        public IActionResult InitNetwork([FromForm] VisionConfigNetworkModel network)
        {
            try
            {
                if (network == null)
                {
                    return Content("Network_Error_NetworkIsNull");
                }

                var visionConfig = ConfigFileManager.ReadFile();

                if (visionConfig == null || visionConfig.Status == EnumDefinition.VisionConfigStatus.NotExist.ToString())
                {
                    return Content("Common_Error_ConfigFileNotExist");
                }

                visionConfig.Network = network;
                ConfigFileManager.SaveFile(visionConfig);

                return Content("OK");
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
                return Content("");
            }
        }

        [HttpGet("Camera")]
        public IActionResult Camera()
        {
            try
            {                
                var visionConfig = ConfigFileManager.ReadFile(); 
                if (visionConfig != null)
                {
                    ViewData["VisionConfigCameraSection"] = JsonConvert.SerializeObject(visionConfig.Camera);
                }
                else
                {
                    ViewData["VisionConfigCameraSection"] = JsonConvert.SerializeObject(new VisionConfigModel().Camera);
                }

                var stepStatus = new StepStatus
                {
                    CameraCompleted = false,
                    ScaleCompleted = false,
                    EIDCompleted = false
                };
                var isConfigInit = visionConfig?.Status != VisionConfigStatus.Completed.ToString();
                if (isConfigInit)
                {
                    if (visionConfig.Camera.IsCompleted)
                    {
                        stepStatus.CameraCompleted = true;
                    }
                    if (visionConfig.Scale.IsCompleted)
                    {
                        stepStatus.ScaleCompleted = true;
                    }
                    if (visionConfig.EID.LF.IsCompleted || visionConfig.EID.UHF.IsCompleted)
                    {
                        stepStatus.EIDCompleted = true;
                    }
                }

                ViewData["StepStatus"] = JsonConvert.SerializeObject(stepStatus);

                return View();
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
                return Content("");
            }
        }

        [HttpPost]
        [Route("InitCamera")]
        public IActionResult InitCamera([FromForm] VisionConfigCameraModel camera)
        {
            try
            {                
                if (camera == null)
                {
                    return Content("Camera_Error_CameraIsNull");
                }

                var visionConfig = ConfigFileManager.ReadFile();

                if (visionConfig == null || visionConfig.Status == VisionConfigStatus.NotExist.ToString())
                {
                    return Content("Common_Error_ConfigFileNotExist");
                }

                visionConfig.Camera = camera;
                ConfigFileManager.SaveFile(visionConfig);

              

                return Content("OK");
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
                return Content("");
            }
        }

        [HttpGet("Scale")]
        public IActionResult Scale()
        {
            try
            {                
                var visionConfig = ConfigFileManager.ReadFile();
                if (visionConfig != null)
                {
                    ViewData["VisionConfigScaleSection"] = JsonConvert.SerializeObject(visionConfig.Scale);
                }
                else
                {
                    ViewData["VisionConfigScaleSection"] = JsonConvert.SerializeObject(new VisionConfigModel().Scale);
                }

                var stepStatus = new StepStatus
                {
                    CameraCompleted = false,
                    ScaleCompleted = false,
                    EIDCompleted = false
                };
                var isConfigInit = visionConfig?.Status != VisionConfigStatus.Completed.ToString();
                if (isConfigInit)
                {
                    if (visionConfig.Camera.IsCompleted)
                    {
                        stepStatus.CameraCompleted = true;
                    }
                    if (visionConfig.Scale.IsCompleted)
                    {
                        stepStatus.ScaleCompleted = true;
                    }
                    if (visionConfig.EID.LF.IsCompleted || visionConfig.EID.UHF.IsCompleted)
                    {
                        stepStatus.EIDCompleted = true;
                    }
                }

                ViewData["StepStatus"] = JsonConvert.SerializeObject(stepStatus);

                return View();
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
                return Content("");
            }
        }

        [HttpPost]
        [Route("InitScale")]
        public IActionResult InitScale([FromForm] VisionConfigScaleModel scale)
        {
            try
            {                
                if (scale == null)
                {
                    return Content("Scale_Error_ScaleIsNull");
                }

                var visionConfig = ConfigFileManager.ReadFile();

                if (visionConfig == null || visionConfig.Status == EnumDefinition.VisionConfigStatus.NotExist.ToString())
                {
                    return Content("Common_Error_ConfigFileNotExist");
                }

                visionConfig.Scale = scale;
                ConfigFileManager.SaveFile(visionConfig);

                return Content("OK");
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
                return Content("");
            }
        }

        [HttpGet("EID")]
        public IActionResult EID()
        {
            try
            {                
                var visionConfig = ConfigFileManager.ReadFile();
                if (visionConfig != null)
                {
                    ViewData["VisionConfigEIDSection"] = JsonConvert.SerializeObject(visionConfig.EID);
                    ViewData["VisionConfigBaudRateList"] = JsonConvert.SerializeObject(visionConfig.BaudRateList);
                }
                else
                {
                    ViewData["VisionConfigEIDSection"] = JsonConvert.SerializeObject(new VisionConfigModel().EID);
                    ViewData["VisionConfigBaudRateList"] = JsonConvert.SerializeObject(Const.BAUD_RATE_LIST);
                }

                var stepStatus = new StepStatus
                {
                    CameraCompleted = false,
                    ScaleCompleted = false,
                    EIDCompleted = false
                };
                var isConfigInit = visionConfig?.Status != VisionConfigStatus.Completed.ToString();
                if (isConfigInit)
                {
                    if (visionConfig.Camera.IsCompleted)
                    {
                        stepStatus.CameraCompleted = true;
                    }
                    if (visionConfig.Scale.IsCompleted)
                    {
                        stepStatus.ScaleCompleted = true;
                    }
                    if (visionConfig.EID.LF.IsCompleted || visionConfig.EID.UHF.IsCompleted)
                    {
                        stepStatus.EIDCompleted = true;
                    }
                }

                ViewData["StepStatus"] = JsonConvert.SerializeObject(stepStatus);

                return View();
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
                return Content("");
            }
        }

        [HttpPost]
        [Route("InitEID")]
        public IActionResult InitEID([FromForm] ISMConfigEIDModel EID)
        {
            try
            {                
                if (EID == null)
                {
                    return Content("EID_Error_EIDIsNull");
                }

                var visionConfig = ConfigFileManager.ReadFile();

                if (visionConfig == null || visionConfig.Status == EnumDefinition.VisionConfigStatus.NotExist.ToString())
                {
                    return Content("Common_Error_ConfigFileNotExist");
                }

                visionConfig.EID = EID;
                ConfigFileManager.SaveFile(visionConfig);

                return Content("OK");
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
                return Content("");
            }
        }

        [HttpGet("Done")]
        public IActionResult Done()
        {
            try
            {                
                var visionConfig = ConfigFileManager.ReadFile();
                if (visionConfig == null)
                {
                    ViewData["VisionConfigEIDSection"] = JsonConvert.SerializeObject(new VisionConfigModel().EID);
                }

                var stepStatus = new StepStatus
                {
                    CameraCompleted = false,
                    ScaleCompleted = false,
                    EIDCompleted = false
                };
                if (visionConfig.Camera.IsCompleted)
                {
                    stepStatus.CameraCompleted = true;
                }
                if (visionConfig.Scale.IsCompleted)
                {
                    stepStatus.ScaleCompleted = true;
                }
                if (visionConfig.EID.LF.IsCompleted || visionConfig.EID.UHF.IsCompleted)
                {
                    stepStatus.EIDCompleted = true;
                }

                ViewData["StepStatus"] = JsonConvert.SerializeObject(stepStatus);

                return View();
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
                return Content("");
            }
        }

        [HttpPost]
        [Route("InitComplete")]
        public IActionResult InitComplete()
        {
            try
            {
                var visionConfig = ConfigFileManager.ReadFile();

                if (visionConfig == null || visionConfig.Status == EnumDefinition.VisionConfigStatus.NotExist.ToString())
                {
                    return Content("Common_Error_ConfigFileNotExist");
                }

                visionConfig.Status = VisionConfigStatus.Completed.ToString();
                ConfigFileManager.SaveFile(visionConfig);

                return Content("OK");
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
                return Content("");
            }
        }

        [HttpGet("Overview")]
        public IActionResult Overview()
        {
            try
            {                
                var visionConfig = ConfigFileManager.ReadFile();

                if (visionConfig == null || visionConfig.Status == VisionConfigStatus.NotExist.ToString())
                {
                    return Content("Common_Error_ConfigFileNotExist");
                }

                // If skip to ChuteSideWebApp, it will go to init page from dashboard 
                if (visionConfig.Status != null && visionConfig.Status != VisionConfigStatus.Completed.ToString())
                {
                    return RedirectToAction("Network");
                }

                ViewData["VisionConfigNetworkSection"] = JsonConvert.SerializeObject(visionConfig.Network);

                ViewData["LocationName"] = visionConfig.LocationName;
                ViewData["DeviceID"] = Configuration.DeviceID;

                return View();
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
                return Content("");
            }
        }

        [HttpGet("InitDone")]
        public IActionResult InitDone()
        {
            try
            {                
                ViewData["ChuteSideAppUrl"] = CommonHelper.GetChuteSideAppUrl(HttpContext.Request);
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
