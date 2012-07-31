using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.UI.Windows;

namespace Squiggle.UI
{
    class ChatWindowCollection: ICollection<ChatWindow>
    {
        List<ChatWindow> windows;

        public ChatWindowCollection()
        {
            windows = new List<ChatWindow>();
        }

        public void Add(ChatWindow item)
        {
            windows.Add(item);
        }

        public void Clear()
        {
            windows.Clear();
        }

        public bool Contains(ChatWindow item)
        {
            return windows.Contains(item);
        }

        public void CopyTo(ChatWindow[] array, int arrayIndex)
        {
            windows.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return windows.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(ChatWindow item)
        {
            return windows.Remove(item);
        }

        public ChatWindow Find(Func<ChatWindow, bool> criterea)
        {
            var window = windows.FirstOrDefault(criterea);
            return window;
        }

        public IEnumerator<ChatWindow> GetEnumerator()
        {
            return windows.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
