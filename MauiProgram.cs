using Microsoft.EntityFrameworkCore;
using DayLog.Data;
using DayLog.Services;
using DayLog.Services.Interfaces;
using Microsoft.Maui.Storage;

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

            // Register application services (single place)
            // Keep exactly one implementation for IJournalService
            builder.Services.AddScoped<IJournalService, JournalService>();

            // Register AuthService (uses SecureStorage, optionally DB fallback)
            builder.Services.AddScoped<IAuthService, AuthService>();

            // Add any other services you need
            // builder.Services.AddScoped<ITagService, TagService>();
            // builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
#endif

            var app = builder.Build();

            // Ensure DB created at startup (prototype mode)
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            }

            return app;
        }
    }
}
