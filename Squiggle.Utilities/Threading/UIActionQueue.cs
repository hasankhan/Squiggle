using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace Squiggle.Utilities.Threading
{
    public class UIActionQueue: ActionQueue
    {
        Dispatcher dispatcher;

        public UIActionQueue(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        protected override void Execute(Action action)
        {
            dispatcher.Invoke(action);
        }
    }
}
