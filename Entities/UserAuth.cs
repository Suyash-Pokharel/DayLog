// Entities/UserAuth.cs
using System;

namespace DayLog.Entities
{
    public class UserAuth
    {
        public int Id { get; set; }

        // Storing salted hash (Base64) — not the plain PIN
        public string? PinHash { get; set; }

        // Salt used for hashing (Base64)
        public string? PinSalt { get; set; }

        // If you plan to store biometric preference
        public bool UseBiometrics { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
