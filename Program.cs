using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FTdx101_WebApp.Services;

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

// Register your services
builder.Services.AddSingleton<RadioStateService>();

builder.Services.AddRazorPages();
builder.Services.AddControllers();

// ADD SIGNALR:
builder.Services.AddSignalR();

// Register the main CAT client for the web app
builder.Services.AddSingleton<ICatClient, MultiplexedCatClient>();

// Register the multiplexer and radio state services
builder.Services.AddSingleton<CatMultiplexerService>();

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

// Register the radio state service
builder.Services.AddSingleton<IRadioStateService, RadioStateService>();

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
    var radioStateService = scope.ServiceProvider.GetRequiredService<RadioStateService>();
    var statePersistence = scope.ServiceProvider.GetRequiredService<RadioStatePersistenceService>();
    var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Program>>();
    
    var settings = settingsService.GetSettingsAsync().GetAwaiter().GetResult();
    
    // CONNECT TO RADIO FIRST
    multiplexer.ConnectAsync(settings.SerialPort, settings.BaudRate).GetAwaiter().GetResult();

    // Enable Auto Information mode and query initial state
    multiplexer.EnableAutoInformationAsync().GetAwaiter().GetResult();
    
    // Give the initial query time to complete and populate reactive state
    System.Threading.Thread.Sleep(1500);
    
    logger.LogInformation("✓ Radio connected with Auto Information streaming enabled");
    
    // NOW check if we got valid data from radio, otherwise use persisted state
    var persistedState = statePersistence.Load();
    
    long initialFreqA = radioStateService.FrequencyA > 0 ? radioStateService.FrequencyA : persistedState.FrequencyA;
    long initialFreqB = radioStateService.FrequencyB > 0 ? radioStateService.FrequencyB : persistedState.FrequencyB;
    
    string bandA = "20m";
    string bandB = "20m";
    
    // If reactive state already populated from radio query, just determine bands
    if (radioStateService.FrequencyA > 0)
    {
        bandA = GetBandFromFrequency(radioStateService.FrequencyA);
        radioStateService.SetBand("A", bandA);
        logger.LogInformation("Receiver A: {Freq} Hz, Band {Band}, Mode {Mode}", 
            radioStateService.FrequencyA, bandA, radioStateService.ModeA);
    }
    else if (persistedState.FrequencyA > 0)
    {
        // Fallback to persisted state if radio query failed
        radioStateService.FrequencyA = persistedState.FrequencyA;
        radioStateService.ModeA = persistedState.ModeA ?? "USB";
        radioStateService.AntennaA = persistedState.AntennaA ?? "1";
        bandA = !string.IsNullOrEmpty(persistedState.BandA) 
            ? persistedState.BandA 
            : GetBandFromFrequency(persistedState.FrequencyA);
        radioStateService.SetBand("A", bandA);
        logger.LogInformation("Receiver A (from file): {Freq} Hz, Band {Band}", 
            persistedState.FrequencyA, bandA);
    }
    
    if (radioStateService.FrequencyB > 0)
    {
        bandB = GetBandFromFrequency(radioStateService.FrequencyB);
        radioStateService.SetBand("B", bandB);
        logger.LogInformation("Receiver B: {Freq} Hz, Band {Band}, Mode {Mode}", 
            radioStateService.FrequencyB, bandB, radioStateService.ModeB);
    }
    else if (persistedState.FrequencyB > 0)
    {
        // Fallback to persisted state if radio query failed
        radioStateService.FrequencyB = persistedState.FrequencyB;
        radioStateService.ModeB = persistedState.ModeB ?? "USB";
        radioStateService.AntennaB = persistedState.AntennaB ?? "1";
        bandB = !string.IsNullOrEmpty(persistedState.BandB) 
            ? persistedState.BandB 
            : GetBandFromFrequency(persistedState.FrequencyB);
        radioStateService.SetBand("B", bandB);
        logger.LogInformation("Receiver B (from file): {Freq} Hz, Band {Band}", 
            persistedState.FrequencyB, bandB);
    }
    
    /*
    var debugFile = Path.Combine(AppContext.BaseDirectory, "startup_debug.txt");
    System.IO.File.AppendAllText(debugFile, 
        $"{DateTime.Now:HH:mm:ss} - Receiver A: {radioStateService.FrequencyA} Hz, Band: {bandA}, Mode: {radioStateService.ModeA}\n");
    System.IO.File.AppendAllText(debugFile, 
        $"{DateTime.Now:HH:mm:ss} - Receiver B: {radioStateService.FrequencyB} Hz, Band: {bandB}, Mode: {radioStateService.ModeB}\n");
    System.IO.File.AppendAllText(debugFile, 
        $"{DateTime.Now:HH:mm:ss} - Persisted BandA: '{persistedState.BandA}', BandB: '{persistedState.BandB}'\n\n");
    */
}

app.Run();