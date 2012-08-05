using System;
using System.ComponentModel;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Windows;
using Point = System.Windows.Point;
using Rect = System.Windows.Rect;

namespace Squiggle.UI.StickyWindow
{
    public static partial class NativeMethods
    {
        /// <summary>
        /// Provides the enumeration values for calls to <see cref="NativeMethods.ShowWindow"/> or <see cref="NativeMethods.ShowWindowAsync"/>
        /// </summary>
        public enum ShowWindowCommand : int
        {
            /// <summary>
            /// Hides the Window and activates another Window.
            /// </summary>
            Hide = 0,
            /// <summary>
            /// Activates and displays a Window. If the Window is minimized or 
            /// maximized, the system restores it to its original size and position.
            /// An application should specify this flag when displaying the Window 
            /// for the first time.
            /// </summary>
            Normal = 1,
            /// <summary>
            /// Activates the Window and displays it as a minimized Window.
            /// </summary>
            ShowMinimized = 2,
            /// <summary>
            /// Maximizes the specified Window.
            /// </summary>
            Maximize = 3, // is this the right value?
            /// <summary>
            /// Activates the Window and displays it as a maximized Window.
            /// </summary>       
            ShowMaximized = 3,
            /// <summary>
            /// Displays a Window in its most recent size and position. This value 
            /// is similar to <see cref="ShowWindowCommand.Normal"/>, except 
            /// the Window is not actived.
            /// </summary>
            ShowNoActivate = 4,
            /// <summary>
            /// Activates the Window and displays it in its current size and position. 
            /// </summary>
            Show = 5,
            /// <summary>
            /// Minimizes the specified Window and activates the next top-level 
            /// Window in the Z order.
            /// </summary>
            Minimize = 6,
            /// <summary>
            /// Displays the Window as a minimized Window. This value is similar to
            /// <see cref="ShowWindowCommand.ShowMinimized"/>, except the 
            /// Window is not activated.
            /// </summary>
            ShowMinNoActive = 7,
            /// <summary>
            /// Displays the Window in its current size and position. This value is 
            /// similar to <see cref="ShowWindowCommand.Show"/>, except the 
            /// Window is not activated.
            /// </summary>
            ShowNA = 8,
            /// <summary>
            /// Activates and displays the Window. If the Window is minimized or 
            /// maximized, the system restores it to its original size and position. 
            /// An application should specify this flag when restoring a minimized Window.
            /// </summary>
            Restore = 9,
            /// <summary>
            /// Sets the show state based on the SW_* value specified in the 
            /// STARTUPINFO structure passed to the CreateProcess function by the 
            /// program that started the application.
            /// </summary>
            ShowDefault = 10,
            /// <summary>
            ///  <b>Windows 2000/XP:</b> Minimizes a Window, even if the thread 
            /// that owns the Window is not responding. This flag should only be 
            /// used when minimizing windows from a different thread.
            /// </summary>
            ForceMinimize = 11
        }

