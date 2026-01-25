// MauiProgram.cs
using DayLog.Data;
using DayLog.Services;
using DayLog.Services.Interfaces;
using DayLog.Services.JournalService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;
using System;
using System.IO;
using System.Linq;

namespace DayLog
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(f => f.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"));

            // Build SQLite path in app data
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "daylog.db");
            var connStr = $"Data Source={dbPath}";

            // Register EF Core DbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(connStr);
            });

            // Session service (singleton so session state is shared app-wide)
            builder.Services.AddSingleton<ISessionService, SessionService>();

            // Auth service (depends on session)
            builder.Services.AddScoped<IAuthService, AuthService>();

            // Journal service and other app services
            builder.Services.AddScoped<IJournalService, JournalService>();

            // Add Blazor (keep this last among service registrations)
            builder.Services.AddMauiBlazorWebView();
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
#endif

            var app = builder.Build();

            // Ensure DB created at startup (prototype mode) and initialize session
            using (var scope = app.Services.CreateScope())
            {
                // ensure DB and seed a demo user if none exist
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();

                if (!db.UserAuths.Any())
                {
                    db.UserAuths.Add(new DayLog.Entities.UserAuth
                    {
                        Username = "student",
                        Password = "1234" // change for testing if desired
                    });
                    db.SaveChanges();
                }

                // initialize session service so logged-in state is read on startup
                var session = scope.ServiceProvider.GetRequiredService<ISessionService>();
                //session.InitializeAsync().GetAwaiter().GetResult(); // synchronous init at startup (assignment OK)
            }

            return app;
        }
    }
}
