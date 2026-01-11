// Entities/LogEntry.cs
using System;

namespace DayLog.Entities
{
    public class LogEntry
    {
        public int Id { get; set; }
        public DateTime EntryDate { get; set; }
        public string? Title { get; set; }
        public string? ContentHtml { get; set; }
        public int PrimaryMoodId { get; set; }
        public string? SecondaryMoodIdsCsv { get; set; }
        public string? TagsCsv { get; set; }
        public string? Category { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int WordCount { get; set; }
    }
}
