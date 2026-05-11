using Avalonia.Controls;
using Avalonia.Controls.Notifications;

namespace Squiggle.UI.Services;

public class AvaloniaNotificationService : INotificationService
{
    private WindowNotificationManager? _notificationManager;

    public void Initialize(Window mainWindow)
    {
        _notificationManager = new WindowNotificationManager(mainWindow)
        {
            Position = NotificationPosition.BottomRight,
            MaxItems = 3,
        };
    }

    public void ShowNotification(string title, string message)
    {
        _notificationManager?.Show(new Notification(
            title,
            message,
            NotificationType.Information));
    }

    public void ShowMessageNotification(string senderName, string messagePreview)
    {
        _notificationManager?.Show(new Notification(
            senderName,
            messagePreview,
            NotificationType.Information));
    }
}
