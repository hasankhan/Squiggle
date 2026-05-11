using System;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Squiggle.Client;
using Squiggle.UI.Controls;
using Squiggle.UI.Services;
using Squiggle.UI.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace Squiggle.UI;

public partial class MainWindow : Window
{
    private ClientViewModel? _viewModel;
    private ITrayIconService? _trayIconService;
    private INotificationService? _notificationService;
    private bool _forceClose;

    public MainWindow()
    {
        InitializeComponent();

        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        var chatClient = App.Services.GetRequiredService<IChatClient>();
        _viewModel = new ClientViewModel(chatClient);
        DataContext = _viewModel;

        signInControl.LoginRequested += SignInControl_LoginRequested;

        InitializeTrayIcon();
        InitializeNotifications();
    }

    private void InitializeTrayIcon()
    {
        _trayIconService = App.Services.GetRequiredService<ITrayIconService>();
        _trayIconService.ShowTrayIcon();
        _trayIconService.TrayIconClicked += (_, _) => ShowAndActivate();

        if (_trayIconService is AvaloniaTrayIconService avaloniaTray)
        {
            avaloniaTray.SignOutRequested += (_, _) => SignOutMenu_Click(this, new RoutedEventArgs());
            avaloniaTray.ExitRequested += (_, _) =>
            {
                _forceClose = true;
                Close();
            };
        }
    }

    private void InitializeNotifications()
    {
        _notificationService = App.Services.GetRequiredService<INotificationService>();
        _notificationService.Initialize(this);
    }

    private void ShowAndActivate()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (_forceClose)
            return;

        // Minimize to tray instead of closing
        e.Cancel = true;
        Hide();
    }

    private async void SignInControl_LoginRequested(object? sender, LoginEventArgs e)
    {
        var chatClient = App.Services.GetRequiredService<IChatClient>();
        try
        {
            await System.Threading.Tasks.Task.CompletedTask;
        }
        catch (Exception)
        {
            var dialogService = App.Services.GetRequiredService<IDialogService>();
            await dialogService.ShowMessageBoxAsync("Error", "Failed to sign in. Please try again.");
        }
    }

    private void SignOutMenu_Click(object? sender, RoutedEventArgs e)
    {
        var chatClient = App.Services.GetRequiredService<IChatClient>();
        if (chatClient.IsLoggedIn)
            chatClient.Logout();
    }

    private void CloseMenu_Click(object? sender, RoutedEventArgs e)
    {
        _forceClose = true;
        Close();
    }

    private async void SettingsMenu_Click(object? sender, RoutedEventArgs e)
    {
        var settingsWindow = new Windows.SettingsWindow();
        await settingsWindow.ShowDialog(this);
    }

    private async void HistoryMenu_Click(object? sender, RoutedEventArgs e)
    {
        var viewer = new Windows.HistoryViewer();
        await viewer.ShowDialog(this);
    }

    private void AboutMenu_Click(object? sender, RoutedEventArgs e)
    {
        // About dialog - will be implemented later
    }
}
