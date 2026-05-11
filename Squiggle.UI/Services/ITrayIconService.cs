using System;
using Squiggle.Core.Presence;

namespace Squiggle.UI.Services;

public interface ITrayIconService
{
    event EventHandler? TrayIconClicked;

    void ShowTrayIcon();
    void HideTrayIcon();
    void SetTooltip(string tooltip);
    void SetStatusIcon(UserStatus status);
}
