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

            if (bottomLeft.X > screen.Bounds.Right)
                this.Left = screen.Bounds.Right - Width;
            else if (this.Left < screen.Bounds.Left)
                this.Left = screen.Bounds.Left;

            if (bottomLeft.Y > screen.Bounds.Bottom)
                this.Top = screen.Bounds.Bottom - Height;
            else if (this.Top < screen.Bounds.Top)
                this.Top = screen.Bounds.Top;
        }    
    }
}
