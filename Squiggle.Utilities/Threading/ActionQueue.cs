using System;
using System.Collections.Generic;

namespace Squiggle.Utilities.Threading
{
    public class ActionQueue
    {
        bool open;
        object syncRoot = new object();
        Queue<Action> actions = new Queue<Action>();      

        public void Enqueue(Action action)
        {
            if (!open)
                lock (syncRoot)
                    if (!open)
                    {
                        actions.Enqueue(action);
                        return;
                    }

            Execute(action);
        }

        protected virtual void Execute(Action action)
        {
            action();
        }

        public void Open()
        {
            lock (syncRoot)
            {
                while (actions.Count > 0)
                    Execute(actions.Dequeue());
                open = true;
            }
        }
    }
}
