using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Squiggle.UI.Helpers
{
    class TimeoutSignal: IDisposable
    {
        TimeSpan timeout;
        ManualResetEvent resetEvent;

        public TimeoutSignal(TimeSpan timeout)
        {
            this.timeout = timeout;
            this.resetEvent = new ManualResetEvent(true);
        }

        public void Begin()
        {
            resetEvent.Reset();
        }

        public void End()
        {
            resetEvent.Set();
        }

        public void Wait()
        {
            resetEvent.WaitOne(timeout);
        }

        public void Dispose()
        {
            resetEvent.Dispose();
        }
    }
}
