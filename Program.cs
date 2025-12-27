using FTdx101_WebApp.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FTdx101_WebApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddControllers();

            // Register Settings Service
            builder.Services.AddSingleton<ISettingsService, SettingsService>();

            // ===== CAT MULTIPLEXER ARCHITECTURE =====
            // Register the multiplexer (manages COM port)
            builder.Services.AddSingleton<CatMultiplexerService>();

            // Register multiplexed client for Web UI
            builder.Services.AddSingleton<ICatClient, MultiplexedCatClient>();

            // Register rigctld server for WSJT-X
            builder.Services.AddHostedService<RigctldServer>();

            builder.Services.AddSingleton<IRigStateService, RigStateService>();
            builder.Services.AddHostedService<CatPollingService>();

            // BUILD THE APP FIRST (creates the real service container)
            var app = builder.Build();

            // NOW we can get services from the container
            var settingsService = app.Services.GetRequiredService<ISettingsService>();
            var settings = await settingsService.GetSettingsAsync();

            // Configure web server URL
            var webAddress = settings.WebAddress == "localhost" ? "localhost" : "0.0.0.0";
            var urls = new[] { $"http://{webAddress}:{settings.WebPort}" };

            // Note: We can't change URLs after app.Build(), so we need to restart
            // For now, log a warning if settings don't match defaults
            var logger = app.Services.GetRequiredService<ILogger<Program>>();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseRouting();
            app.UseAuthorization();
            app.MapControllers();
            app.MapStaticAssets();
            app.MapRazorPages().WithStaticAssets();

            // Initialize the multiplexer from the REAL service container
            var multiplexer = app.Services.GetRequiredService<CatMultiplexerService>();
            var connected = await multiplexer.ConnectAsync(settings.SerialPort, settings.BaudRate);

            if (!connected)
            {
                logger.LogWarning("⚠️ WARNING: Failed to connect to {Port}", settings.SerialPort);
                logger.LogWarning("   The web interface will still start, but radio control will not work.");
            }

            logger.LogInformation("========================================");
            logger.LogInformation("📡 FT-dx101 CAT Multiplexer Started");
            logger.LogInformation("========================================");
            logger.LogInformation("🔌 COM Port:      {Port} @ {Baud} baud", settings.SerialPort, settings.BaudRate);
            logger.LogInformation("🌐 Web UI:        http://localhost:{Port}", settings.WebPort);
            logger.LogInformation("🔧 rigctld:       localhost:4532 (for WSJT-X)");
            logger.LogInformation("📡 API:           http://localhost:{Port}/api/cat/status", settings.WebPort);
            logger.LogInformation("========================================");

            // Register cleanup
            var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStopping.Register(() =>
            {
                logger.LogInformation("⚠️ Shutting down - releasing COM port...");
                multiplexer.Dispose();
                logger.LogInformation("✅ COM port released");
            });

            // Open browser
            OpenBrowser($"http://localhost:{settings.WebPort}");
            logger.LogInformation("🌐 Browser opened");

            // Run the web server (this blocks until shutdown)
            await app.RunAsync();
        }

        private static void OpenBrowser(string url)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
            }
            catch { }
        }
    }
}