using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Data.Entity;
using Squiggle.History.DAL.Entities;
using System.Data.Entity.Core.Objects;

namespace Squiggle.History.DAL
{
    class HistoryRepository : IDisposable
    {
        HistoryContext context;

        public HistoryRepository(HistoryContext context)
        {
            this.context = context;
            this.context.Database.CreateIfNotExists();
        }

        public void AddSessionEvent(string sessionId, DateTime stamp, EventType type, string sender, string senderName, IEnumerable<string> recipients, string data)
        {
            var session = context.Sessions.FirstOrDefault(s => s.Id == sessionId);
            if (session != null)
            {
                session.End = session.End < stamp ? stamp : session.End;
                var evnt = new Event() 
                { 
                    Id = Guid.NewGuid().ToString(), 
                    Type = type, 
                    SenderId = sender.ToString(), 
                    Stamp = stamp, 
                    SenderName = senderName 
                };
                evnt.Data = data;
                evnt.Session = session;
                context.Events.Add(evnt);
                context.SaveChanges();
            }
        }

        public IEnumerable<Session> GetSessions(SessionCriteria criteria)
        {
            string text = criteria.Text ?? String.Empty;

            var result = (from session in context.Sessions.Include("Participants")
                         where (criteria.SessionId == null || session.Id == criteria.SessionId) &&
                             (criteria.From == null || session.Start >= criteria.From.Value) &&
                             (criteria.To == null || session.Start <= criteria.To.Value) &&
                             (text.Length == 0 || session.Events.Any(e=>e.Data.Contains(text))) &&
                             (criteria.Participant == null || session.Participants.Any(p => p.ContactId == criteria.Participant))
                         orderby session.Start
                         select session);

            return result.ToList();

        }

        public Session GetSession(string sessionId)
        {
            var session = context.Sessions.Include("Participants")
                                          .Include("Events")
                                          .FirstOrDefault(s => s.Id == sessionId);
            return session;
        }

        public IEnumerable<StatusUpdate> GetStatusUpdates(StatusCriteria criteria)
        {
            var updates = (from update in context.StatusUpdates
                           where (criteria.From == null || update.Stamp >= criteria.From.Value) &&
                                 (criteria.To == null || update.Stamp <= criteria.To.Value)
                           orderby update.Stamp
                           select update);

            return updates.ToList();
        }

        public void ClearChatHistory(string sessionId = null)
        {
            DeleteSession(sessionId);

            context.SaveChanges();
        }

        public void ClearStatusHistory()
        {
            DeleteAll(context.StatusUpdates, _ => true);

            context.SaveChanges();
        }

        public void AddSession(Session newSession, IEnumerable<Participant> participants)
        {
            var session = GetSession(newSession.Id);
            if (session == null)
            {
                context.Sessions.Add(newSession);
                session = newSession;
            }
            
            foreach (var participant in participants)
                AddParticipant(session, participant);

            context.SaveChanges();
        }

        public void AddParticipant(string sessionId, Participant participant)
        {
            var session = GetSession(sessionId);
            if (session != null)
                AddParticipant(session, participant);

            context.SaveChanges();
        }

        public void DeleteSessions(IEnumerable<string> sessionIds)
        {
            foreach (string sessionId in sessionIds)
                DeleteSession(sessionId);

            context.SaveChanges();
        }

        public void AddStatusUpdate(DateTime stamp, string contactId, string contactName, int status)
        {
            context.StatusUpdates.Add(new StatusUpdate() 
            {
                Id = Guid.NewGuid().ToString(),
                ContactId = contactId, 
                ContactName = contactName, 
                StatusCode = status, 
                Stamp = stamp 
            });
            context.SaveChanges();
        }

        public void Dispose()
        {
            context.Dispose();
        }

        void DeleteSession(string sessionId)
        {
            DeleteAll(context.Events, e => sessionId == null || e.Session.Id == sessionId);
            DeleteAll(context.Participants, p => sessionId == null || p.Session.Id == sessionId);
            DeleteAll(context.Sessions, s => sessionId == null || s.Id == sessionId);
        }

        void AddParticipant(Session session, Participant participant)
        {
            participant.SessionId = session.Id;
            if (!session.Participants.Any(p => p.ContactId == participant.ContactId))
                context.Participants.Add(participant);
        }

        void DeleteAll<TEntity>(DbSet<TEntity> set, Expression<Func<TEntity, bool>> condition) where TEntity:class
        {
            foreach (var item in set.Where(condition))
                set.Remove(item);
        }

        IQueryable<TEntity> WhereIn<TEntity, TValue>(ObjectQuery<TEntity> query, Expression<Func<TEntity, TValue>> selector, IEnumerable<TValue> collection)
        {
            if (selector == null) 
                throw new ArgumentNullException("selector");
            if (collection == null) 
                throw new ArgumentNullException("collection");
            ParameterExpression p = selector.Parameters.Single();

            if (!collection.Any()) 
                return query;

            IEnumerable<Expression> equals = collection.Select(value => (Expression)Expression.Equal(selector.Body, Expression.Constant(value, typeof(TValue))));

            Expression body = equals.Aggregate((accumulate, equal) => Expression.Or(accumulate, equal));

            return query.Where(Expression.Lambda<Func<TEntity, bool>>(body, p));
        }
    }
}
