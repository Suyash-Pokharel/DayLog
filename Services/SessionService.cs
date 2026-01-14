// Services/SessionService.cs
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using DayLog.Services.Interfaces;

namespace DayLog.Services
{
    public class SessionService : ISessionService
    {
        private const string SessionFileName = "session.txt";
        private readonly string _path;

        public bool IsLoggedIn { get; private set; } = false;
        public string? Username { get; private set; }

        public SessionService()
        {
            _path = Path.Combine(FileSystem.AppDataDirectory, SessionFileName);
        }

        public async Task InitializeAsync()
        {
            try
            {
                if (File.Exists(_path))
                {
                    var content = await File.ReadAllTextAsync(_path);
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        Username = content.Trim();
                        IsLoggedIn = true;
                    }
                }
            }
            catch
            {
                // ignore read errors for assignment
                IsLoggedIn = false;
                Username = null;
            }
        }

        public async Task SignInAsync(string username)
        {
            Username = username;
            IsLoggedIn = true;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
                await File.WriteAllTextAsync(_path, username);
            }
            catch
            {
                // ignore write errors for assignment
            }
        }

        public Task SignOutAsync()
        {
            Username = null;
            IsLoggedIn = false;
            try
            {
                if (File.Exists(_path)) File.Delete(_path);
            }
            catch
            {
                // ignore
            }
            return Task.CompletedTask;
        }
    }
}
