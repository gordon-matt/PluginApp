using Autofac;
using Framework.Infrastructure;
using Framework.Plugins;

namespace MainApp.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order => 1;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<PluginFinder>().As<IPluginFinder>().InstancePerDependency();
        }
    }
}