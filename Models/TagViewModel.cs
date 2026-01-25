// Models/TagViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace DayLog.Models
{
    public class TagViewModel
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
