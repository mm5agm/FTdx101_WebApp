using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FTdx101_WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Register the main CAT client for the web app
builder.Services.AddSingleton<ICatClient, MultiplexedCatClient>();

// Register the multiplexer and radio state services
builder.Services.AddSingleton<CatMultiplexerService>();
builder.Services.AddSingleton<RadioStateService>();

// Register the rigctld server as a background service
builder.Services.AddHostedService<RigctldServer>();

// Register your settings service
builder.Services.AddSingleton<ISettingsService, SettingsService>();

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

// Ensure CAT multiplexer connects to the serial port at startup
using (var scope = app.Services.CreateScope())
{
    var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();
    var multiplexer = scope.ServiceProvider.GetRequiredService<CatMultiplexerService>();
    var settings = settingsService.GetSettingsAsync().GetAwaiter().GetResult();
    multiplexer.ConnectAsync(settings.SerialPort, settings.BaudRate).GetAwaiter().GetResult();

    // Optional: Log a test CAT command response for diagnostics
    var response = multiplexer.SendCommandAsync("FA;", "StartupDiag").GetAwaiter().GetResult();
    var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Program>>();
    logger.LogInformation("Startup CAT FA; response: {Response}", response);
}

app.Run();