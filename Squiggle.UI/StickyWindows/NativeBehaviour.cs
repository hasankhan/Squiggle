using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
//using Huddled.Interop;
using MessageMapping = System.Collections.Generic.KeyValuePair<Squiggle.UI.StickyWindows.NativeMethods.WindowMessage,Squiggle.UI.StickyWindows.NativeMethods.MessageHandler>;

namespace Squiggle.UI.StickyWindows
{

    /// <summary>A behavior based on hooking a window message</summary>
    public abstract class NativeBehavior : DependencyObject
    {
        /// <summary>
        /// Called when this behavior is initially hooked up to an initialized <see cref="System.Windows.Window"/>
        /// <see cref="NativeBehavior"/> implementations may override this to perfom actions
        /// on the actual window (the Chrome behavior uses this to change the template)
        /// </summary>
        /// <remarks>Implementations should NOT depend on this being exectued before 
        /// the Window is SourceInitialized, and should use a WeakReference if they need 
        /// to keep track of the window object...
        /// </remarks>
        /// <param name="window"></param>
        virtual public void AddTo(Window window) { }

        /// <summary>
        /// Called when this behavior is unhooked from a <see cref="System.Windows.Window"/>
        /// <see cref="NativeBehavior"/> implementations may override this to perfom actions
        /// on the actual window.
        /// </summary>
        /// <param name="window"></param>
        virtual public void RemoveFrom(Window window) { }

        /// <summary>
        /// Gets the <see cref="MessageMapping"/>s for this behavior 
        /// (one for each Window Message you need to handle)
        /// </summary>
        /// <value>A collection of <see cref="MessageMapping"/> objects.</value>
        public abstract IEnumerable<MessageMapping> GetHandlers();
    }
}