        /// <summary>
        /// Provides the enumeration values for calls to <see cref="NativeMethods.GetWindow"/>
        /// </summary>
        public enum GetWindowCommand : int
        {
            /// <summary>
            /// The retrieved handle identifies the Window of the same type that is highest in the Z order. If the specified Window is a topmost Window, the handle identifies the topmost Window that is highest in the Z order. If the specified Window is a top-level Window, the handle identifies the top-level Window that is highest in the Z order. If the specified Window is a child Window, the handle identifies the sibling Window that is highest in the Z order.
            /// </summary>
            First = 0,
            /// <summary>
            /// The retrieved handle identifies the Window of the same type that is lowest in the Z order. If the specified Window is a topmost Window, the handle identifies the topmost Window that is lowest in the Z order. If the specified Window is a top-level Window, the handle identifies the top-level Window that is lowest in the Z order. If the specified Window is a child Window, the handle identifies the sibling Window that is lowest in the Z order.
            /// </summary>
            Last = 1,
            /// <summary>
            /// The retrieved handle identifies the Window below the specified Window in the Z order. If the specified Window is a topmost Window, the handle identifies the topmost Window below the specified Window. If the specified Window is a top-level Window, the handle identifies the top-level Window below the specified Window. If the specified Window is a child Window, the handle identifies the sibling Window below the specified Window. 
            /// </summary>
            Next = 2,
            /// <summary>
            /// The retrieved handle identifies the Window above the specified Window in the Z order. If the specified Window is a topmost Window, the handle identifies the topmost Window above the specified Window. If the specified Window is a top-level Window, the handle identifies the top-level Window above the specified Window. If the specified Window is a child Window, the handle identifies the sibling Window above the specified Window.
            /// </summary>
            Previous = 3,
            /// <summary>
            /// The retrieved handle identifies the specified Window's owner Window, if any.
            /// </summary>
            Owner = 4,
            /// <summary>
            /// The retrieved handle identifies the child Window at the top of the Z order, if the specified Window is a parent Window; otherwise, the retrieved handle is NULL. The function examines only child windows of the specified Window. It does not examine descendant windows.
            /// </summary>
            Child = 5,
            /// <summary>
            /// Windows 2000/XP: The retrieved handle identifies the enabled popup Window owned by the specified Window (the search uses the first such Window found using GW_HWNDNEXT); otherwise, if there are no enabled popup windows, the retrieved handle is that of the specified Window.
            /// </summary>
            Popup = 6
        }

        #region PInvoke Method Signatures
        // It looks like this function is in shell32.dll in Win2k and XP - but not exported pre XP SP1. 
        // We could hypothetically reference it by ordinal number -- should work from Win2K SP4 on.
        // [DllImport("shell32.dll",EntryPoint="#680",CharSet=CharSet.Unicode)]
        [DllImport("shell32.dll", EntryPoint = "IsUserAnAdmin", CharSet = CharSet.Unicode)]
        public static extern bool IsUserAnAdmin();

        //[DllImport("user32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //public static extern bool BringWindowToTop(IntPtr hWnd);



        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);


        #region user32!GetWindow
        /// <summary> The GetWindow function retrieves a handle to a Window 
        /// that has the specified relationship (Z-Order or owner) to the specified Window.
        /// </summary>
        /// <param name="windowHandle">
        /// Handle to a Window. The Window handle retrieved is relative to this Window, 
        /// based on the value of the command parameter.
        /// </param>
        /// <param name="command">
        /// Specifies the relationship between the specified Window and the Window 
        /// whose handle is to be retrieved.
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is a Window handle. 
        /// If no Window exists with the specified relationship to the specified Window, 
        /// the return value is IntPtr.Zero. 
        /// To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("user32", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern IntPtr GetWindow(IntPtr windowHandle, GetWindowCommand command);
        #endregion
        #region user32!ShowWindow
        /// <summary>
        /// The ShowWindow function sets the specified Window's show state.
        /// </summary>
        /// <param name="windowHandle">
        /// Handle to the Window.
        /// </param>
        /// <param name="command">
        /// Specifies how the Window is to be shown. This parameter is ignored 
        /// the first time an application calls <see cref="ShowWindow"/>, if the program that
        /// launched the application provides a <see cref="StartupInfo"/> structure. 
        /// Otherwise, the first time ShowWindow is called, the value should be the value 
        /// obtained by the WinMain function in its nCmdShow parameter.</param>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern bool ShowWindow(IntPtr windowHandle, ShowWindowCommand command);
        #endregion

        #endregion PInvoke Method Signatures

        public delegate IntPtr MessageHandler(IntPtr wParam, IntPtr lParam, ref bool handled);




        /// <summary>Window message values, WM_*
        /// </summary>
        public enum WindowMessage
        {
            NULL = 0x0000,
            Create = 0x0001,
            Destroy = 0x0002,
            Move = 0x0003,
            Moving = 0x0216,
            Size = 0x0005,
            Activate = 0x0006,
            SetFocus = 0x0007,
            KillFocus = 0x0008,
            Enable = 0x000a,
            SetRedraw = 0x000b,
            SetText = 0x000c,
            GetText = 0x000d,
            GetTextLength = 0x000e,
            Paint = 0x000f,
            Close = 0x0010,
            EnterSizeMove = 0x0231,
            QueryEndSession = 0x0011,
            Quit = 0x0012,
            QueryOpen = 0x0013,
            EraseBackground = 0x0014,
            SystemColorChange = 0x0015,

