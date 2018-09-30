using Microsoft.EntityFrameworkCore;
using TelephoneCallsWebApplication.Models;

namespace TelephoneCallsWebApplication.Data
{
    public class TelephoneCallsContext : DbContext
    {
        public TelephoneCallsContext(DbContextOptions<TelephoneCallsContext> options) : base(options)
        {
        }

        public DbSet<Event> Events { get; set; }
        public DbSet<Call> Calls { get; set; }
        public DbSet<EventType> EventTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventType>().ToTable("T_EVENT_TYPE");
            modelBuilder.Entity<Event>().ToTable("T_EVENT");
            modelBuilder.Entity<Call>().ToTable("T_CALL");
        }
    }
}
