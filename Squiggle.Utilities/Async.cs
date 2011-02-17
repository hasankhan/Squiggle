using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace Squiggle.Utilities
{
    public static class Async
    {
        public static void Invoke(Action action, TimeSpan delay)
        {
            Invoke(() =>
            {
                Thread.Sleep((int)delay.TotalMilliseconds);
                action();
            }, delay);
        }

        public static void Invoke(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception occured in async operation: " + ex.ToString());
                throw;
            }
        }
    }
}
