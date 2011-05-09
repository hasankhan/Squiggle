using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using System.Linq.Expressions;

namespace Squiggle.History.DAL
{
    class HistoryRepository : IDisposable
    {
        HistoryEntities context = new HistoryEntities();

        public void AddSessionEvent(Guid sessionId, DateTime stamp, EventType type, Guid sender, string senderName, IEnumerable<Guid> recepients, string data)
        {
            var session = context.Sessions.FirstOrDefault(s => s.Id == sessionId);
            if (session != null)
            {
                session.End = DateTime.Now;
                var evnt = Event.CreateEvent((int)type, sender, stamp, senderName, Guid.NewGuid());
                evnt.Data = data;
                evnt.Session = session;
                context.AddToEvents(evnt);
                context.SaveChanges();
            }
        }

        public IEnumerable<Session> GetSessions(SessionCriteria criteria)
        {
            string text = criteria.Text ?? String.Empty;
            string participant = criteria.Participant.HasValue ? criteria.Participant.Value.ToString("N") : String.Empty;

            var result = (from session in context.Sessions.Include("Participants")
                         where (criteria.SessionId == null || session.Id == criteria.SessionId.Value) &&
                             (criteria.From == null || session.Start >= criteria.From.Value) &&
                             (criteria.To == null || session.Start <= criteria.To.Value) &&
                             (text.Length == 0 || session.Events.Any(e=>e.Data.Contains(text))) && 
                             (criteria.Participant == null || session.Participants.Any(p=>p.ParticipantId == criteria.Participant.Value))
                         orderby session.Start
                         select session);

            return result.ToList();

        }

        public Session GetSession(Guid sessionId)
        {
            var session = context.Sessions.Include("Participants")
                                          .Include("Events")
                                          .FirstOrDefault(s => s.Id == sessionId);
            return session;
        }

        public void ClearHistory(Guid? sessionId = null)
        {
            DeleteAll(context.Events, e=>sessionId == null || e.Session.Id == sessionId.Value);
            DeleteAll(context.Participants, p => sessionId == null || p.Session.Id == sessionId.Value);
            DeleteAll(context.Sessions, s => sessionId == null || s.Id == sessionId.Value);

            context.SaveChanges();
        }

        public void AddSession(Session newSession, IEnumerable<Participant> participants)
        {
            var session = GetSession(newSession.Id);
            if (session == null)
            {
                foreach (var participant in participants)
                    newSession.Participants.Add(participant);
                context.AddToSessions(newSession);
            }            
            else
                foreach (var participant in participants)
                    AddParticipant(session, participant);

            context.SaveChanges();
        }

        public void AddParticipant(Guid sessionId, Participant participant)
        {
            var session = GetSession(sessionId);
            if (session != null)
                AddParticipant(session, participant);

            context.SaveChanges();
        }

        public void Dispose()
        {
            context.Dispose();
        }

        void AddParticipant(Session session, Participant participant)
        {
            if (!session.Participants.Any(p => p.ParticipantId == participant.Id))
                session.Participants.Add(participant);
        }

        void DeleteAll<TEntity>(ObjectQuery<TEntity> items, Expression<Func<TEntity, bool>> condition)
        {
            foreach (var item in items.Where(condition))
                context.DeleteObject(item);
        }        
    }
}
