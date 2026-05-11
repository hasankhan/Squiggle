using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Squiggle.UI.Avalonia.Windows;

namespace Squiggle.UI.Avalonia.Services;

public class AvaloniaDialogService : IDialogService
{
    private Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;
        return null;
    }

    private IStorageProvider? GetStorageProvider()
    {
        var window = GetMainWindow();
        return window != null ? TopLevel.GetTopLevel(window)?.StorageProvider : null;
    }

    public async Task<IStorageFile?> OpenFileAsync(string title, params FilePickerFileType[] filters)
    {
        var provider = GetStorageProvider();
        if (provider == null)
            return null;

        var results = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = filters.Length > 0 ? filters : null,
        });

        return results.FirstOrDefault();
    }

    public async Task<IStorageFile?> SaveFileAsync(string title, string? defaultFileName, params FilePickerFileType[] filters)
    {
        var provider = GetStorageProvider();
        if (provider == null)
            return null;

        return await provider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = defaultFileName,
            FileTypeChoices = filters.Length > 0 ? filters : null,
        });
    }

    public async Task<IStorageFolder?> OpenFolderAsync(string title)
    {
        var provider = GetStorageProvider();
        if (provider == null)
            return null;

        var results = await provider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
        });

        return results.FirstOrDefault();
    }

    public async Task<MessageBoxResult> ShowMessageBoxAsync(string title, string message, MessageBoxButton buttons = MessageBoxButton.Ok)
    {
        var owner = GetMainWindow();
        if (owner == null)
            return MessageBoxResult.None;

        var dialog = new MessageBoxWindow(title, message, buttons);
        return await dialog.ShowDialog<MessageBoxResult>(owner);
    }
}
