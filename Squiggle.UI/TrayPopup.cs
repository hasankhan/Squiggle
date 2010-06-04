using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using StackOverflowClient;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Squiggle.UI
{
    class TrayPopup
    {
        public static void Show(string title, string message)
        {
            Show(title, message, _ => { });
        }

        public static void Show(string title, string message, Action<MouseEventArgs> onClick)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                int timeout = 5000;
                FancyBalloon balloon = new FancyBalloon(timeout);
                balloon.BalloonText = title;
                balloon.DataContext = message;
                Hardcodet.Wpf.TaskbarNotification.TaskbarIcon icon = new Hardcodet.Wpf.TaskbarNotification.TaskbarIcon();
                icon.Visibility = Visibility.Hidden;
                icon.ShowCustomBalloon(balloon, PopupAnimation.Slide, timeout);
                balloon.MouseLeftButtonDown += (sender, e) =>
                {
                    e.Handled = false;
                    onClick(e);
                };
            });
        }
    }
}
