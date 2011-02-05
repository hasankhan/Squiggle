using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Squiggle.Utilities
{
    public static class Async
    {
        public static void Invoke(Action action, TimeSpan delay)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                Thread.Sleep((int)delay.TotalMilliseconds);
                action();
            });
        }
    }
}
