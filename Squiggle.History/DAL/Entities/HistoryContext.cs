using Microsoft.EntityFrameworkCore;

namespace Squiggle.History.DAL.Entities
{
    class HistoryContext : DbContext 
    {
        public DbSet<Participant> Participants { get; set; } = null!;
        public DbSet<Session> Sessions { get; set; } = null!;
        public DbSet<Event> Events { get; set; } = null!;
        public DbSet<StatusUpdate> StatusUpdates { get; set; } = null!;

        readonly string connectionString;

        public HistoryContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(connectionString);
        }
    }
}
