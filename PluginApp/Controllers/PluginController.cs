using Microsoft.AspNetCore.Mvc;

namespace PluginApp.Controllers
{
    [Route("plugin-app")]
    public class PluginController : Controller
    {
        [Route("")]
        public IActionResult Index()
        {
            return View();
            //return View("/Plugins/PluginApp/Views/Plugin/Index.cshtml");
        }
    }
}