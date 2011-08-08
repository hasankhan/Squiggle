using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;

namespace Squiggle.UI.StickyWindows
{
    public class StickyWindow: Window
    {
        bool loaded;

        public StickyWindow()
        {            
            this.Loaded += new RoutedEventHandler(StickyWindow_Loaded);
        }

        void StickyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustLocation();
            if (loaded)
                return;

            WindowManager.RegisterWindow(this);
            var nativeBehaviors = new NativeBehaviors(this);
            var snapBehavior = new SnapToBehavior();
            snapBehavior.OriginalForm = this;
            nativeBehaviors.Add(snapBehavior);
            loaded = true;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            WindowManager.UnregisterWindow(this);

            base.OnClosing(e);
        }

        void AdjustLocation()
        {
            var bottomRight = new System.Drawing.Point((int)this.Left + (int)this.Width, (int)this.Top + (int)this.Height);
            var topLeft = new System.Drawing.Point((int)this.Left, (int)this.Top);
            var screen = Screen.FromPoint(topLeft);

            if (bottomRight.X > screen.WorkingArea.Right)
                this.Left = screen.WorkingArea.Right - this.Width;
            else if (this.Left < screen.WorkingArea.Left)
                this.Left = screen.WorkingArea.Left;

            if (bottomRight.Y > screen.WorkingArea.Bottom)
                this.Top = screen.WorkingArea.Bottom - this.Height;
            else if (this.Top < screen.WorkingArea.Top)
                this.Top = screen.WorkingArea.Top;
        }    
    }
}
