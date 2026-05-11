using Avalonia.Controls;
using Avalonia.Interactivity;
using Squiggle.Client;
using Squiggle.UI.Avalonia.Controls;
using Squiggle.UI.Avalonia.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace Squiggle.UI.Avalonia;

public partial class MainWindow : Window
{
    private ClientViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        var chatClient = App.Services.GetRequiredService<IChatClient>();
        _viewModel = new ClientViewModel(chatClient);
        DataContext = _viewModel;

        signInControl.LoginRequested += SignInControl_LoginRequested;
    }

    private async void SignInControl_LoginRequested(object? sender, LoginEventArgs e)
    {
        var chatClient = App.Services.GetRequiredService<IChatClient>();
        try
        {
            // Login logic will be connected when chat infrastructure is wired
            await System.Threading.Tasks.Task.CompletedTask;
        }
        catch (System.Exception)
        {
            // Show error - will be implemented with proper dialog support
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
        Close();
    }

    private void SettingsMenu_Click(object? sender, RoutedEventArgs e)
    {
        // Settings window will be implemented in #52
    }

    private void AboutMenu_Click(object? sender, RoutedEventArgs e)
    {
        // About dialog - will be implemented later
    }
}
