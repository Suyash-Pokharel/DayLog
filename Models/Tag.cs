// Entities/Tag.cs
using System;

namespace DayLog.Entities
{
    public class Tag
    {
        public int Id { get; set; }

        // Name should be unique (configure unique index in DbContext)
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
