// Common/PasswordHasher.cs
using System;
using System.Security.Cryptography;
using System.Text;

namespace DayLog.Common
{
    public static class PasswordHasher
    {
        // PBKDF2 parameters
        private const int SaltLength = 16; // bytes
        private const int KeyLength = 32;  // bytes
        private const int Iterations = 100_000;

        public static (string HashBase64, string SaltBase64) Hash(string plain)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltLength);
            using var pbkdf2 = new Rfc2898DeriveBytes(plain, salt, Iterations, HashAlgorithmName.SHA256);
            var key = pbkdf2.GetBytes(KeyLength);
            return (Convert.ToBase64String(key), Convert.ToBase64String(salt));
        }

        public static bool Verify(string plain, string hashBase64, string saltBase64)
        {
            if (hashBase64 == null || saltBase64 == null) return false;
            var salt = Convert.FromBase64String(saltBase64);
            using var pbkdf2 = new Rfc2898DeriveBytes(plain, salt, Iterations, HashAlgorithmName.SHA256);
            var key = pbkdf2.GetBytes(KeyLength);
            var candidate = Convert.ToBase64String(key);
            // constant-time comparison
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(candidate),
                Convert.FromBase64String(hashBase64)
            );
        }
    }
}
