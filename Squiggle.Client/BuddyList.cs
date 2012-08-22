using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Squiggle.Chat
{
    class BuddyList: IList<Buddy>
    {
        List<Buddy> buddies;

        public BuddyList()
        {
            buddies = new List<Buddy>();
        }

        #region IList<Buddy> Members

        public int IndexOf(Buddy item)
        {
            lock (buddies)
                return (buddies.IndexOf(item));
        }

        public void Insert(int index, Buddy item)
        {
            lock (buddies)
                buddies.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            lock (buddies)
                buddies.RemoveAt(index);
        }

        public Buddy this[string id]
        {
            get
            {
                lock (buddies)
                {
                    Buddy buddy = buddies.FirstOrDefault(b => b.Id.Equals(id));
                    return buddy;
                }
            }
        }

        public Buddy this[int index]
        {
            get
            {
                lock (buddies)
                    return buddies[index];
            }
            set
            {
                lock (buddies)
                    buddies[index] = value;
            }
        }

        #endregion

        #region ICollection<Buddy> Members

        public void Add(Buddy item)
        {
            lock (buddies)
                buddies.Add(item);
        }

        public void Clear()
        {
            lock (buddies)
                buddies.Clear();
        }

        public bool Contains(Buddy item)
        {
            lock (buddies)
                return buddies.Contains(item);
        }

        public void CopyTo(Buddy[] array, int arrayIndex)
        {
            lock (buddies)
                buddies.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get 
            {
                lock (buddies)
                    return buddies.Count;
            }
        }

        public bool IsReadOnly
        {
            get 
            {
                lock (buddies)
                    return false;
            }
        }

        public bool Remove(Buddy item)
        {
            lock (buddies)
                return buddies.Remove(item);
        }

        #endregion

        #region IEnumerable<Buddy> Members

        public IEnumerator<Buddy> GetEnumerator()
        {
            lock (buddies)
                return buddies.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (buddies)
                return GetEnumerator();
        }

        #endregion
    }
}
