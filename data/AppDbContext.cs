// Data/AppDbContext.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DayLog.Entities;

namespace DayLog.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<LogEntry> LogEntries { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<UserAuth> UserAuths { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // index EntryDate for faster date queries and enforce one entry per day
            modelBuilder.Entity<LogEntry>()
                .HasIndex(e => e.EntryDate)
                .IsUnique();

            // tag name uniqueness (keeps earlier behavior)
            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.Name)
                .IsUnique();

            // Unique index on Username to prevent duplicates
            modelBuilder.Entity<UserAuth>()
                        .HasIndex(u => u.Username)
                        .IsUnique();

            base.OnModelCreating(modelBuilder);
        }

        // Optional: keep CreatedAt/UpdatedAt in sync automatically for certain entities
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is LogEntry || e.Entity is UserAuth);

            foreach (var entry in entries)
            {
                if (entry.Entity is LogEntry le)
                {
                    if (entry.State == EntityState.Added)
                    {
                        le.CreatedAt = DateTime.UtcNow;
                        le.UpdatedAt = DateTime.UtcNow;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        le.UpdatedAt = DateTime.UtcNow;
                    }
                }
                else if (entry.Entity is UserAuth ua)
                {
                    if (entry.State == EntityState.Added)
                    {
                        ua.CreatedAt = DateTime.UtcNow;
                        ua.UpdatedAt = DateTime.UtcNow;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        ua.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }
        }
    }
}