            WindowPositionChanging = 0x0046,
            WindowPositionChanged = 0x0047,

            SetIcon = 0x0080,
            CreateNonClientArea = 0x0081,
            DestroyNonClientArea = 0x0082,
            CalculateNonClientSize = 0x0083,
            HitTestNonClientArea = 0x0084,
            PaintNonClientArea = 0x0085,
            ActivateNonClientArea = 0x0086,
            GetDialogCode = 0x0087,
            SyncPaint = 0x0088,
            NonClientMouseMove = 0x00a0,
            NonClientLeftButtonDown = 0x00a1,
            NonClientLeftButtonUp = 0x00a2,
            NonClientLeftButtonDoubleClick = 0x00a3,
            NonClientRightButtonDown = 0x00a4,
            NonClientRightButtonUp = 0x00a5,
            NonClientRightButtonDoubleClick = 0x00a6,
            NonClientMiddleButtonDown = 0x00a7,
            NonClientMiddleButtonUp = 0x00a8,
            NonClientMiddleButtonDoubleClick = 0x00a9,

            SysKeyDown = 0x0104,
            SysKeyUp = 0x0105,
            SysChar = 0x0106,
            SysDeadChar = 0x0107,
            SysCommand = 0x0112,

            Hotkey = 0x312,

            DwmCompositionChanged = 0x031e,
            User = 0x0400,
            App = 0x8000,
        }

        /// <summary>SetWindowPos options
        /// </summary>
        [Flags]
        public enum WindowPositionFlags
        {
            AsyncWindowPosition = 0x4000,
            DeferErase = 0x2000,
            DrawFrame = 0x0020,
            FrameChanged = 0x0020,
            HideWindow = 0x0080,
            NoActivate = 0x0010,
            NoCopyBits = 0x0100,
            NoMove = 0x0002,
            NoOwnerZorder = 0x0200,
            NoRedraw = 0x0008,
            NoReposition = 0x0200,
            NoSendChanging = 0x0400,
            NoSize = 0x0001,
            NoZorder = 0x0004,
            ShowWindow = 0x0040,
        }


        /// <summary>
        /// Non-client hit test values, HT*
        /// </summary>
        public enum HT
        {
            ERROR = -2,
            TRANSPARENT = -1,
            NOWHERE = 0,
            CLIENT = 1,
            CAPTION = 2,
            SYSMENU = 3,
            GROWBOX = 4,
            MENU = 5,
            HSCROLL = 6,
            VSCROLL = 7,
            MINBUTTON = 8,
            MAXBUTTON = 9,
            LEFT = 10,
            RIGHT = 11,
            TOP = 12,
            TOPLEFT = 13,
            TOPRIGHT = 14,
            BOTTOM = 15,
            BOTTOMLEFT = 16,
            BOTTOMRIGHT = 17,
            BORDER = 18,
            OBJECT = 19,
            CLOSE = 20,
            HELP = 21
        }

        /// <summary>
        /// GetWindowLongPtr values, GWL_*
        /// </summary>
        public enum WindowLongValues
        {
            WndProc = (-4),
            hInstance = (-6),
            HwndParent = (-8),
            Id = (-12),
            Style = (-16),
            ExtendedStyle = (-20),
            UserData = (-21),
        }

        /// <summary>
        /// SystemParameterInfo values, SPI_*
        /// </summary>
        public enum SPI
        {
            GETNONCLIENTMETRICS = 41,
        }

