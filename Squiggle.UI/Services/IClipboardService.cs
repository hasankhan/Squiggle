using System.Threading.Tasks;

namespace Squiggle.UI.Services;

public interface IClipboardService
{
    Task CopyTextAsync(string text);
    Task<string?> GetTextAsync();
    Task<bool> ContainsTextAsync();
}
