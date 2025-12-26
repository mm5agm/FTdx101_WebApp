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
            builder.Services.AddControllers(); // Enable API Controllers

            // Register Settings Service
            builder.Services.AddSingleton<ISettingsService, SettingsService>();

            // Register CAT services
            builder.Services.AddSingleton<ICatClient, SerialPortCatClient>();
            builder.Services.AddSingleton<IRigStateService, RigStateService>();

            
            // Register Background Service
            builder.Services.AddHostedService<CatPollingService>();  // ? RE-ENABLE THIS
            // Load settings to configure web server
            var tempServiceProvider = builder.Services.BuildServiceProvider();
            var settingsService = tempServiceProvider.GetRequiredService<ISettingsService>();
            var settings = await settingsService.GetSettingsAsync();

            // Configure web server URL based on settings (HTTP only - simpler and works everywhere)
            var webAddress = settings.WebAddress == "localhost" ? "localhost" : "0.0.0.0"; // 0.0.0.0 means all interfaces
            var httpUrl = $"http://{webAddress}:{settings.WebPort}";

            builder.WebHost.UseUrls(httpUrl);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            // No HTTPS redirect needed - we're HTTP only

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllers(); // Map API Controller routes
            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("========================================");
            logger.LogInformation("📡 FT-dx101MP Web Server Started");
            logger.LogInformation("========================================");

            if (settings.WebAddress == "localhost")
            {
                logger.LogInformation("🏠 Local Access:  http://localhost:{Port}", settings.WebPort);
            }
            else
            {
                logger.LogInformation("🏠 Local Access:  http://localhost:{Port}", settings.WebPort);
                logger.LogInformation("🌐 Network Access: http://{IPAddress}:{Port}", settings.WebAddress, settings.WebPort);
            }

            logger.LogInformation("🔌 API Endpoint:  http://localhost:{Port}/api/cat/status", settings.WebPort);
            logger.LogInformation("========================================");

            // Register cleanup on application shutdown (OPTION 1)
            var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStopping.Register(() =>
            {
                logger.LogInformation("⚠️ Application stopping - releasing COM port...");
                
                var catClient = app.Services.GetRequiredService<ICatClient>();
                if (catClient is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                
                logger.LogInformation("✅ COM port released successfully");
            });

            // Start the web server in the background
            _ = Task.Run(() => app.Run());

            // Wait a moment for the server to start
            await Task.Delay(1000);

            // Open the browser automatically
            var browserUrl = $"http://localhost:{settings.WebPort}";
            OpenBrowser(browserUrl);
            logger.LogInformation("🌐 Browser opened: {Url}", browserUrl);

            // Keep the application running
            await Task.Delay(-1);
        }

        /// <summary>
        /// Opens the default web browser with the specified URL
        /// </summary>
        private static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // Fallback for different operating systems
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
        }
    }
}