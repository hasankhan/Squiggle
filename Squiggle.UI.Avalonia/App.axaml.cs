using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Squiggle.Client;
using Squiggle.History;
using Squiggle.UI.Avalonia.Helpers;
using Squiggle.UI.Avalonia.Services;

namespace Squiggle.UI.Avalonia;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public static bool RunInBackground { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Services = ConfigureServices();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (desktop.Args is { Length: > 0 } && desktop.Args[0].Trim() == "/background")
                RunInBackground = true;

            desktop.MainWindow = new MainWindow();
            desktop.ShutdownMode = global::Avalonia.Controls.ShutdownMode.OnMainWindowClose;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        var appLocation = AppDomain.CurrentDomain.BaseDirectory;

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                Path.Combine(appLocation, "logs", "squiggle-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();

        services.AddLogging(builder => builder.AddSerilog(dispose: true));

        services.AddSingleton<IChatClient>(provider =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            return new ChatClient(Guid.NewGuid().ToString(), null, loggerFactory);
        });

        var dbPath = Path.Combine(appLocation, "squiggle_history.db");
        services.AddSingleton(new HistoryManager($"Data Source={dbPath}"));

        // Plugins
        var pluginsPath = Path.Combine(appLocation, "Plugins");
#pragma warning disable IL2026 // RequiresUnreferencedCode - plugin loading is inherently reflection-based
        var pluginLoader = new PluginLoader(pluginsPath);
#pragma warning restore IL2026
        services.AddSingleton(pluginLoader);

        // Platform services
        services.AddSingleton<ITrayIconService, AvaloniaTrayIconService>();
        services.AddSingleton<INotificationService, AvaloniaNotificationService>();
        services.AddSingleton<IDialogService, AvaloniaDialogService>();
        services.AddSingleton<IClipboardService, AvaloniaClipboardService>();
        services.AddSingleton<IAutoStartService, WindowsAutoStartService>();

        return services.BuildServiceProvider();
    }
}
