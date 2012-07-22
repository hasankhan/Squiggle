using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace Squiggle.Utilities.Application
{
    public static class DispatcherExtensions
    {
        public static void Invoke(this Dispatcher dispatcher, Action action)
        {
            if (dispatcher.CheckAccess())
                action();
            else
                dispatcher.Invoke(action);
        }

        public static void Delay(this Dispatcher dispatcher, Action action, TimeSpan delay)
        {
            Async.Invoke(() =>
            {
                Thread.Sleep((int)delay.TotalMilliseconds);
            }, action);
        }
    }
}