        /// <summary>
        /// WindowStyles values, WS_*
        /// </summary>
        [Flags]
        public enum WindowStyles : uint
        {
            Overlapped = 0x00000000,
            Popup = 0x80000000,
            Child = 0x40000000,
            Minimize = 0x20000000,
            Visible = 0x10000000,
            Disabled = 0x08000000,
            ClipSiblings = 0x04000000,
            ClipChildren = 0x02000000,
            Maximize = 0x01000000,
            Border = 0x00800000,
            DialogFrame = 0x00400000,
            VerticalScroll = 0x00200000,
            HorizontalScroll = 0x00100000,
            SystemMenu = 0x00080000,
            ThickFrame = 0x00040000,
            Group = 0x00020000,
            TabStop = 0x00010000,

            MinimizeBox = 0x00020000,
            MaximizeBox = 0x00010000,

            Caption = Border | DialogFrame,
            Tiled = Overlapped,
            Iconic = Minimize,
            SizeBox = ThickFrame,
            TiledWindow = OverlappedWindow,

            OverlappedWindow = Overlapped | Caption | SystemMenu | ThickFrame | MinimizeBox | MaximizeBox,
            PopupWindow = Popup | Border | SystemMenu,
            ChildWindow = Child,
        }

        /// <summary>
        /// Window style extended values, WS_EX_*
        /// </summary>
        [Flags]
        public enum ExtendedWindowStyles : uint
        {
            None = 0,
            DLGMODALFRAME = 0x00000001,
            NOPARENTNOTIFY = 0x00000004,
            TOPMOST = 0x00000008,
            ACCEPTFILES = 0x00000010,
            TRANSPARENT = 0x00000020,
            MDICHILD = 0x00000040,
            TOOLWINDOW = 0x00000080,
            WINDOWEDGE = 0x00000100,
            CLIENTEDGE = 0x00000200,
            CONTEXTHELP = 0x00000400,
            RIGHT = 0x00001000,
            LEFT = 0x00000000,
            RTLREADING = 0x00002000,
            LTRREADING = 0x00000000,
            LEFTSCROLLBAR = 0x00004000,
            RIGHTSCROLLBAR = 0x00000000,
            CONTROLPARENT = 0x00010000,
            STATICEDGE = 0x00020000,
            APPWINDOW = 0x00040000,
            LAYERED = 0x00080000,
            NOINHERITLAYOUT = 0x00100000, // Disable inheritence of mirroring by children
            LAYOUTRTL = 0x00400000, // Right to left mirroring
            COMPOSITED = 0x02000000,
            NOACTIVATE = 0x08000000,
            OVERLAPPEDWINDOW = (WINDOWEDGE | CLIENTEDGE),
            PALETTEWINDOW = (WINDOWEDGE | TOOLWINDOW | TOPMOST),
        }

        /// <summary>
        /// GetDeviceCaps nIndex values.
        /// </summary>
        public enum DeviceCap
        {
            /// <summary>
            /// Logical pixels inch in X
            /// </summary>
            LOGPIXELSX = 88,
            /// <summary>
            /// Logical pixels inch in Y
            /// </summary>
            LOGPIXELSY = 90,
        }

        /// <summary>
        /// EnableMenuItem uEnable values, MF_*
        /// </summary>
        [Flags]
        public enum EnableMenuItemOptions : uint
        {
            /// <summary>
            /// Possible return value for EnableMenuItem
            /// </summary>
            DOES_NOT_EXIST = unchecked((uint)-1),
            ENABLED = 0,
            BYCOMMAND = 0,
            GRAYED = 1,
            DISABLED = 2,
        }

        /// <summary>Specifies the type of visual style attribute to set on a window.</summary>
        public enum WINDOWTHEMEATTRIBUTETYPE : uint
        {
            /// <summary>Non-client area window attributes will be set.</summary>
            WTA_NONCLIENT = 1,
        }

        /// <summary>
        /// WindowThemeNonClientAttributes
        /// </summary>
        [Flags]
        public enum WindowThemeNonClientAttributes : uint
        {
            /// <summary>Prevents the window caption from being drawn.</summary>
            NoDrawCaption = 0x00000001,
            /// <summary>Prevents the system icon from being drawn.</summary>
            NoDrawIcon = 0x00000002,
            /// <summary>Prevents the system icon menu from appearing.</summary>
            NoSysMenu = 0x00000004,
            /// <summary>Prevents mirroring of the question mark, even in right-to-left (RTL) layout.</summary>
            NoMirrorHelp = 0x00000008,
            /// <summary> A mask that contains all the valid bits.</summary>
            ValidBits = NoDrawCaption | NoDrawIcon | NoMirrorHelp | NoSysMenu,
        }

