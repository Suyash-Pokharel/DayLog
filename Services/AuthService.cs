// Services/AuthService.cs
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using DayLog.Common;
using DayLog.Services.Interfaces;
using DayLog.Data;       // only if using DB fallback
using DayLog.Entities;  // only if using DB fallback
using Microsoft.EntityFrameworkCore;

namespace DayLog.Services
{
    public class AuthService : IAuthService
    {
        private const string SecureKey = "daylog_pin_hash"; // key for secure storage
        private const string SecureSaltKey = "daylog_pin_salt";

        private readonly AppDbContext? _db; // optional fallback

        public AuthService(AppDbContext? db = null)
        {
            _db = db;
        }

        public async Task<bool> HasPinAsync()
        {
            try
            {
                var hash = await SecureStorage.Default.GetAsync(SecureKey);
                return !string.IsNullOrEmpty(hash);
            }
            catch
            {
                // On platforms where SecureStorage fails or permissions missing,
                // you might fallback to DB check if you implemented local storage
                if (_db != null)
                    return await _db.UserAuths.AnyAsync();
                return false;
            }
        }

        public async Task<ServiceResult<bool>> SetPinAsync(string pin)
        {
            try
            {
                var (hash, salt) = DayLog.Common.PasswordHasher.Hash(pin);
                await SecureStorage.Default.SetAsync(SecureKey, hash);
                await SecureStorage.Default.SetAsync(SecureSaltKey, salt);

                // optional: persist a minimal record in DB (only hashed values) if you need a backup
                if (_db != null)
                {
                    var existing = await _db.UserAuths.FirstOrDefaultAsync();
                    if (existing == null)
                    {
                        existing = new UserAuth { PinHash = hash, PinSalt = salt, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
                        _db.UserAuths.Add(existing);
                    }
                    else
                    {
                        existing.PinHash = hash;
                        existing.PinSalt = salt;
                        existing.UpdatedAt = DateTime.UtcNow;
                        _db.UserAuths.Update(existing);
                    }
                    await _db.SaveChangesAsync();
                }

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Fail(ex.Message);
            }
        }

        public async Task<ServiceResult<bool>> VerifyPinAsync(string pin)
        {
            try
            {
                string? hash = null, salt = null;
                try
                {
                    hash = await SecureStorage.Default.GetAsync(SecureKey);
                    salt = await SecureStorage.Default.GetAsync(SecureSaltKey);
                }
                catch
                {
                    // ignore; will fallback to DB below if present
                }

                if (string.IsNullOrEmpty(hash) && _db != null)
                {
                    // fallback: read first UserAuth row
                    var ua = await _db.UserAuths.FirstOrDefaultAsync();
                    if (ua != null)
                    {
                        hash = ua.PinHash;
                        salt = ua.PinSalt;
                    }
                }

                if (string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(salt))
                    return ServiceResult<bool>.Fail("No PIN set");

                var ok = PasswordHasher.Verify(pin, hash, salt);
                return ok ? ServiceResult<bool>.Ok(true) : ServiceResult<bool>.Fail("Invalid PIN");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Fail(ex.Message);
            }
        }

        public async Task<ServiceResult<bool>> RemovePinAsync()
        {
            try
            {
                try
                {
                    SecureStorage.Default.Remove(SecureKey);
                    SecureStorage.Default.Remove(SecureSaltKey);
                }
                catch { /* ignore */ }

                if (_db != null)
                {
                    var all = _db.UserAuths.ToList();
                    _db.UserAuths.RemoveRange(all);
                    await _db.SaveChangesAsync();
                }

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Fail(ex.Message);
            }
        }
    }
}
