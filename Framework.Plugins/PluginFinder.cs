using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Plugins
{
    /// <summary>
    /// Plugin finder
    /// </summary>
    public class PluginFinder : IPluginFinder
    {
        #region Fields

        private IList<PluginDescriptor> plugins;
        private bool arePluginsLoaded;

        #endregion Fields

        #region Utilities

        /// <summary>
        /// Ensure plugins are loaded
        /// </summary>
        protected virtual void EnsurePluginsAreLoaded()
        {
            if (!arePluginsLoaded)
            {
                var foundPlugins = PluginManager.ReferencedPlugins.ToList();
                foundPlugins.Sort();
                plugins = foundPlugins.ToList();

                arePluginsLoaded = true;
            }
        }

        /// <summary>
        /// Check whether the plugin is available in a certain tenant
        /// </summary>
        /// <param name="pluginDescriptor">Plugin descriptor to check</param>
        /// <param name="loadMode">Load plugins mode</param>
        /// <returns>true - available; false - no</returns>
        protected virtual bool CheckLoadMode(PluginDescriptor pluginDescriptor, LoadPluginsMode loadMode)
        {
            if (pluginDescriptor == null)
            {
                throw new ArgumentNullException(nameof(pluginDescriptor));
            }

            switch (loadMode)
            {
                case LoadPluginsMode.All: return true;//no filering
                case LoadPluginsMode.InstalledOnly: return pluginDescriptor.Installed;
                case LoadPluginsMode.NotInstalledOnly: return !pluginDescriptor.Installed;
                default: throw new Exception("Not supported LoadPluginsMode");
            }
        }

        /// <summary>
        /// Check whether the plugin is in a certain group
        /// </summary>
        /// <param name="pluginDescriptor">Plugin descriptor to check</param>
        /// <param name="group">Group</param>
        /// <returns>true - available; false - no</returns>
        protected virtual bool CheckGroup(PluginDescriptor pluginDescriptor, string group)
        {
            if (pluginDescriptor == null)
            {
                throw new ArgumentNullException(nameof(pluginDescriptor));
            }

            if (string.IsNullOrEmpty(group))
            {
                return true;
            }

            return group.Equals(pluginDescriptor.Group, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion Utilities

        #region Methods

        /// <summary>
        /// Check whether the plugin is available in a certain tenant
        /// </summary>
        /// <param name="pluginDescriptor">Plugin descriptor to check</param>
        /// <param name="tenantId">Tenant identifier to check</param>
        /// <returns>true - available; false - no</returns>
        public virtual bool AuthenticateTenant(PluginDescriptor pluginDescriptor, int? tenantId)
        {
            if (pluginDescriptor == null)
            {
                throw new ArgumentNullException(nameof(pluginDescriptor));
            }

            //no validation required
            if (!tenantId.HasValue || tenantId == 0)
            {
                return true;
            }

            if (!pluginDescriptor.LimitedToTenants.Any())
            {
                return true;
            }

            return pluginDescriptor.LimitedToTenants.Contains(tenantId.Value);
        }

        /// <summary>
        /// Gets plugin groups
        /// </summary>
        /// <returns>Plugins groups</returns>
        public virtual IEnumerable<string> GetPluginGroups()
        {
            return GetPluginDescriptors(LoadPluginsMode.All).Select(x => x.Group).Distinct().OrderBy(x => x);
        }

        /// <summary>
        /// Gets plugins
        /// </summary>
        /// <typeparam name="T">The type of plugins to get.</typeparam>
        /// <param name="loadMode">Load plugins mode</param>
        /// <param name="tenantId">Load records allowed only in a specified tenant; pass 0 to load all records</param>
        /// <param name="group">Filter by plugin group; pass null to load all records</param>
        /// <returns>Plugins</returns>
        public virtual IEnumerable<T> GetPlugins<T>(
            LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly,
            int tenantId = 0,
            string group = null) where T : class, IPlugin
        {
            return GetPluginDescriptors<T>(loadMode, tenantId, group).Select(p => p.Instance<T>());
        }

        /// <summary>
        /// Get plugin descriptors
        /// </summary>
        /// <param name="loadMode">Load plugins mode</param>
        /// <param name="tenantId">Load records allowed only in a specified tenant; pass 0 to load all records</param>
        /// <param name="group">Filter by plugin group; pass null to load all records</param>
        /// <returns>Plugin descriptors</returns>
        public virtual IEnumerable<PluginDescriptor> GetPluginDescriptors(
            LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly,
            int tenantId = 0,
            string group = null)
        {
            //ensure plugins are loaded
            EnsurePluginsAreLoaded();

            return plugins.Where(p => CheckLoadMode(p, loadMode) && AuthenticateTenant(p, tenantId) && CheckGroup(p, group));
        }

        /// <summary>
        /// Get plugin descriptors
        /// </summary>
        /// <typeparam name="T">The type of plugin to get.</typeparam>
        /// <param name="loadMode">Load plugins mode</param>
        /// <param name="tenantId">Load records allowed only in a specified tenant; pass 0 to load all records</param>
        /// <param name="group">Filter by plugin group; pass null to load all records</param>
        /// <returns>Plugin descriptors</returns>
        public virtual IEnumerable<PluginDescriptor> GetPluginDescriptors<T>(
            LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly,
            int tenantId = 0,
            string group = null)
            where T : class, IPlugin
        {
            return GetPluginDescriptors(loadMode, tenantId, group)
                .Where(p => typeof(T).IsAssignableFrom(p.PluginType));
        }

        /// <summary>
        /// Get a plugin descriptor by its system name
        /// </summary>
        /// <param name="systemName">Plugin system name</param>
        /// <param name="loadMode">Load plugins mode</param>
        /// <returns>>Plugin descriptor</returns>
        public virtual PluginDescriptor GetPluginDescriptorBySystemName(string systemName, LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly)
        {
            return GetPluginDescriptors(loadMode)
                .SingleOrDefault(p => p.SystemName.Equals(systemName, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Get a plugin descriptor by its system name
        /// </summary>
        /// <typeparam name="T">The type of plugin to get.</typeparam>
        /// <param name="systemName">Plugin system name</param>
        /// <param name="loadMode">Load plugins mode</param>
        /// <returns>>Plugin descriptor</returns>
        public virtual PluginDescriptor GetPluginDescriptorBySystemName<T>(string systemName, LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly)
            where T : class, IPlugin
        {
            return GetPluginDescriptors<T>(loadMode)
                .SingleOrDefault(p => p.SystemName.Equals(systemName, StringComparison.InvariantCultureIgnoreCase));
        }

        #endregion Methods
    }
}