        /// <summary>
        /// SetWindowPos options
        /// </summary>
        [Flags]
        public enum SetWindowPositionOptions
        {
            AsyncWindowPosition = 0x4000,
            DeferErase = 0x2000,
            DrawFrame = 0x0020,
            FrameChanged = 0x0020,
            HideWindow = 0x0080,
            NoActivate = 0x0010,
            NoCopyBits = 0x0100,
            NoMove = 0x0002,
            NoOwnerZOrder = 0x0200,
            NoRedraw = 0x0008,
            NoReposition = 0x0200,
            NoSendChanging = 0x0400,
            NoSize = 0x0001,
            NoZOrder = 0x0004,
            ShowWindow = 0x0040,
        }

        /// <summary>
        /// ShowWindow options
        /// </summary>
        public enum ShowWindowOptions
        {
            Hide = 0,
            ShowNormal = 1,
            Normal = 1,
            ShowMinimized = 2,
            ShowMaximized = 3,
            Maximize = 3,
            ShowNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinimizedWithoutActivating = 7,
            ShowNA = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimize = 11,
        }

        public enum SystemMenuItem
        {
            Size = 0xF000,
            Move = 0xF010,
            Minimize = 0xF020,
            Maximize = 0xF030,
            NextWindow = 0xF040,
            PreviousWindow = 0xF050,
            Close = 0xF060,
            VerticalScroll = 0xF070,
            HorizontalScroll = 0xF080,
            MouseMenu = 0xF090,
            KeyMenu = 0xF100,
            Arrange = 0xF110,
            Restore = 0xF120,
            TaskList = 0xF130,
            ScreenSave = 0xF140,
            Hotkey = 0xF150,
            Default = 0xF160,
            MonitorPower = 0xF170,
            ContextHelp = 0xF180,
            Separator = 0xF00F,
            /// <summary>
            /// SCF_ISSECURE
            /// </summary>
            IsSecure = 0x00000001,
            Icon = Minimize,
            Zoom = Maximize,
        }



        /// <summary>lParam for WindowPositionChanging
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct WindowPosition
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        /// <summary>A Win32 Margins structure for the DWM api calls.
        /// </summary>
        [Serializable, StructLayout(LayoutKind.Sequential)]
        struct Margins
        {
            /// <summary>
            /// Create a Device Margins struct from a (DPI-aware) Logical Thickness
            /// </summary>
            /// <param name="t">The Thickness to convert</param>
            public Margins(System.Windows.Thickness t)
            {
                t = DpiHelper.LogicalPixelsToDevice(t);

                Left = (int)Math.Ceiling(t.Left);
                Right = (int)Math.Ceiling(t.Right);
                Top = (int)Math.Ceiling(t.Top);
                Bottom = (int)Math.Ceiling(t.Bottom);
            }

