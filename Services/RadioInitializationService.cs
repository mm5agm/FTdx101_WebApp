using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using FTdx101_WebApp.Hubs;
using System.Diagnostics; // Place at the top of the file if not already present

namespace FTdx101_WebApp.Services
{
    public class RadioInitializationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<RadioHub> _hubContext;
        private readonly BrowserLauncher _browserLauncher;

        public RadioInitializationService(
            IServiceProvider serviceProvider,
            IHubContext<RadioHub> hubContext,
            BrowserLauncher browserLauncher)
        {
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
            _browserLauncher = browserLauncher;
        }

        public async Task InitializeRadioAsync()
        {
            await ExecuteInitializationAsync(CancellationToken.None);
        }

        private async Task ExecuteInitializationAsync(CancellationToken stoppingToken)
        {
            ILogger<RadioInitializationService>? logger = null; // Make logger nullable
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();
                var multiplexer = scope.ServiceProvider.GetRequiredService<CatMultiplexerService>();
                var radioStateService = scope.ServiceProvider.GetRequiredService<RadioStateService>();
                var statePersistence = scope.ServiceProvider.GetRequiredService<RadioStatePersistenceService>();
                logger = scope.ServiceProvider.GetRequiredService<ILogger<RadioInitializationService>>();

                var settings = await settingsService.GetSettingsAsync();

                // Check if COM port is configured - if not, redirect to Settings
                if (string.IsNullOrWhiteSpace(settings.SerialPort) || settings.SerialPort == "Not Set")
                {
                    logger.LogWarning("[RadioInitializationService] No COM port configured - redirecting to Settings");
                    AppStatus.InitializationStatus = "error";
                    await _hubContext.Clients.All.SendAsync("ShowSettingsPage");
                    if (!Debugger.IsAttached &&
                        string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Production", StringComparison.OrdinalIgnoreCase))
                    {
                        _browserLauncher.OpenOnce("http://localhost:8080/Settings");
                    }
                    return;
                }

                logger.LogInformation("Attempting to connect to radio on port {SerialPort} at baud {BaudRate}", settings.SerialPort, settings.BaudRate);

                try
                {
                    await multiplexer.ConnectAsync(settings.SerialPort, settings.BaudRate);
                }
                catch (Exception connEx)
                {
                    // COM port error (wrong port, port in use, etc.) - go to Settings
                    logger.LogError(connEx, "[RadioInitializationService] Failed to open COM port {SerialPort} - redirecting to Settings", settings.SerialPort);
                    AppStatus.InitializationStatus = "error";
                    await _hubContext.Clients.All.SendAsync("ShowSettingsPage");
                    if (!Debugger.IsAttached &&
                        string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Production", StringComparison.OrdinalIgnoreCase))
                    {
                        _browserLauncher.OpenOnce("http://localhost:8080/Settings");
                    }
                    return;
                }

                logger.LogInformation("Disabling auto information...");
                await multiplexer.DisableAutoInformationAsync();

                logger.LogInformation("Sending FA; command...");
                var faResponse = await multiplexer.SendCommandAsync("FA;", "Initialization", stoppingToken);
                if (string.IsNullOrWhiteSpace(faResponse) || !faResponse.StartsWith("FA"))
                {
                    // Radio not responding - likely OFF. Stay on Index page, user can turn it on.
                    logger.LogWarning("[RadioInitializationService] Radio not responding (probably OFF). User can power on via UI.");
                    radioStateService.RadioPowerOn = false;
                    AppStatus.InitializationStatus = "radio_off";
                    await _hubContext.Clients.All.SendAsync("InitializationStatus", "radio_off");

                    // Open main page (not Settings) - user can turn radio on
                    if (!Debugger.IsAttached && 
                        string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Production", StringComparison.OrdinalIgnoreCase))
                    {
                        _browserLauncher.OpenOnce("http://localhost:8080");
                    }
                    return;
                }

                // Radio responded - it's ON
                radioStateService.RadioPowerOn = true;
                logger.LogInformation("[RadioInitializationService] Radio responded to FA;: {Response}", faResponse);

                // Send initialization commands and wait for DT0 response (with timeout)
                logger.LogInformation("[RadioInitializationService] Sending full initialization sequence and waiting for DT0...");

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, cts.Token);

                try
                {
                    await multiplexer.InitializeRadioAsync();
                    logger.LogInformation("[RadioInitializationService] ✓ DT0 received, initialization sequence complete.");
                }
                catch (OperationCanceledException) when (cts.IsCancellationRequested)
                {
                    logger.LogWarning("[RadioInitializationService] ⚠ Timeout waiting for DT0 response - continuing anyway");
                }

                // 2. Load persisted state from .json
                var persistedState = statePersistence.Load();
                logger.LogInformation("[RadioInitializationService] Persisted values before initialization: " +
                    "ModeA={ModeA}, ModeB={ModeB}, Power={Power}, AntennaA={AntennaA}, AntennaB={AntennaB}, MicGain={MicGain}",
                    persistedState.ModeA, persistedState.ModeB, persistedState.Power, persistedState.AntennaA, persistedState.AntennaB, persistedState.MicGain);

                // 3. Send only non-empty/non-zero values to the radio
                if (!string.IsNullOrEmpty(persistedState.ModeA))
                {
                    logger.LogInformation("About to send ModeA={ModeA} to radio", persistedState.ModeA);
                    await multiplexer.SendCommandAsync(CatCommands.FormatMode(persistedState.ModeA, false), "Initialization", stoppingToken);
                    radioStateService.ModeA = persistedState.ModeA;
                }
                if (!string.IsNullOrEmpty(persistedState.ModeB))
                {
                    await multiplexer.SendCommandAsync(CatCommands.FormatMode(persistedState.ModeB, true), "Initialization", stoppingToken);
                    radioStateService.ModeB = persistedState.ModeB;
                }
                if (persistedState.Power > 0)
                {
                    await multiplexer.SendCommandAsync($"PC{persistedState.Power};", "Initialization", stoppingToken);
                    radioStateService.Power = persistedState.Power;
                }
                if (!string.IsNullOrEmpty(persistedState.AntennaA))
                {
                    await multiplexer.SendCommandAsync($"AN0{persistedState.AntennaA};", "Initialization", stoppingToken);
                    radioStateService.AntennaA = persistedState.AntennaA;
                }
                if (!string.IsNullOrEmpty(persistedState.AntennaB))
                {
                    await multiplexer.SendCommandAsync($"AN1{persistedState.AntennaB};", "Initialization", stoppingToken);
                    radioStateService.AntennaB = persistedState.AntennaB;
                }
                // Restore AF Gain
                if (persistedState.AfGainA >= 0 && persistedState.AfGainA <= 255)
                {
                    await multiplexer.SendCommandAsync($"AG0{persistedState.AfGainA:D3};", "Initialization", stoppingToken);
                    radioStateService.AfGainA = persistedState.AfGainA;
                }
                if (persistedState.AfGainB >= 0 && persistedState.AfGainB <= 255)
                {
                    await multiplexer.SendCommandAsync($"AG1{persistedState.AfGainB:D3};", "Initialization", stoppingToken);
                    radioStateService.AfGainB = persistedState.AfGainB;
                }
                // Restore MIC Gain
                if (persistedState.MicGain >= 0 && persistedState.MicGain <= 100)
                {
                    await multiplexer.SendCommandAsync($"MG{persistedState.MicGain:D3};", "Initialization", stoppingToken);
                    radioStateService.MicGain = persistedState.MicGain;
                }

                // 4. Read actual radio state (frequencies, band, etc.) before marking initialized
                logger.LogInformation("[RadioInitializationService] Reading actual radio state...");

                // Query VFO A frequency
                var faFreqResponse = await multiplexer.SendCommandAsync("FA;", "Initialization", stoppingToken);
                if (!string.IsNullOrWhiteSpace(faFreqResponse) && faFreqResponse.StartsWith("FA"))
                {
                    var freqStr = faFreqResponse.Substring(2).TrimEnd(';');
                    if (int.TryParse(freqStr, out int freqHz))
                    {
                        radioStateService.FrequencyA = freqHz;
                        logger.LogInformation("[RadioInitializationService] VFO A frequency: {FreqHz} Hz", freqHz);
                    }
                }

                // Query VFO B frequency  
                var fbFreqResponse = await multiplexer.SendCommandAsync("FB;", "Initialization", stoppingToken);
                if (!string.IsNullOrWhiteSpace(fbFreqResponse) && fbFreqResponse.StartsWith("FB"))
                {
                    var freqStr = fbFreqResponse.Substring(2).TrimEnd(';');
                    if (int.TryParse(freqStr, out int freqHz))
                    {
                        radioStateService.FrequencyB = freqHz;
                        logger.LogInformation("[RadioInitializationService] VFO B frequency: {FreqHz} Hz", freqHz);
                    }
                }

                // Query TX VFO (FT0 = VFO A is TX, FT1 = VFO B is TX)
                var ftResponse = await multiplexer.SendCommandAsync("FT;", "Initialization", stoppingToken);
                if (!string.IsNullOrWhiteSpace(ftResponse) && ftResponse.StartsWith("FT"))
                {
                    var txVfoStr = ftResponse.Substring(2).TrimEnd(';');
                    if (int.TryParse(txVfoStr, out int txVfo))
                    {
                        radioStateService.TxVfo = txVfo;
                        logger.LogInformation("[RadioInitializationService] TX VFO: {TxVfo} ({VfoName})", txVfo, txVfo == 0 ? "VFO A" : "VFO B");
                    }
                }

                // 5. Set IsInitialized = true FIRST to allow property changes to be persisted and broadcast
                radioStateService.IsInitialized = true;

                // Derive bands from the actual frequencies (must be AFTER IsInitialized = true)
                var bandA = radioStateService.GetBandFromFrequency(radioStateService.FrequencyA);
                var bandB = radioStateService.GetBandFromFrequency(radioStateService.FrequencyB);
                logger.LogInformation("[RadioInitializationService] Calculated bands from frequencies: A={BandA} ({FreqA} Hz), B={BandB} ({FreqB} Hz)",
                    bandA, radioStateService.FrequencyA, bandB, radioStateService.FrequencyB);

                radioStateService.SetBand("A", bandA);
                radioStateService.SetBand("B", bandB);
                logger.LogInformation("[RadioInitializationService] Bands set: A={BandA}, B={BandB}", bandA, bandB);

                // 6. Enable auto information
                await multiplexer.EnableAutoInformationAsync();

                logger.LogInformation("[RadioInitializationService] ✓ Radio connected, initialized, and Auto Information streaming enabled");
                AppStatus.InitializationStatus = "complete";
                logger.LogInformation("[RadioInitializationService] InitializationStatus set to complete");
                await _hubContext.Clients.All.SendAsync("InitializationStatus", "complete");

                // On success, open main page only in Production and not under debugger
                if (!Debugger.IsAttached && 
                    string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Production", StringComparison.OrdinalIgnoreCase))
                {
                    _browserLauncher.OpenOnce("http://localhost:8080");
                }
            }
            catch (Exception ex)
            {
                AppStatus.InitializationStatus = "error";
                logger?.LogError(ex, "[RadioInitializationService] Radio initialization failed");
                await _hubContext.Clients.All.SendAsync("InitializationStatus", "error");
                await _hubContext.Clients.All.SendAsync("ShowSettingsPage");

                // On failure, open settings page only in Production and not under debugger
                if (!Debugger.IsAttached &&
                    string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Production", StringComparison.OrdinalIgnoreCase))
                {
                    _browserLauncher.OpenOnce("http://localhost:8080/Settings");
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await ExecuteInitializationAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                // Prevent app crash - log the error and set status
                Console.WriteLine($"[RadioInitializationService] Fatal error: {ex.Message}");
                AppStatus.InitializationStatus = "error";

                // Try to open Settings page even if initialization completely failed
                try
                {
                    _browserLauncher.OpenOnce("http://localhost:8080/Settings");
                }
                catch { /* Ignore browser launch errors */ }
            }
        }
    }
}