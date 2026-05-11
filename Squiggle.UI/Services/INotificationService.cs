using Avalonia.Controls;

namespace Squiggle.UI.Services;

public interface INotificationService
{
    void Initialize(Window mainWindow);
    void ShowNotification(string title, string message);
    void ShowMessageNotification(string senderName, string messagePreview);
}
