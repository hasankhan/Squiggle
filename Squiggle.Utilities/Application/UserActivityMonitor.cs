using System;
using System.Runtime.InteropServices;
using System.Timers;

namespace Squiggle.Utilities.Application
{
    public class IdleEventArgs : EventArgs
    {
        public TimeSpan IdleTime { get; set; }
    }

    public class UserActivityMonitor: IDisposable
    {
        #region PInvoke
        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));

            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dwTime;
        }

        static TimeSpan GetIdleTime()
        {
            int idleTime = 0;
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;

            int envTicks = Environment.TickCount;

            if (GetLastInputInfo(ref lastInputInfo))
            {
                int lastInputTick = (int)lastInputInfo.dwTime;

                idleTime = envTicks - lastInputTick;
            }

            int miliseconds = (idleTime > 0) ? idleTime : 0;
            return TimeSpan.FromMilliseconds(miliseconds);
        }
        #endregion

        Timer timer;
        bool isIdle;
        TimeSpan timeout;

        public event EventHandler<IdleEventArgs> Idle = delegate { };
        public event EventHandler Active = delegate { };

        public UserActivityMonitor(TimeSpan timeout)
        {
            this.timeout = timeout;
            timer = new Timer(1.Seconds().TotalMilliseconds);
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TimeSpan idleTime = GetIdleTime();
            if (idleTime > timeout)
            {
                if (!isIdle)
                    Idle(this, new IdleEventArgs() { IdleTime = idleTime });
                isIdle = true;
            }
            else
            {
                if (isIdle)
                    Active(this, EventArgs.Empty);
                isIdle = false;
            }
        }

        public void Start()
        {
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }

        #region IDisposable Members

        public void Dispose()
        {
            Stop();
        }

        #endregion
    }
}
