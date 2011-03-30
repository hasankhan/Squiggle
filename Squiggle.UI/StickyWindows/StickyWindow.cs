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
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            WindowManager.UnregisterWindow(this);

            base.OnClosing(e);
        }
    }
}
