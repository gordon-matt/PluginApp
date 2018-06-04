using System;
using System.Linq;
using Framework.Plugins;
using Microsoft.AspNetCore.Mvc;

namespace MainApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("plugins")]
    public class PluginController : Controller
    {
        private readonly Lazy<IPluginFinder> pluginFinder;

        public PluginController(Lazy<IPluginFinder> pluginFinder)
        {
            this.pluginFinder = pluginFinder;
        }

        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("install/{systemName}")]
        public JsonResult Install(string systemName)
        {
            systemName = systemName.Replace('-', '.');

            try
            {
                var pluginDescriptor = pluginFinder.Value.GetPluginDescriptors(LoadPluginsMode.All)
                    .FirstOrDefault(x => x.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

                if (pluginDescriptor == null)
                {
                    //No plugin found with the specified id
                    return Json(new { Success = false, Message = "Plugin Not Found" });
                    //return RedirectToAction("Index");
                }

                //check whether plugin is not installed
                if (pluginDescriptor.Installed)
                {
                    return Json(new { Success = false, Message = "Plugin Not Installed" });
                    //return RedirectToAction("Index");
                }

                //install plugin
                pluginDescriptor.Instance().Install();

                //restart application
                //webHelper.Value.RestartSite();
            }
            catch (Exception x)
            {
                //Logger.LogError(new EventId(), x, x.GetBaseException().Message);
                return Json(new { Success = false, Message = x.GetBaseException().Message });
            }

            return Json(new { Success = true, Message = "Successfully installed plugin" });
            //return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("uninstall/{systemName}")]
        public JsonResult Uninstall(string systemName)
        {
            systemName = systemName.Replace('-', '.');

            try
            {
                var pluginDescriptor = pluginFinder.Value.GetPluginDescriptors(LoadPluginsMode.All)
                    .FirstOrDefault(x => x.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

                if (pluginDescriptor == null)
                {
                    //No plugin found with the specified id
                    return Json(new { Success = false, Message = "Plugin Not Found" });
                    //return RedirectToAction("Index");
                }

                //check whether plugin is installed
                if (!pluginDescriptor.Installed)
                {
                    return Json(new { Success = false, Message = "Plugin Not Installed" });
                    //return RedirectToAction("Index");
                }

                //uninstall plugin
                pluginDescriptor.Instance().Uninstall();

                //restart application
                //webHelper.Value.RestartSite();
            }
            catch (Exception x)
            {
                //Logger.LogError(new EventId(), x, x.GetBaseException().Message);
                return Json(new { Success = false, Message = x.GetBaseException().Message });
            }

            return Json(new { Success = true, Message = "Successfully uninstalled plugin" });
            //return RedirectToAction("Index");
        }
    }
}