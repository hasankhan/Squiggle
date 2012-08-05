using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using MessageMapping = System.Collections.Generic.KeyValuePair<Squiggle.UI.StickyWindow.NativeMethods.WindowMessage,Squiggle.UI.StickyWindow.NativeMethods.MessageHandler>;

namespace Squiggle.UI.StickyWindow
{
    public class SnapToBehavior : NativeBehavior
    {
        System.Windows.Point snapPoint;
        Window originalForm;

        public Window OriginalForm
        {
            get { return originalForm; }
            set { originalForm = value; }
        }
        
        public override IEnumerable<MessageMapping> GetHandlers()
        {
            yield return new MessageMapping(NativeMethods.WindowMessage.Moving, OnMoving);
            yield return new MessageMapping(NativeMethods.WindowMessage.EnterSizeMove, OnEnterSizeMove);
        }

        IntPtr OnEnterSizeMove(IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            var rectangle = new Rectangle((int)OriginalForm.Left, (int)OriginalForm.Top, (int)OriginalForm.Width, (int)OriginalForm.Height);
            snapPoint = NativeMethods.GetMousePosition();
            snapPoint.X = snapPoint.X - rectangle.Left;
            snapPoint.Y = snapPoint.Y - rectangle.Top;

            return IntPtr.Zero;
        }

        //http://www.codeproject.com/KB/winsdk/snapping_window.aspx
        IntPtr OnMoving(IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            var snap_prc = (NativeMethods.WindowPosition)Marshal.PtrToStructure(lParam, typeof(NativeMethods.WindowPosition));
            var snap_cur_pos = NativeMethods.GetMousePosition();
            OffsetRect(ref snap_prc, snap_cur_pos.X - (snap_prc.Left + snapPoint.X),
                                     snap_cur_pos.Y - (snap_prc.Top + snapPoint.Y));

            Screen screen = Screen.FromPoint(new System.Drawing.Point((int)snap_cur_pos.X, (int)snap_cur_pos.Y));
            Rect snap_wa = new Rect(screen.WorkingArea.Left, screen.WorkingArea.Top, screen.WorkingArea.Width, screen.WorkingArea.Height);

            if (IsSnapClose(snap_prc.Left, snap_wa.Left, SnapDistance))
                OffsetRect(ref snap_prc, snap_wa.Left - snap_prc.Left, 0);
            else if (IsSnapClose(snap_wa.Right, snap_prc.Right, SnapDistance))
                OffsetRect(ref snap_prc, snap_wa.Right - snap_prc.Right, 0);

            if (IsSnapClose(snap_prc.Top, snap_wa.Top, SnapDistance))
                OffsetRect(ref snap_prc, 0, snap_wa.Top - snap_prc.Top);
            else if (IsSnapClose(snap_wa.Bottom, snap_prc.Bottom, SnapDistance))
                OffsetRect(ref snap_prc, 0, snap_wa.Bottom - snap_prc.Bottom);

            Marshal.StructureToPtr(snap_prc, lParam, true);

            return IntPtr.Zero;
        }

        void OffsetRect(ref NativeMethods.WindowPosition rect, double x, double y)
        {
            rect.Left += (int)x;
            rect.Right += (int)x;
            rect.Top += (int)y;
            rect.Bottom += (int)y;
        }

        bool IsSnapClose(double a, double b, double margin)
        {
            return (Math.Abs(a - b) < margin);
        }
        
        #region Additional Dependency Properties
        public static readonly DependencyProperty SnapDistanceProperty =
            DependencyProperty.Register("SnapDistance", typeof(int), typeof(SnapToBehavior), new UIPropertyMetadata(20));

        public static readonly DependencyProperty WindowListProperty =
                    DependencyProperty.Register("WindowList", typeof(List<Window>), typeof(SnapToBehavior), new UIPropertyMetadata(new List<Window>()));

        public List<Window> WindowList
        {
            get
            {
                if (WindowListProperty == null)
                    SetValue(WindowListProperty, new List<Window>());

                return (List<Window>)GetValue(WindowListProperty);
            }

            set { SetValue(WindowListProperty, value); }
        }

        public int SnapDistance
        {
            get { return (int)GetValue(SnapDistanceProperty); }
            set { SetValue(SnapDistanceProperty, value); }
        }
        #endregion Additional Dependency Properties
    }
}
