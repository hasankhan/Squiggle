using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using MessageMapping = System.Collections.Generic.KeyValuePair<Squiggle.UI.StickyWindows.NativeMethods.WindowMessage,Squiggle.UI.StickyWindows.NativeMethods.MessageHandler>;

namespace Squiggle.UI.StickyWindows
{
    public class NativeBehaviors : ObservableCollection<NativeBehavior>
    {
        /// <summary>The HWND handle to our window</summary>
        protected IntPtr WindowHandle { get; private set; }
        /// <summary>Gets the collection of active handlers.</summary>
        /// <value>A List of the mappings from <see cref="NativeMethods.WindowMessage"/>s
        /// to <see cref="NativeMethods.MessageHandler"/> delegates.</value>
        protected List<MessageMapping> Handlers { get; set; }
        /// <summary>
        /// The reference to the Target or "owner" window 
        /// should be accessed through the <see cref="NativeBehaviors.Window"/> property.
        /// </summary>
        private WeakReference _owner;
        /// <summary>Gets or Sets the target/owner window</summary>
        /// <value>The <see cref="Window"/> these Native Behavrios affect.</value>
        public Window Target
        {
            get
            {
                if (_owner != null)
                {
                    return _owner.Target as Window;
                }
                else return null;
            }
            set
            {
                // design mode bailout (in Design mode there's no window, and no wndproc)
                if (DesignerProperties.GetIsInDesignMode(value)) { return; }

                if (_owner != null && WindowHandle != IntPtr.Zero)
                {
                    HwndSource.FromHwnd(WindowHandle).RemoveHook(WndProc);
                }

                Debug.Assert(null != value);
                _owner = new WeakReference(value);

                // Use whether we can get an HWND to determine if the Window has been loaded.
                WindowHandle = new WindowInteropHelper(value).Handle;


                if (IntPtr.Zero == WindowHandle)
                {
                    value.SourceInitialized += (sender, e) =>
                    {
                        WindowHandle = new WindowInteropHelper((Window)sender).Handle;
                        HwndSource.FromHwnd(WindowHandle).AddHook(WndProc);
                    };
                }
                else
                {
                    HwndSource.FromHwnd(WindowHandle).AddHook(WndProc);
                }
            }
        }

        /// <summary>Initializes a new instance of the <see cref="NativeBehaviors"/> class
        /// with no behaviors and no owner window.
        /// </summary>
        /// <remarks>We need this constructor for DesignMode support</remarks>
        public NativeBehaviors() { Handlers = new List<MessageMapping>(); }

        /// <summary>Initializes a new instance of the <see cref="NativeBehaviors"/> class
        /// with the specified target <see cref="Window"/> 
        /// and <em>no</em> <see cref="NativeBehavior"/>s.
        /// </summary>
        /// <param name="target">The Window to be affected by this collection of behaviors</param>
        public NativeBehaviors(Window target)
        {
            Handlers = new List<MessageMapping>();
            Target = target;
            target.SetValue(NativeBehaviorsProperty, this);
        }

        ///// <summary>Initializes a new instance of the <see cref="NativeBehaviors"/> class
        ///// with the specified target <see cref="Window"/> 
        ///// and <see cref="NativeBehavior"/>s.
        ///// </summary>
        ///// <param name="target">The Window to be affected by this collection of behaviors</param>
        ///// <param name="behaviors">The NativeBehaviors</param>
        //public NativeBehaviors(Window target, NativeBehaviors behaviors)
        //{
        //   Handlers = new List<MessageMapping>();
        //   Window = target;
        //   target.SetValue(NativeBehaviorsProperty, behaviors);
        //}

        /// <summary>
        /// A Window Process Message Handler delegate
        /// which processes all the registered message mappings
        /// </summary>
        /// <param name="hwnd">The window handle.</param>
        /// <param name="msg">The message.</param>
        /// <param name="wParam">The wParam.</param>
        /// <param name="lParam">The lParam.</param>
        /// <param name="handled">Set to true if the message has been handled</param>
        /// <returns>IntPtr.Zero</returns>
        [DebuggerNonUserCode]
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            Debug.Assert(hwnd == WindowHandle); // Only expecting messages for our cached HWND.

            // cast and cache the message
            var message = (NativeMethods.WindowMessage)msg;
            // NOTE: we may process a message multiple times
            // and we have no good way to handle that...
            var result = IntPtr.Zero;
            foreach (var handlePair in Handlers)
            {
                if (handlePair.Key == message)
                {
                    var r = handlePair.Value(wParam, lParam, ref handled);
                    // So, we'll return the last non-zero result (if any)
                    if (r != IntPtr.Zero) { result = r; }
                }
            }
            return result;
        }


