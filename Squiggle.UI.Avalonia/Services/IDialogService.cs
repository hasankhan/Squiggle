using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace Squiggle.UI.Avalonia.Services;

public enum MessageBoxButton
{
    Ok,
    OkCancel,
    YesNo,
}

public enum MessageBoxResult
{
    None,
    Ok,
    Cancel,
    Yes,
    No,
}

public interface IDialogService
{
    Task<IStorageFile?> OpenFileAsync(string title, params FilePickerFileType[] filters);
    Task<IStorageFile?> SaveFileAsync(string title, string? defaultFileName = null, params FilePickerFileType[] filters);
    Task<IStorageFolder?> OpenFolderAsync(string title);
    Task<MessageBoxResult> ShowMessageBoxAsync(string title, string message, MessageBoxButton buttons = MessageBoxButton.Ok);
}