            public int Left;
            public int Right;
            public int Top;
            public int Bottom;
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct ApiRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public ApiRect(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public ApiRect(Rect r)
            {
                Left = (int)r.Left;
                Right = (int)r.Right;
                Top = (int)r.Top;
                Bottom = (int)r.Bottom;
            }

            public void Offset(int dx, int dy)
            {
                Left += dx;
                Top += dy;
                Right += dx;
                Bottom += dy;
            }

            public int Height
            {
                get { return (Bottom - Top) + 1; }
                set { Bottom = (Top + value) - 1; }
            }
            public int Width
            {
                get { return (Right - Left) + 1; }
                set { Right = (Left + value) - 1; }
            }
            public Size Size { get { return new Size(Width, Height); } }

            public Point Location { get { return new Point(Left, Top); } }

            public override int GetHashCode()
            {
                return Left ^ ((Top << 13) | (Top >> 0x13))
                  ^ ((Width << 0x1a) | (Width >> 6))
                  ^ ((Height << 7) | (Height >> 0x19));
            }

            #region Operator overloads

            /// <summary>
            /// Convert to a Drawing Rectangle.
            /// </summary>
            /// <returns></returns>
            public System.Drawing.Rectangle ToRectangle()
            {
                return System.Drawing.Rectangle.FromLTRB(Left, Top, Right, Bottom);
            }

            public static implicit operator System.Drawing.Rectangle(ApiRect apiRect)
            {
                return System.Drawing.Rectangle.FromLTRB(apiRect.Left, apiRect.Top, apiRect.Right, apiRect.Bottom);
            }

            public static ApiRect FromRectangle(System.Drawing.Rectangle rectangle)
            {
                return new ApiRect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
            }

            public static implicit operator ApiRect(System.Drawing.Rectangle rect)
            {
                return new ApiRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
            }

            public static implicit operator Rect(ApiRect source)
            {
                return new Rect(source.Left, source.Top, Math.Abs(source.Right - source.Left), Math.Abs(source.Bottom - source.Top));
            }

            public static implicit operator ApiRect(Rect source)
            {
                return new ApiRect(source);
            }

            #endregion
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct LogFont
        {
            public int lfHeight;
            public int lfWidth;
            public int lfEscapement;
            public int lfOrientation;
            public int lfWeight;
            public byte lfItalic;
            public byte lfUnderline;
            public byte lfStrikeOut;
            public byte lfCharSet;
            public byte lfOutPrecision;
            public byte lfClipPrecision;
            public byte lfQuality;
            public byte lfPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string lfFaceName;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NonClientMetrics
        {
            public int cbSize;
            public int iBorderWidth;
            public int iScrollWidth;
            public int iScrollHeight;
            public int iCaptionWidth;
            public int iCaptionHeight;
            public LogFont lfCaptionFont;
            public int iSmCaptionWidth;
            public int iSmCaptionHeight;
            public LogFont lfSmCaptionFont;
            public int iMenuWidth;
            public int iMenuHeight;
            public LogFont lfMenuFont;
            public LogFont lfStatusFont;
            public LogFont lfMessageFont;
            // Vista only
            public int iPaddedBorderWidth;

            public static NonClientMetrics VistaMetricsStruct
            {
                get
                {
                    var ncm = new NonClientMetrics();
                    ncm.cbSize = Marshal.SizeOf(typeof(NonClientMetrics));
                    return ncm;
                }
            }

            public static NonClientMetrics XPMetricsStruct
            {
                get
                {
                    var ncm = new NonClientMetrics();
                    // Account for the missing iPaddedBorderWidth
                    ncm.cbSize = Marshal.SizeOf(typeof(NonClientMetrics)) - sizeof(int);
                    return ncm;
                }
            }
        }

        /// <summary>Defines options that are used to set window visual style attributes.</summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct WindowThemeAttributes
        {
            // public static readonly uint Size = (uint)Marshal.SizeOf(typeof(WindowThemeAttributes));
            public const uint Size = 8;

            /// <summary>
            /// A combination of Flags that modify window visual style attributes.
            /// Can be a combination of the WindowThemeNonClientAttributes constants.
            /// </summary>
            [FieldOffset(0)]
            public WindowThemeNonClientAttributes dwFlags;

            /// <summary>
            /// A bitmask that describes how the values specified in Flags should be applied.
            /// If the bit corresponding to a value in Flags is 0, that flag will be removed.
            /// If the bit is 1, the flag will be added.
            /// </summary>
            [FieldOffset(4)]
            public WindowThemeNonClientAttributes dwMask;
        }



        [StructLayout(LayoutKind.Sequential)]
        public class MonitorInfo
        {
            public int Size = Marshal.SizeOf(typeof(MonitorInfo));
            public ApiRect MonitorRect;
            public ApiRect MonitorWorkingSpaceRect;
            public int Flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ApiPoint
        {
            public int x;
            public int y;
        }



        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct CalculateNonClientSizeParameter
        {
            public ApiRect r1;
            public ApiRect r2;

            public ApiRect r3;
            public WindowPosition lppos; //pointer to windowpos
        }

        [Flags]
        public enum CalculateNonClientSizeResults : int
        {
            AlignTop = 0x0010,
            AlignLeft = 0x0020,
            AlignBottom = 0x0040,
            AlignRight = 0x0080,
            HRedraw = 0x0100,
            VRedraw = 0x0200,
            Redraw = (HRedraw | VRedraw),
            ValidRects = 0x0400,
        }


        [StructLayout(LayoutKind.Sequential)]
        public class WindowPlacement
        {
            public int Size = Marshal.SizeOf(typeof(WindowPlacement));
            public int Flags;
            public ShowWindowOptions ShowCommand;
            public ApiPoint MinimizedPosition;
            public ApiPoint MaximizedPosition;
            public ApiRect NormalPosition;
        }













        [DllImport("gdi32.dll", EntryPoint = "CreateRoundRectRgn", SetLastError = true)]
        private static extern IntPtr _CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        public static IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse)
        {
            IntPtr ret = _CreateRoundRectRgn(nLeftRect, nTopRect, nRightRect, nBottomRect, nWidthEllipse, nHeightEllipse);
            if (IntPtr.Zero == ret)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return ret;
        }

        [DllImport("gdi32.dll", EntryPoint = "CreateRectRgn", SetLastError = true)]
        private static extern IntPtr _CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        public static IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect)
        {
            IntPtr ret = _CreateRectRgn(nLeftRect, nTopRect, nRightRect, nBottomRect);
            if (IntPtr.Zero == ret)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return ret;
        }

        [DllImport("gdi32.dll", EntryPoint = "CreateRectRgnIndirect", SetLastError = true)]
        private static extern IntPtr _CreateRectRgnIndirect([In] ref ApiRect lprc);

        public static IntPtr CreateRectRgnIndirect(ApiRect lprc)
        {
            IntPtr ret = _CreateRectRgnIndirect(ref lprc);
            if (IntPtr.Zero == ret)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return ret;
        }


        private enum CombineRgnStyles : int
        {
            RGN_AND = 1,
            RGN_OR = 2,
            RGN_XOR = 3,
            RGN_DIFF = 4,
            RGN_COPY = 5,
            RGN_MIN = RGN_AND,
            RGN_MAX = RGN_COPY
        }

        private enum CombineRgnResult : int
        {
            Error = 0,
            NullRegion = 1,
            SimpleRegion = 2,
            ComplexRegion = 3
        }




        [DllImport("gdi32.dll", EntryPoint = "CombineRgn", SetLastError = true)]
        private static extern CombineRgnResult _CombineRgn([Out] IntPtr hrgnDest, [In] IntPtr hrgnSrc1, [In] IntPtr hrgnSrc2, CombineRgnStyles fnCombineMode);

        public static IntPtr OrRgn(IntPtr mainRegion, IntPtr addRegion)
        {
            IntPtr hrgnDest = addRegion;

            var result = _CombineRgn(hrgnDest, mainRegion, addRegion, CombineRgnStyles.RGN_OR);
            if (result == CombineRgnResult.Error)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return hrgnDest;
        }


        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "DefWindowProcW")]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, WindowMessage Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "EnableMenuItem")]
        private static extern int _EnableMenuItem(IntPtr hMenu, SystemMenuItem uIDEnableItem, EnableMenuItemOptions uEnable);

        public static EnableMenuItemOptions EnableMenuItem(IntPtr hMenu, SystemMenuItem uIDEnableItem, EnableMenuItemOptions uEnable)
        {
            // Returns the previous state of the menu item, or -1 if the menu item does not exist.
            int iRet = _EnableMenuItem(hMenu, uIDEnableItem, uEnable);
            return (EnableMenuItemOptions)iRet;
        }

        [DllImport("user32.dll", EntryPoint = "GetDC", SetLastError = true)]
        private static extern IntPtr _GetDC(IntPtr hwnd);

        public static IntPtr GetDC(IntPtr hwnd)
        {
            IntPtr hdc = _GetDC(hwnd);
            if (IntPtr.Zero == hdc)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return hdc;
        }

        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hdc, DeviceCap nIndex);

        [DllImport("user32.dll", EntryPoint = "GetMonitorInfo", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _GetMonitorInfo(IntPtr hMonitor, [In, Out] MonitorInfo lpmi);

        public static MonitorInfo GetMonitorInfo(IntPtr hMonitor)
        {
            var mi = new MonitorInfo();
            if (!_GetMonitorInfo(hMonitor, mi))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return mi;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)] bool bRevert);

        // This is aliased as a macro in 32bit Windows.
        public static IntPtr GetWindowLongPtr(IntPtr hwnd, WindowLongValues nIndex)
        {
            IntPtr ret = IntPtr.Zero;
            if (8 == IntPtr.Size)
            {
                ret = GetWindowLongPtr64(hwnd, nIndex);
            }
            else
            {
                ret = GetWindowLongPtr32(hwnd, nIndex);
            }
            if (IntPtr.Zero == ret)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return ret;
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, WindowLongValues nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, WindowLongValues nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowPlacement(IntPtr hwnd, WindowPlacement lpwndpl);

        public static WindowPlacement GetWindowPlacement(IntPtr hwnd)
        {
            WindowPlacement wndpl = new WindowPlacement();
            if (GetWindowPlacement(hwnd, wndpl))
            {
                return wndpl;
            }
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }



        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", EntryPoint = "PostMessage", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _PostMessage(IntPtr hWnd, WindowMessage Msg, IntPtr wParam, IntPtr lParam);

        public static void PostMessage(IntPtr hWnd, WindowMessage Msg, IntPtr wParam, IntPtr lParam)
        {
            if (!_PostMessage(hWnd, Msg, wParam, lParam))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        //SetWindowLong won't work correctly for 64-bit: we should use SetWindowLongPtr instead.  On
        //32-bit, SetWindowLongPtr is just #defined as SetWindowLong.  SetWindowLong really should 
        //take/return int instead of IntPtr/HandleRef, but since we're running this only for 32-bit
        //it'll be OK.
        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 4)
            {
                return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
            }
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        }

        //[SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable")]
        [DllImport("User32", CharSet = CharSet.Auto, EntryPoint = "SetWindowLong")]
        public static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        //[SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist")]
        [DllImport("User32", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtr")]
        public static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        public static IntPtr SetWindowLong(IntPtr hWnd, short nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 4)
            {
                return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
            }
            return SetWindowLongPtr64(hWnd, (int)nIndex, dwNewLong);
        }
        //[SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable")]
        [DllImport("User32", CharSet = CharSet.Auto, EntryPoint = "SetWindowLong")]
        public static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, short nIndex, IntPtr dwNewLong);


        [DllImport("user32.dll", EntryPoint = "SetWindowRgn", SetLastError = true)]
        private static extern int _SetWindowRgn(IntPtr hWnd, IntPtr hRgn, [MarshalAs(UnmanagedType.Bool)] bool bRedraw);

        public static void SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw)
        {
            int err = _SetWindowRgn(hWnd, hRgn, bRedraw);
            if (0 == err)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowPos", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, SetWindowPositionOptions uFlags);

        public static void SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, SetWindowPositionOptions uFlags)
        {
            if (!_SetWindowPos(hWnd, hWndInsertAfter, x, y, cx, cy, uFlags))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        // This function is strange in that it returns a BOOL if TPM_RETURNCMD isn't specified, but otherwise the command Id.
        // Currently it's only used with TPM_RETURNCMD, so making the signature match that.
        [DllImport("user32.dll")]
        public static extern uint TrackPopupMenuEx(IntPtr hmenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public static implicit operator System.Drawing.Point(POINT p)
            {
                return new System.Drawing.Point(p.X, p.Y);
            }

            public static implicit operator POINT(System.Drawing.Point p)
            {
                return new POINT(p.X, p.Y);
            }
        }        


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out POINT lpPoint);

        public static System.Windows.Point GetMousePosition()
        {
            POINT pt;
            GetCursorPos(out pt);
            var point = new System.Windows.Point(pt.X, pt.Y);
            return point;
        }
    }
}
