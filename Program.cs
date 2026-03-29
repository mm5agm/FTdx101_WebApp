using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FTdx101_WebApp.Services;
using Microsoft.AspNetCore.SignalR;
using FTdx101_WebApp.Hubs; // Adjust namespace as needed
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ICalibrationService, CalibrationService>();

// ADD SIGNALR EARLY (before services that depend on IHubContext):
builder.Services.AddSignalR();

// Register the persistence service (no hub dependency)
builder.Services.AddSingleton<RadioStatePersistenceService>();

// Register RadioStateService and CatMessageBuffer as singletons
builder.Services.AddSingleton<RadioStateService>();
builder.Services.AddSingleton<CatMessageBuffer>();

// Register CatMessageDispatcher as singleton
builder.Services.AddSingleton<CatMessageDispatcher>();

// Register CatMultiplexerService as singleton
builder.Services.AddSingleton<CatMultiplexerService>();

// Register the main CAT client for the web app
builder.Services.AddSingleton<ICatClient, MultiplexedCatClient>();

// Register the rigctld server as a background service
builder.Services.AddHostedService<RigctldServer>();

// Register your settings service
builder.Services.AddSingleton<ISettingsService, SettingsService>();


// Add after existing service registrations
builder.Services.AddHostedService<SMeterPollingService>();

// Register the radio state service — reuse the same singleton instance as RadioStateService
builder.Services.AddSingleton<IRadioStateService>(sp => sp.GetRequiredService<RadioStateService>());

// Register the radio initialization service
builder.Services.AddSingleton<RadioInitializationService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<RadioInitializationService>());

// ADD THIS LINE for Razor Pages support:
builder.Services.AddRazorPages();

// Force the web host to use port 8080 on all interfaces
builder.WebHost.UseUrls("http://0.0.0.0:8080");

builder.Services.AddSingleton<BrowserLauncher>();
// builder.Services.AddHostedService<SystemTrayService>();

// Register WSJT-X UDP listener as a singleton so it can be injected into controllers
builder.Services.AddSingleton<WsjtxUdpService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<WsjtxUdpService>());

// Register process status cache service for efficient process lookups
builder.Services.AddSingleton<ProcessStatusCacheService>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddFilter((category, level) =>
{
    // Show Information and above for WsjtxUdpService
    if (!string.IsNullOrEmpty(category) && category.Contains("FTdx101_WebApp.Services.WsjtxUdpService"))
        return level >= LogLevel.Information;
    // Show Warning and above for everything else
    return level >= LogLevel.Warning;
});
builder.Logging.AddDebug();


try
{
    var app = builder.Build();

    // Middleware to force Content-Language: en on all responses
    app.Use(async (context, next) =>
    {
        context.Response.OnStarting(() => {
            if (!context.Response.Headers.ContainsKey("Content-Language"))
            {
                context.Response.Headers.Append("Content-Language", "en");
            }
            return System.Threading.Tasks.Task.CompletedTask;
        });
        await next();
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthorization();
    //app.MapGet("/", () => "ROOT ROUTE HIT");

    app.MapRazorPages();
    app.MapControllers();

    // MAP SIGNALR HUB:
    app.MapHub<FTdx101_WebApp.Hubs.RadioHub>("/radioHub");

    app.MapGet("/api/status/init", () => new { status = FTdx101_WebApp.Services.AppStatus.InitializationStatus });

    // Open browser automatically when app starts (but not when debugging in Visual Studio)
    var browserLauncher = app.Services.GetRequiredService<BrowserLauncher>();
    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStarted.Register(() =>
    {
        browserLauncher.OpenOnce("http://localhost:8080");
    });

    app.Run();
}
catch (Exception ex)
{
    var msg = $"[FATAL] Application failed to start: {ex.Message}\n{ex.StackTrace}";
    Console.Error.WriteLine(msg);
    try
    {
        System.IO.File.AppendAllText("fatal_startup_error.log", $"{DateTime.Now:u} {msg}\n");
    }
    catch { }
    throw;
}

