// Services/Interfaces/ISessionService.cs
using System.Threading.Tasks;

namespace DayLog.Services.Interfaces
{
    public interface ISessionService
    {
        bool IsLoggedIn { get; }
        string? Username { get; }

        Task InitializeAsync(); // loads session file
        Task SignInAsync(string username);
        Task SignOutAsync();
    }
}