        #region The Attached DependencyProperty
        /// <summary>
        /// The Behaviors DependencyProperty is the collection of WindowMessage-based behaviors
        /// </summary>
        /// <remarks>
        /// Making the DependencyProperty Private or Internal means that the XAML parser can't see it.
        /// However, the XAML parser *can* see the Public "GetBehaviors" and/or "SetBehaviors" methods
        /// So when you use <code><![CDATA[<wpf:Native.Behaviors />]]></code> in XAML, it will use the 
        /// GetBehaviors method, which gives us the opportunity to initialize the collection -- this 
        /// will not work if you used the name of a public DependencyProperty.
        /// </remarks>
        /// <example><![CDATA[
        /// <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        ///     xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        ///     xmlns:huddled="http://schemas.huddledmasses.org/wpf"
        ///     >
        ///     <huddled:Behaviors.Behaviors>
        ///         <huddled:SnappingWindow SnapDistance="40" />
        ///     </huddled:Behaviors.Behaviors>
        ///     <Grid><Label Content="Drag this window near the screen edges"/></Grid>
        /// </Window>
        /// ]]></example>
        private static readonly DependencyProperty NativeBehaviorsProperty = DependencyProperty.RegisterAttached(
            "NativeBehaviors", typeof(NativeBehaviors), typeof(NativeBehaviors),
            new FrameworkPropertyMetadata(null));

        #region The PUBLIC accessors which wrap the hidden dependency property
        /// <summary>Sets the behaviors.</summary>
        /// <param name="window">The window.</param>
        /// <param name="behaviors">The collection of <see cref="NativeBehavior"/>s.</param>
        public static void SetBehaviors(Window window, NativeBehaviors behaviors)
        {
            if (window == null)
            {
                throw new ArgumentNullException("window");
            }
            window.SetValue(NativeBehaviorsProperty, behaviors);
        }

        /// <summary>Gets the behaviors.</summary>
        /// <param name="window">The window.</param>
        /// <returns>The collection of <see cref="NativeBehavior"/>s.</returns>
        public static NativeBehaviors GetBehaviors(Window window)
        {
            return GetNativeBehaviors(window);
        }

        public static IEnumerable<TBehavior> SelectBehaviors<TBehavior>(Window window) where TBehavior : NativeBehavior
        {
            foreach (var behavior in NativeBehaviors.GetBehaviors(window))
            {
                if (behavior is TBehavior)
                {
                    yield return (TBehavior)behavior;
                }
            }
        }
        #endregion
        /// <summary>Gets the behaviors.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <returns></returns>
        private static NativeBehaviors GetNativeBehaviors(Window window)
        {
            if (window == null) { throw new ArgumentNullException("window"); }
            // This is the plain old normal thing:
            var behaviors = (NativeBehaviors)window.GetValue(NativeBehaviorsProperty);
            // Our raison d'être: create a new collection if there isn't one yet
            if (behaviors == null) { behaviors = new NativeBehaviors(window); }

            return behaviors;
        }

        /// <summary>
        /// Handles changes to the NativeBehaviors collection, invoking the <see cref="NativeBehavior.AddTo"/>
        /// and <see cref="NativeBehavior.RemoveFrom"/> methods, and adding their handlers to the list.
        /// </summary>
        /// <param name="nccea"></param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs nccea)
        {
            base.OnCollectionChanged(nccea);
            // design mode bailout because NativeBehaviors don't work in DesignMode
            if (Target == null || DesignerProperties.GetIsInDesignMode(Target)) { return; }
            // notify new behaviors they are being hooked up, and track their handlers
            if (nccea.Action == NotifyCollectionChangedAction.Add ||
                nccea.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (NativeBehavior behavior in nccea.NewItems)
                {
                    behavior.AddTo(Target);
                    Handlers.AddRange(behavior.GetHandlers());
                }
            }

            // notify removed behaviors they are being unhooked, and stop tracking their handlers
            if (nccea.Action == NotifyCollectionChangedAction.Remove ||
                nccea.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (NativeBehavior behavior in nccea.OldItems)
                {
                    behavior.RemoveFrom(Target);
                    foreach (var h in behavior.GetHandlers())
                    {
                        Handlers.Remove(h);
                    }
                }
            }
        }

        #endregion

    }
}