using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using Squiggle.UI.Avalonia.Services;
using Squiggle.UI.Avalonia.ViewModel;

namespace Squiggle.UI.Avalonia.Windows;

public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _viewModel;

    public SettingsWindow()
    {
        InitializeComponent();
        _viewModel = new SettingsViewModel();
        DataContext = _viewModel;
    }

    public SettingsWindow(SettingsViewModel viewModel) : this()
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        var themeService = App.Services.GetRequiredService<ThemeService>();
        themeService.ApplyTheme(_viewModel.GeneralSettings.ThemeMode);
        Close(true);
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private async void BrowseFolder_Click(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Downloads Folder",
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            _viewModel.GeneralSettings.DownloadsFolder = folders[0].Path.LocalPath;
        }
    }
}
