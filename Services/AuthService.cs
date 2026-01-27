// Services/AuthService.cs
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DayLog.Data;
using Microsoft.Extensions.Logging;
using DayLog.Entities;
using DayLog.Services.Interfaces;

namespace DayLog.Services
{
    /// <summary>
    /// Simple authentication service for assignment/demo purposes.
    /// Checks plaintext username/password against the UserAuths table
    /// and sets the session via ISessionService.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly ISessionService _session;
        private readonly Microsoft.Extensions.Logging.ILogger<AuthService> _logger;

        public AuthService(AppDbContext db, ISessionService session, Microsoft.Extensions.Logging.ILogger<AuthService> logger)
        {
            _db = db;
            _session = session;
            _logger = logger;
        }

        public string? CurrentUsername => _session.Username;

        public async Task<bool> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || password == null)
                return false;

            // simple plaintext match (assignment/demo only)
            var user = await _db.UserAuths
                                .AsNoTracking()
                                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

            if (user == null)
                return false;

            await _session.SignInAsync(user.Username);
            return true;
        }

        public async Task LogoutAsync()
        {
            await _session.SignOutAsync();
        }

        public async Task<DayLog.Common.ServiceResult<bool>> UpdateCredentialsAsync(string currentUsername, string? newUsername, string? newPassword)
        {
            if (string.IsNullOrWhiteSpace(currentUsername))
                return DayLog.Common.ServiceResult<bool>.Fail("Not signed in");

            var user = await _db.UserAuths.FirstOrDefaultAsync(u => u.Username == currentUsername);
            if (user == null)
                return DayLog.Common.ServiceResult<bool>.Fail("Current user not found");

            // If username is changing, check for duplicates
            if (!string.IsNullOrWhiteSpace(newUsername) && newUsername != currentUsername)
            {
                var exists = await _db.UserAuths.AsNoTracking().AnyAsync(u => u.Username == newUsername);
                if (exists)
                    return DayLog.Common.ServiceResult<bool>.Fail("Username already taken");
                user.Username = newUsername;
            }

            if (!string.IsNullOrEmpty(newPassword))
            {
                user.Password = newPassword; // plaintext per current app design
            }

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException dbe)
            {
                _logger.LogError(dbe, "Failed to update credentials for {User}", currentUsername);
                return DayLog.Common.ServiceResult<bool>.Fail("Failed to update credentials.");
            }

            // Update session username if changed
            if (!string.IsNullOrWhiteSpace(newUsername) && newUsername != currentUsername)
            {
                await _session.SignInAsync(user.Username);
            }

            _logger.LogInformation("Credentials updated for user {User}. UsernameChanged={UsernameChanged}", currentUsername, !string.IsNullOrWhiteSpace(newUsername) && newUsername != currentUsername);
            return DayLog.Common.ServiceResult<bool>.Ok(true);
        }

        public Task<bool> IsLoggedInAsync()
        {
            return Task.FromResult(_session.IsLoggedIn);
        }
    }
}
