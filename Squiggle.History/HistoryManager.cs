using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.History.DAL;
using Squiggle.History.DAL.Entities;

namespace Squiggle.History
{
    public class HistoryManager
    {
        public void AddSessionEvent(Guid sessionId, DateTime stamp, EventType type, Guid senderId, string senderName, IEnumerable<Guid> recipients, string data)
        {
            using (var repository = new HistoryRepository())
            {
                repository.AddSessionEvent(sessionId, stamp, type, senderId, senderName, recipients, data);
                if (type == EventType.Joined)
                {
                    var participant = new Participant() 
                    {
                        Id = Guid.NewGuid(),
                        ContactId = senderId, 
                        ContactName = senderName 
                    };
                    repository.AddParticipant(sessionId, participant);
                }
            }
        }

        public void AddStatusUpdate(DateTime stamp, Guid contactId, string contactName, int status)
        {
            using (var repository = new HistoryRepository())
                repository.AddStatusUpdate(stamp, contactId, contactName, status);
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


        public IEnumerable<StatusUpdate> GetStatusUpdates(StatusCriteria criteria)
        {
            using (var repository = new HistoryRepository())
                return repository.GetStatusUpdates(criteria);
        }

        public void ClearChatHistory()
        {
            using (var repository = new HistoryRepository())
                repository.ClearChatHistory();
        }

        public void ClearStatusHistory()
        {
            using (var repository = new HistoryRepository())
                repository.ClearStatusUpdates();
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
