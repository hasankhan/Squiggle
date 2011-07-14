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
        public StickyWindow()
        {
            WindowManager.RegisterWindow(this);
            var nativeBehaviors = new NativeBehaviors(this);
            var snapBehavior = new SnapToBehavior();
            snapBehavior.OriginalForm = this;
            nativeBehaviors.Add(snapBehavior);
            this.Loaded += new RoutedEventHandler(StickyWindow_Loaded);
        }

        void StickyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustLocation();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            WindowManager.UnregisterWindow(this);

            base.OnClosing(e);
        }

        void AdjustLocation()
        {
            var bottomLeft = new System.Drawing.Point((int)this.Left + (int)this.Width, (int)this.Top + (int)this.Height);
            var screen = Screen.FromPoint(bottomLeft);

            if (bottomLeft.X > screen.WorkingArea.Width)
                this.Left = screen.WorkingArea.Width - Width;
            else if (this.Left < screen.WorkingArea.X)
                this.Left = screen.WorkingArea.X;

            if (bottomLeft.Y > screen.WorkingArea.Height)
                this.Top = screen.WorkingArea.Height - Height;
            else if (this.Top < screen.WorkingArea.Y)
                this.Top = screen.WorkingArea.Y;
        }    
    }
}
