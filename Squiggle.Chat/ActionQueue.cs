using System;
using System.Collections.Generic;

namespace Squiggle.Chat
{
    public class ActionQueue
    {
        Queue<Action> actions = new Queue<Action>();      

        public void Enqueue(Action action)
        {
            lock (actions)
                actions.Enqueue(action);
        }

        public void DequeueAll()
        {
            while (Dequeue()) ;
        }

        public bool Dequeue()
        {
            Action action = null;
            lock (actions)
                if (actions.Count > 0)
                    action = actions.Dequeue();

            if (action == null)
                return false;
            
            action();
            return true;
        }
    }
}
