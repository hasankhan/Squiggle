using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Squiggle.Chat.Services.Chat
{
    class ChatSessionCollection: ICollection<IChatSession>
    {
        Dictionary<Guid, IChatSession> chatSessions = new Dictionary<Guid, IChatSession>();
        Dictionary<IPEndPoint, List<IChatSession>> userSessions = new Dictionary<IPEndPoint, List<IChatSession>>();

        public void Add(IChatSession item)
        {
            chatSessions[item.ID] = item;
            foreach (IPEndPoint user in item.RemoteUsers)
            {
                List<IChatSession> sessions;
                if (!userSessions.TryGetValue(user, out sessions))
                    sessions = new List<IChatSession>();
                sessions.Add(item);
                userSessions[user] = sessions;
            }
        }

        public void Clear()
        {
            chatSessions.Clear();
            userSessions.Clear();
        }

        public bool Contains(IChatSession item)
        {
            return Contains(item.ID);
        }

        public bool Contains(Guid sessionId)
        {
            return chatSessions.ContainsKey(sessionId);
        }

        public IEnumerable<IChatSession> FindSessions(IPEndPoint user)
        {
            List<IChatSession> sessions;
            if (userSessions.TryGetValue(user, out sessions))
                return sessions;
            else
                return Enumerable.Empty<IChatSession>();
        }

        public void CopyTo(IChatSession[] array, int arrayIndex)
        {
            chatSessions.Values.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return chatSessions.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(IChatSession item)
        {
            bool removed = chatSessions.Remove(item.ID);
            foreach (IPEndPoint user in item.RemoteUsers)
            {
                List<IChatSession> sessions;
                if (userSessions.TryGetValue(user, out sessions))
                    sessions.Remove(item);
            }
            return removed;
        }

        public IEnumerator<IChatSession> GetEnumerator()
        {
            return chatSessions.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
