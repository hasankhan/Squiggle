using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Squiggle.Utilities.Threading
{
    public class UIActionQueue: ActionQueue
    {
        SynchronizationContext context;

        public UIActionQueue(SynchronizationContext context)
        {
            this.context = context;
        }

        protected override void Execute(Action action)
        {
            context.Send(_ => action(), null);
        }
    }
}
