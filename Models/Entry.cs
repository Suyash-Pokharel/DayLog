using System;

namespace DayLog.Models
{
    public class Entry
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? PrimaryMoodId { get; set; }
        public string SecondaryMoodIds { get; set; } // simple comma list
        public string Tags { get; set; } // simple comma list
        public int? CategoryId { get; set; }
    }
}
