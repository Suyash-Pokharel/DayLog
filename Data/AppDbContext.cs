// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using DayLog.Entities;

namespace DayLog.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<LogEntry> LogEntries { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<UserAuth> UserAuths { get; set; } = null!; // optional but useful

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // index EntryDate for faster date queries
            modelBuilder.Entity<LogEntry>().HasIndex(e => e.EntryDate);

            // tag name uniqueness
            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.Name)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
