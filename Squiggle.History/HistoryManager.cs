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
            {
                repository.AddSessionEvent(sessionId, stamp, type, sender, senderName, recepients, data);
                if (type == EventType.Joined)
                {
                    var participant = Participant.CreateParticipant(Guid.NewGuid(), sender, senderName);
                    repository.AddParticipant(sessionId, participant);
                }
            }
        }

        public IEnumerable<Session> GetSessions(SessionCriteria criteria)
        {
            using (var repository = new HistoryRepository())
                return repository.GetSessions(criteria);
        }

        public Session GetSession(Guid sessionId)
        {
            using (var repository = new HistoryRepository())
                return repository.GetSession(sessionId);
        }

        public void Clear()
        {
            using (var repository = new HistoryRepository())
                repository.ClearHistory();
        }

        public void AddSession(Session newSession, IEnumerable<Participant> participants)
        {
            using (var repository = new HistoryRepository())
                repository.AddSession(newSession, participants);
        }

        public void DeleteSessions(IEnumerable<Guid> sessionIds)
        {
            using (var repository = new HistoryRepository())
                repository.DeleteSessions(sessionIds);
        }
    }
}
