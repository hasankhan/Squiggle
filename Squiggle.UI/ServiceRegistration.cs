using System;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Squiggle.Client;
using Squiggle.History;
using Squiggle.UI.Components;
using Squiggle.UI.Windows;
using Squiggle.Utilities;
using Squiggle.Utilities.Application;

namespace Squiggle.UI
{
    static class ServiceRegistration
    {
        [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
            Justification = "MEF plugin loading requires reflection; plugins are loaded from disk at runtime")]
        public static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    Path.Combine(AppInfo.Location, "logs", "squiggle-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7)
                .CreateLogger();

            services.AddLogging(builder => builder.AddSerilog(dispose: true));

            services.AddSingleton(_ => Settings.SettingsProvider.Current);

            services.AddSingleton<HistoryManager?>(provider =>
            {
                ConnectionStringSettings? setting = ConfigurationManager.ConnectionStrings["HistoryContext"];
                if (setting == null)
                    return null;

                string connectionString = Environment.ExpandEnvironmentVariables(setting.ConnectionString);
                return new HistoryManager(connectionString);
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
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                return new ChatClient(settings.ConnectionSettings.ClientID, history, loggerFactory);
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

            var serviceProvider = services.BuildServiceProvider();

            ExceptionMonster.Logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("ExceptionMonster");

            return serviceProvider;
        }
    }
}
