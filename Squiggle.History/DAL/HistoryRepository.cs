using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.History.DAL
{
    class HistoryRepository : IDisposable
    {
        HistoryEntities context = new HistoryEntities();

        public void AddSessionEvent(Guid sessionId, DateTime stamp, EventType type, Guid sender, string senderName, IEnumerable<Guid> recepients, string data)
        {
            string temp = String.Join(",", recepients.Select(r => r.ToString("N")).ToArray());
            var evnt = Event.CreateEvent(sessionId, (int)type, sender, stamp, senderName, temp, Guid.NewGuid());
            evnt.Data = data;
            context.AddToEvents(evnt);
            context.SaveChanges();
        }

        public IEnumerable<Conversation> GetConversations(ConversationCriteria criteria)
        {
            string text = criteria.Text ?? String.Empty;
            string participant = criteria.Participant.HasValue ? criteria.Participant.Value.ToString("N") : String.Empty;

            var result = (from evnt in context.Events
                         group evnt by evnt.SessionId into g
                         where g.Any
                         (e => (criteria.SessionId == null || e.SessionId == criteria.SessionId.Value) &&
                             (criteria.From == null || e.Stamp >= criteria.From.Value) &&
                             (criteria.To == null || e.Stamp <= criteria.To.Value) &&
                             (text.Length == 0 || e.Data.Contains(text)) &&
                             (criteria.Participant == null || e.Sender == criteria.Participant.Value || e.Recepients.Contains(participant))
                         )
                         select new Conversation
                         {
                             Id = g.Key,
                             Start = g.Min(e => e.Stamp),
                             End = g.Max(e => e.Stamp),
                             Participants = g.Select(e => new Participant()
                             {
                                 Id = e.Sender,
                                 Name = e.SenderName
                             }).Distinct()
                         }).OrderBy(c=>c.Start);

            return result.ToList();

        }

        public IEnumerable<Event> GetEvents(Guid sessionId)
        {
            var events = context.Events.Where(e => e.SessionId == sessionId).OrderBy(e=>e.Stamp);
            return events.ToList();
        }

        public void ClearHistory(Guid? sessionId = null)
        {
            var query = context.Events.Where(e=>sessionId == null || e.SessionId == sessionId.Value);
            foreach (var item in query)
                context.DeleteObject(item);

            context.SaveChanges();
        }

        public void Dispose()
        {
            context.Dispose();
        }        
    }
}
