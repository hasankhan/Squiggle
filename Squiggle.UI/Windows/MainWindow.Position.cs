using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;

namespace Squiggle.UI.Windows
{
    partial class MainWindow
    {
        class PositionHelper
        {
            Window window;
            Properties.Settings settings;

            public PositionHelper(Window parent, Properties.Settings settings)
            {
                this.window = parent;
                this.settings = settings;
            }

            public void Initialize()
            {
                window.Height = settings.MainWindowHeight;
                window.Width = settings.MainWindowWidth;

                window.Top = settings.MainWindowTop >= 0 ? settings.MainWindowTop : Screen.PrimaryScreen.WorkingArea.Height / 2 - window.Height / 2;
                window.Left = settings.MainWindowLeft >= 0 ? settings.MainWindowLeft : Screen.PrimaryScreen.WorkingArea.Width / 2 - window.Width / 2;

                window.LocationChanged += Window_LocationChanged;
                window.SizeChanged += Window_SizeChanged;
            }

            private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
            {
                if (window.WindowState == WindowState.Normal && window.Height > 0 && window.Width > 0)
                {
                    settings.MainWindowHeight = window.Height;
                    settings.MainWindowWidth = window.Width;
                    settings.Save();
                }
            }

            private void Window_LocationChanged(object sender, EventArgs e)
            {
                if (window.WindowState == WindowState.Normal && window.Top >= 0 && window.Left >= 0)
                {
                    settings.MainWindowTop = window.Top;
                    settings.MainWindowLeft = window.Left;
                    settings.Save();
                }                
            }
        }
    }
}
