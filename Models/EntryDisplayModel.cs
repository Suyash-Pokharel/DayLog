// Models/EntryDisplayModel.cs
using System;

namespace DayLog.Models
{
    public class EntryDisplayModel
    {
        public int Id { get; set; }
        public DateTime EntryDate { get; set; }
        public string? Title { get; set; }
        public string? PreviewHtml { get; set; }
        public int PrimaryMoodId { get; set; }
        public string? TagsCsv { get; set; }
    }
}
