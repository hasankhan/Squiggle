using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace Squiggle.Utilities
{
    public static class Async
    {
        public static void Invoke(Action action)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Exception occured in async operation: " + ex.ToString());
                }
            });
        }

        public static void Invoke(Action action, Action onComplete, Dispatcher dispatcher)
        {
            Invoke(() =>
            {
                action();
                dispatcher.Invoke(onComplete);
            });
        }
    }
}
