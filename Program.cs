using FTdx101_WebApp.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Serilog;

namespace FTdx101_WebApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Configure Serilog FIRST (before builder)
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build())
                .WriteTo.Console()
                .WriteTo.File("Logs/app.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var builder = WebApplication.CreateBuilder(args);

            // Use Serilog for logging
            builder.Host.UseSerilog();

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddControllers();

            // Register Settings Service
            builder.Services.AddSingleton<ISettingsService, SettingsService>();

            // ===== CAT MULTIPLEXER ARCHITECTURE =====
            builder.Services.AddSingleton<CatMultiplexerService>();
            builder.Services.AddSingleton<ICatClient, MultiplexedCatClient>();
            builder.Services.AddHostedService<RigctldServer>();
            builder.Services.AddSingleton<IRigStateService, RigStateService>();
            builder.Services.AddHostedService<CatPollingService>();

            // BUILD THE APP FIRST (creates the real service container)
            var app = builder.Build();

            // NOW we can get services from the container
            var settingsService = app.Services.GetRequiredService<ISettingsService>();
            var settings = await settingsService.GetSettingsAsync();

            // Configure web server URL for LAN access
            var listenUrl = $"http://0.0.0.0:{settings.WebPort}";

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
            logger.LogInformation("🌐 Web UI:        http://{Host}:{Port}", GetLocalIPAddress(), settings.WebPort);
            logger.LogInformation("🔧 rigctld:       localhost:4532 (for WSJT-X)");
            logger.LogInformation("📡 API:           http://{Host}:{Port}/api/cat/status", GetLocalIPAddress(), settings.WebPort);
            logger.LogInformation("========================================");

            // Register cleanup
            var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStopping.Register(() =>
            {
                logger.LogInformation("⚠️ Shutting down - releasing COM port...");
                multiplexer.Dispose();
                logger.LogInformation("✅ COM port released");
            });

            // Open browser (optional, opens localhost on the server)
            OpenBrowser($"http://localhost:{settings.WebPort}");
            logger.LogInformation("🌐 Browser opened");

            // Run the web server on all interfaces for LAN access
            await app.RunAsync(listenUrl);
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

        // Helper to get the local LAN IP address for logging
        private static string GetLocalIPAddress()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !ip.ToString().StartsWith("127."))
                    {
                        return ip.ToString();
                    }
                }
            }
            catch { }
            return "localhost";
        }
    }
}