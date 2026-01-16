using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FTdx101_WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers();

// ADD SIGNALR:
builder.Services.AddSignalR();

// Register the main CAT client for the web app
builder.Services.AddSingleton<ICatClient, MultiplexedCatClient>();

// Register the multiplexer and radio state services
builder.Services.AddSingleton<CatMultiplexerService>();
builder.Services.AddSingleton<RadioStateService>();

// Register the persistence service
builder.Services.AddSingleton<RadioStatePersistenceService>();

// Register the rigctld server as a background service
builder.Services.AddHostedService<RigctldServer>();

// Register your settings service
builder.Services.AddSingleton<ISettingsService, SettingsService>();

// Add after existing service registrations
builder.Services.AddSingleton<CatMessageBuffer>();
builder.Services.AddSingleton<CatMessageDispatcher>();
builder.Services.AddHostedService<SMeterPollingService>();

// Force the web host to use port 8080 on all interfaces
builder.WebHost.UseUrls("http://0.0.0.0:8080");

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

// Ensure CAT multiplexer connects to the serial port at startup
using (var scope = app.Services.CreateScope())
{
    var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();
    var multiplexer = scope.ServiceProvider.GetRequiredService<CatMultiplexerService>();
    var settings = settingsService.GetSettingsAsync().GetAwaiter().GetResult();
    
    multiplexer.ConnectAsync(settings.SerialPort, settings.BaudRate).GetAwaiter().GetResult();

    // Enable Auto Information mode for real-time updates
    multiplexer.EnableAutoInformationAsync().GetAwaiter().GetResult();

    var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Program>>();
    logger.LogInformation("✓ Radio connected with Auto Information streaming enabled");
}

app.Run();