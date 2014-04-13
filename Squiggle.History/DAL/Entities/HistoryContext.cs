using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Common;

namespace Squiggle.History.DAL.Entities
{
    class HistoryContext : DbContext 
    {
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<StatusUpdate> StatusUpdates { get; set; }

        public HistoryContext(DbConnection connection, bool contextOwnsConnection)
            : base(connection, contextOwnsConnection)
        {
        }
    }
}
