namespace Squiggle.UI.Avalonia.Services;

public interface IAutoStartService
{
    bool IsEnabled { get; }
    void Enable();
    void Disable();
}
