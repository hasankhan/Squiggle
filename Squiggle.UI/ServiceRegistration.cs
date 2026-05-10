using System;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Squiggle.Client;
using Squiggle.History;
using Squiggle.UI.Components;
using Squiggle.UI.Windows;
using Squiggle.Utilities.Application;

namespace Squiggle.UI
{
    static class ServiceRegistration
    {
        public static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton(_ => Settings.SettingsProvider.Current);

            services.AddSingleton<HistoryManager?>(provider =>
            {
                ConnectionStringSettings? setting = ConfigurationManager.ConnectionStrings["HistoryContext"];
                if (setting == null)
                    return null;

                string? dbProvider = ConfigurationManager.AppSettings["DbProvider"];
                if (dbProvider == null)
                    return null;

                string connectionString = Environment.ExpandEnvironmentVariables(setting.ConnectionString);
                DbConnection connection = DbProviderFactories.GetFactory(dbProvider).CreateConnection()!;
                connection.ConnectionString = connectionString;
                return new HistoryManager(connection);
            });

            services.AddSingleton<PluginLoader>(provider =>
            {
                var pluginPath = Path.Combine(AppInfo.Location, "Plugins");
                Squiggle.Utilities.Application.Shell.CreateDirectoryIfNotExists(pluginPath);
                var dirCatalog = new DirectoryCatalog(pluginPath);
                var assemblyCatalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
                var aggregate = new AggregateCatalog(dirCatalog, assemblyCatalog);
                return new PluginLoader(aggregate);
            });

            services.AddSingleton<IChatClient>(provider =>
            {
                var settings = provider.GetRequiredService<Settings.SettingsProvider>().Settings;
                var history = provider.GetService<HistoryManager>();
                return new ChatClient(settings.ConnectionSettings.ClientID, history);
            });

            services.AddSingleton<SquiggleContext>(provider =>
            {
                var context = new SquiggleContext();
                context.PluginLoader = provider.GetRequiredService<PluginLoader>();
                context.ChatClient = provider.GetRequiredService<IChatClient>();
                SquiggleContext.Current = context;
                return context;
            });

            services.AddSingleton<MainWindow>();

            return services.BuildServiceProvider();
        }
    }
}
