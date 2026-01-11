// Models/UserAuth.cs
using System.ComponentModel.DataAnnotations;

namespace DayLog.Models
{
    // Model used when initially setting a PIN
    public class SetPinModel
    {
        [Required]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "PIN must be between 4 and 20 characters.")]
        [DataType(DataType.Password)]
        public string? Pin { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Pin), ErrorMessage = "PIN and confirmation do not match.")]
        public string? ConfirmPin { get; set; }
    }

    // Model used when changing an existing PIN (enter current + new)
    public class ChangePinModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string? CurrentPin { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "New PIN must be between 4 and 20 characters.")]
        [DataType(DataType.Password)]
        public string? NewPin { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPin), ErrorMessage = "New PIN and confirmation do not match.")]
        public string? ConfirmNewPin { get; set; }
    }

    // Model used when prompting the user to unlock (verify PIN)
    public class VerifyPinModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string? Pin { get; set; }
    }
}
