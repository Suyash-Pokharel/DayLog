// Services/Interfaces/IAuthService.cs
using System.Threading.Tasks;

namespace DayLog.Services.Interfaces
{
    /// <summary>
    /// Simple auth service for assignment: checks plaintext credentials
    /// stored in the local DB and manages sign-in/out via ISessionService.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Attempt to sign in with username and password.
        /// Returns true when credentials matched and session created.
        /// </summary>
        Task<bool> LoginAsync(string username, string password);

        /// <summary>
        /// Sign the current user out.
        /// </summary>
        Task LogoutAsync();

        /// <summary>
        /// Returns the current signed-in username (or null).
        /// </summary>
        string? CurrentUsername { get; }

        /// <summary>
        /// Returns whether a user is currently signed in.
        /// </summary>
        Task<bool> IsLoggedInAsync();
    }
}
