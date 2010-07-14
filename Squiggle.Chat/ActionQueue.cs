using System;
using System.Collections.Generic;

namespace Squiggle.Chat
{
    public class ActionQueue
    {
        Queue<Action> actions = new Queue<Action>();      

        public void Enqueue<T>(object sender, T args, EventHandler<T> handler) where T:EventArgs
        {
            Enqueue(()=>handler.DynamicInvoke(sender, args));
        }

        public void Enqueue(Action action)
        {
            lock (actions)
                actions.Enqueue(action);
        }

        public void DequeueAll()
        {
            while (actions.Count > 0)
            {
                Action action;
                lock (actions)
                    action = actions.Dequeue();
                action();
            }
        }
    }
}
