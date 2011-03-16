using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.History.DAL;

namespace Squiggle.History
{
    public class HistoryManager
    {
        public void AddSessionEvent(Guid sessionId, DateTime stamp, EventType type, Guid sender, string senderName, IEnumerable<Guid> recepients, string data)
        {
            using (var repository = new HistoryRepository())
                repository.AddSessionEvent(sessionId, stamp, type, sender, senderName, recepients, data);
        }

        public IEnumerable<Conversation> GetConversations(ConversationCriteria criteria)
        {
            using (var repository = new HistoryRepository())
                return repository.GetConversations(criteria);
        }

        public IEnumerable<Event> GetEvents(Guid sessionId)
        {
            using (var repository = new HistoryRepository())
                return repository.GetEvents(sessionId);
        }

        public void Clear()
        {
            using (var repository = new HistoryRepository())
                repository.ClearHistory();
        }
    }
}
