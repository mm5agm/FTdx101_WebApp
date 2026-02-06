using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FTdx101_WebApp.Services;
using Microsoft.AspNetCore.SignalR;
using FTdx101_WebApp.Hubs; // Adjust namespace as needed

// ADD THIS HELPER METHOD AT THE TOP
static string GetBandFromFrequency(long frequencyHz)
{
    return frequencyHz switch
    {
        >= 1800000 and <= 2000000 => "160m",
        >= 3500000 and <= 4000000 => "80m",
        >= 5330500 and <= 5403500 => "60m",
        >= 7000000 and <= 7300000 => "40m",
        >= 10100000 and <= 10150000 => "30m",
        >= 14000000 and <= 14350000 => "20m",
        >= 18068000 and <= 18168000 => "17m",
        >= 21000000 and <= 21450000 => "15m",
        >= 24890000 and <= 24990000 => "12m",
        >= 28000000 and <= 29700000 => "10m",
        >= 50000000 and <= 54000000 => "6m",
        >= 70000000 and <= 71000000 => "4m",
        _ => "20m" // Default
    };
}

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

// Register the radio state service
builder.Services.AddSingleton<IRadioStateService, RadioStateService>();

// Register the radio initialization service
builder.Services.AddHostedService<RadioInitializationService>();

// ADD SIGNALR:
builder.Services.AddSignalR();

// ADD THIS LINE for Razor Pages support:
builder.Services.AddRazorPages();

// Force the web host to use port 8080 on all interfaces
builder.WebHost.UseUrls("http://0.0.0.0:8080");

var app = builder.Build();
// Test: Manually trigger a [received] log
var buffer = app.Services.GetRequiredService<CatMessageBuffer>();
// buffer.AppendData("FA0142000;"); // This should log [received] FA0142000;

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

app.Run();