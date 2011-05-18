using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

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
            if ((this.Left + Width) > System.Windows.SystemParameters.VirtualScreenWidth)
                this.Left = System.Windows.SystemParameters.VirtualScreenWidth - Width;
            else if (this.Left < 0)
                this.Left = 0;

            if ((this.Top + Height) > System.Windows.SystemParameters.VirtualScreenHeight)
                this.Top = System.Windows.SystemParameters.VirtualScreenHeight - Height;
            else if (this.Top < 0)
                this.Top = 0;
        }    
    }
}
