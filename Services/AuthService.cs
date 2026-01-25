// Services/AuthService.cs
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DayLog.Data;
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

        public AuthService(AppDbContext db, ISessionService session)
        {
            _db = db;
            _session = session;
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

        public Task<bool> IsLoggedInAsync()
        {
            return Task.FromResult(_session.IsLoggedIn);
        }
    }
}
