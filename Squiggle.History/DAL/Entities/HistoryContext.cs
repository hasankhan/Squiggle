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
        public DbSet<Participant> Participants { get; set; } = null!;
        public DbSet<Session> Sessions { get; set; } = null!;
        public DbSet<Event> Events { get; set; } = null!;
        public DbSet<StatusUpdate> StatusUpdates { get; set; } = null!;

        public HistoryContext(DbConnection connection, bool contextOwnsConnection)
            : base(connection, contextOwnsConnection)
        {
        }
    }
}
