using Microsoft.EntityFrameworkCore;
using DayLog.Models;

namespace DayLog.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Entry> Entries { get; set; }
        public DbSet<Mood> Moods { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<UserSecurity> UserSecurities { get; set; }
    }
}
