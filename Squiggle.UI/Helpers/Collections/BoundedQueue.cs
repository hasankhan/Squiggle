using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.UI.Helpers.Collections
{
    class BoundedQueue<T>: IEnumerable<T>
    {
        int maxItems;
        Queue<T> queue = new Queue<T>();

        public BoundedQueue(int maxItems)
        {
            this.maxItems = maxItems;
            queue = new Queue<T>(maxItems);
        }

        public void Enqueue(T item)
        {
            queue.Enqueue(item);
            if (queue.Count > maxItems)
                queue.Dequeue();
        }

        public T Dequeue(T item)
        {
            return queue.Dequeue();
        }

        public void Clear()
        {
            queue.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return queue.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
