using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FTdx101_WebApp.Services;
using Microsoft.AspNetCore.SignalR;
using FTdx101_WebApp.Hubs; // Adjust namespace as needed
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Register RadioStateService and CatMessageBuffer as singletons
builder.Services.AddSingleton<RadioStateService>();
builder.Services.AddSingleton<CatMessageBuffer>();

// Register CatMessageDispatcher as singleton
builder.Services.AddSingleton<CatMessageDispatcher>();

// Register CatMultiplexerService as singleton
builder.Services.AddSingleton<CatMultiplexerService>();

// Register the main CAT client for the web app
builder.Services.AddSingleton<ICatClient, MultiplexedCatClient>();

// Register the persistence service
builder.Services.AddSingleton<RadioStatePersistenceService>();

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

// ADD SIGNALR:
builder.Services.AddSignalR();

// ADD THIS LINE for Razor Pages support:
builder.Services.AddRazorPages();

// Force the web host to use port 8080 on all interfaces
builder.WebHost.UseUrls("http://0.0.0.0:8080");

builder.Services.AddSingleton<BrowserLauncher>();
builder.Services.AddHostedService<SystemTrayService>();

// Register WSJT-X UDP listener as a singleton so it can be injected into controllers
builder.Services.AddSingleton<WsjtxUdpService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<WsjtxUdpService>());

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

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
    // Only launch browser if not attached to debugger (Visual Studio F5 already opens a browser)
    if (!System.Diagnostics.Debugger.IsAttached)
    {
        browserLauncher.OpenOnce("http://localhost:8080");
    }
});

app.Run();

