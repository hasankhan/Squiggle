using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;

namespace Squiggle.UI.Avalonia.Services;

public class AvaloniaClipboardService : IClipboardService
{
    private IClipboard? GetClipboard()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return TopLevel.GetTopLevel(desktop.MainWindow)?.Clipboard;
        return null;
    }

    public async Task CopyTextAsync(string text)
    {
        var clipboard = GetClipboard();
        if (clipboard != null)
            await clipboard.SetTextAsync(text);
    }

    public async Task<string?> GetTextAsync()
    {
        var clipboard = GetClipboard();
        if (clipboard != null)
            return await clipboard.GetTextAsync();
        return null;
    }

    public async Task<bool> ContainsTextAsync()
    {
        var text = await GetTextAsync();
        return !string.IsNullOrEmpty(text);
    }
}
