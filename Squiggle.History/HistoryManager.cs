using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Squiggle.History.DAL;
using Squiggle.History.DAL.Entities;

namespace Squiggle.History
{
    public class HistoryManager
    {
        DbConnection connection;
        
        public HistoryManager(DbConnection connection)
        {
            this.connection = connection;
        }        

        public void AddSessionEvent(Guid sessionId, EventType type, Guid senderId, string senderName, IEnumerable<Guid> recipients, string data)
        {
            using (HistoryRepository repository = this.CreateRepository())
            {
                repository.AddSessionEvent(sessionId, DateTime.UtcNow, type, senderId, senderName, recipients, data);
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

        public void AddStatusUpdate(Guid contactId, string contactName, int status)
        {
            using (HistoryRepository repository = this.CreateRepository())
                repository.AddStatusUpdate(DateTime.UtcNow, contactId, contactName, status);
        }

        public IEnumerable<Session> GetSessions(SessionCriteria criteria)
        {
            using (HistoryRepository repository = this.CreateRepository())
                return repository.GetSessions(criteria);
        }

        public Session GetSession(Guid sessionId)
        {
            using (HistoryRepository repository = this.CreateRepository())
                return repository.GetSession(sessionId);
        }


        public IEnumerable<StatusUpdate> GetStatusUpdates(StatusCriteria criteria)
        {
            using (HistoryRepository repository = this.CreateRepository())
                return repository.GetStatusUpdates(criteria);
        }

        public void ClearChatHistory()
        {
            using (HistoryRepository repository = this.CreateRepository())
                repository.ClearChatHistory();
        }

        public void ClearStatusHistory()
        {
            using (HistoryRepository repository = this.CreateRepository())
                repository.ClearStatusHistory();
        }

        public void AddSession(Session newSession, IEnumerable<Participant> participants)
        {
            using (HistoryRepository repository = this.CreateRepository())
                repository.AddSession(newSession, participants);
        }

        public void DeleteSessions(IEnumerable<Guid> sessionIds)
        {
            using (HistoryRepository repository = this.CreateRepository())
                repository.DeleteSessions(sessionIds);
        }

        private HistoryRepository CreateRepository()
        {
            return new HistoryRepository(new HistoryContext(this.connection, contextOwnsConnection: false));
        }
    }
}
