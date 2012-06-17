using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace Squiggle.Utilities
{
    public static class Async
    {
        public static Task Invoke(Action action)
        {
            return Task.Factory.StartNew(action).ContinueWith(task =>
            {
                if (task.IsFaulted)
                    task.Exception.Handle(ex=>
                    {
                        Trace.WriteLine("Exception occured in async operation: " + ex.ToString());
                        return false;
                    });
            });
        }

        public static void Invoke(Action action, Action onComplete)
        {
            Invoke(action).ContinueWith(task => onComplete(), TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
