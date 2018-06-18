using System;
using System.Linq;
using System.Threading.Tasks;
using Extenso.Collections;
using Framework.Plugins;
using KendoGridBinder;
using KendoGridBinder.ModelBinder.Mvc;
using MainApp.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace MainApp.Areas.Admin.Controllers.Api
{
    [Route("api/plugins")]
    public class PluginApiController : Controller
    {
        private readonly IPluginFinder pluginFinder;

        public PluginApiController(IPluginFinder pluginFinder)
        {
            this.pluginFinder = pluginFinder;
        }

        [HttpPost]
        [Route("get")]
        public virtual async Task<IActionResult> Get([FromBody]KendoGridMvcRequest request)
        {
            var query = pluginFinder.GetPluginDescriptors(LoadPluginsMode.All).Select(x => (EdmPluginDescriptor)x).AsQueryable();
            var grid = new KendoGrid<EdmPluginDescriptor>(request, query);
            return Json(grid, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }

        [HttpGet]
        [Route("{key}")]
        public virtual async Task<IActionResult> Get(string key)
        {
            string systemName = key.Replace('-', '.');
            var pluginDescriptor = pluginFinder.GetPluginDescriptorBySystemName(systemName, LoadPluginsMode.All);
            EdmPluginDescriptor entity = pluginDescriptor;

            return Json(JObject.FromObject(entity));
        }

        [HttpPut]
        [Route("{key}")]
        public virtual async Task<IActionResult> Put(string key, [FromBody]EdmPluginDescriptor entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                string systemName = key.Replace('-', '.');
                var pluginDescriptor = pluginFinder.GetPluginDescriptorBySystemName(systemName, LoadPluginsMode.All);

                if (pluginDescriptor == null)
                {
                    return NotFound();
                }

                pluginDescriptor.FriendlyName = entity.FriendlyName;
                pluginDescriptor.DisplayOrder = entity.DisplayOrder;
                pluginDescriptor.LimitedToTenants.Clear();

                if (!entity.LimitedToTenants.IsNullOrEmpty())
                {
                    pluginDescriptor.LimitedToTenants = entity.LimitedToTenants.ToList();
                }

                PluginManager.SavePluginDescriptor(pluginDescriptor);
            }
            catch (Exception x)
            {
                //Logger.LogError(new EventId(), x, x.GetBaseException().Message);
            }

            return Ok(entity);
        }
    }
}