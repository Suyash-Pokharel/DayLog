// Models/EntryViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace DayLog.Models
{
    public class EntryViewModel
    {
        public int Id { get; set; }

        [Required]
        public DateTime EntryDate { get; set; } = DateTime.Today;

        [MaxLength(200)]
        public string? Title { get; set; }

        // Primary mood required
        [Range(1, int.MaxValue, ErrorMessage = "Please select a primary mood")]
        public int PrimaryMoodId { get; set; }

        public string? TagsCsv { get; set; }

        public string? ContentHtml { get; set; }
    }
}
