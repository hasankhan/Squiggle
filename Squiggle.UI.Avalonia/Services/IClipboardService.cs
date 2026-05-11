using System.Threading.Tasks;

namespace Squiggle.UI.Avalonia.Services;

public interface IClipboardService
{
    Task CopyTextAsync(string text);
    Task<string?> GetTextAsync();
    Task<bool> ContainsTextAsync();
}
