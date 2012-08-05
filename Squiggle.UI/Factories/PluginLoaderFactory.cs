using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.UI.Helpers;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.IO;

namespace Squiggle.UI.Factories
{
    class PluginLoaderFactory : IInstanceFactory<PluginLoader>
    {
        public PluginLoader CreateInstance()
        {
            var aggregate = CreateCatalog();
            var instance = new PluginLoader(aggregate);
            return instance;
        }

        static AggregateCatalog CreateCatalog()
        {
            var dirCatalog = CreatePluginDirCatalog();
            var assemblyCatalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            var aggregate = new AggregateCatalog(dirCatalog, assemblyCatalog);
            return aggregate;
        }

        static DirectoryCatalog CreatePluginDirCatalog()
        {
            var pluginPath = Path.Combine(AppInfo.Location, "Plugins");
            if (!Directory.Exists(pluginPath))
                Directory.CreateDirectory(pluginPath);
            var catalog = new DirectoryCatalog(pluginPath);
            return catalog;
        }
    }
}
