using System;
using System.Collections.Generic;

namespace Squiggle.UI
{
    class EventQueue
    {
        class EventItem
        {
            public object Sender {get; set; }
            public EventArgs Args {get; set; }
            public Delegate Handler {get; set; }

            public void Invoke()
            {
                if (Handler is EventHandler)
                    ((EventHandler)Handler)(Sender, Args);
                else if (Handler.GetType().GetGenericTypeDefinition().Equals(typeof(EventHandler<>)))
                    Handler.DynamicInvoke(Sender, Args);
            }
        }

        Queue<EventItem> events = new Queue<EventItem>();

        public void Enqueue(object sender, EventArgs args, EventHandler handler)
        {
            EnqueueInternal(sender, args, handler);
        }        

        public void Enqueue<T>(object sender, T args, EventHandler<T> handler) where T:EventArgs
        {
            EnqueueInternal(sender, args, handler);
        }

        void EnqueueInternal(object sender, EventArgs args, Delegate handler)
        {
            events.Enqueue(new EventItem()
            {
                Sender = sender,
                Args = args,
                Handler = handler
            });
        }

        public void DequeueAll()
        {
            while (events.Count > 0)
                events.Dequeue().Invoke();
        }
    }
}
