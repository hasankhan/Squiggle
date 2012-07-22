using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Squiggle.Utilities
{
    public static class Async
    {
        public static TaskScheduler UIScheduler { get; set; }

        public static Task Invoke(Action action)
        {
            return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default)
                               .ContinueWith(task =>
                                {
                                    task.Exception.Handle(ex=>
                                    {
                                        Trace.WriteLine("Exception occured in async operation: " + ex.ToString());
                                        return true;
                                    });
                                }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public static void Invoke(Action action, Action onComplete)
        {
            Invoke(action).ContinueWith(task => onComplete(), UIScheduler);
        }
    }
}
