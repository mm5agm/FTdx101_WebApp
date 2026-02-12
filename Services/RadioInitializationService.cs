using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using FTdx101_WebApp.Hubs;

namespace FTdx101_WebApp.Services
{
    public class RadioInitializationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<RadioHub> _hubContext;

        public RadioInitializationService(IServiceProvider serviceProvider, IHubContext<RadioHub> hubContext)
        {
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ILogger<RadioInitializationService> logger = null;
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();
                var multiplexer = scope.ServiceProvider.GetRequiredService<CatMultiplexerService>();
                var radioStateService = scope.ServiceProvider.GetRequiredService<RadioStateService>();
                var statePersistence = scope.ServiceProvider.GetRequiredService<RadioStatePersistenceService>();
                logger = scope.ServiceProvider.GetRequiredService<ILogger<RadioInitializationService>>();

                var settings = await settingsService.GetSettingsAsync();

                // 1. Connect and stop auto information
                await multiplexer.ConnectAsync(settings.SerialPort, settings.BaudRate);
                await multiplexer.DisableAutoInformationAsync();

                // 2. Load persisted state from .json
                var persistedState = statePersistence.Load();
                logger.LogInformation("[RadioInitializationService] Persisted values before initialization: " +
                    "ModeA={ModeA}, ModeB={ModeB}, PowerA={PowerA}, PowerB={PowerB}, AntennaA={AntennaA}, AntennaB={AntennaB}",
                    persistedState.ModeA, persistedState.ModeB, persistedState.PowerA, persistedState.PowerB, persistedState.AntennaA, persistedState.AntennaB);

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
                if (persistedState.PowerA > 0)
                {
                    await multiplexer.SendCommandAsync($"PC{persistedState.PowerA};", "Initialization", stoppingToken);
                    radioStateService.PowerA = persistedState.PowerA;
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

                // 4. Set IsInitialized = true to allow future property changes to be persisted
                radioStateService.IsInitialized = true;

                // 5. Enable auto information
                await multiplexer.EnableAutoInformationAsync();

                logger.LogInformation("[RadioInitializationService] ✓ Radio connected, initialized, and Auto Information streaming enabled");
                AppStatus.InitializationStatus = "complete";
                logger.LogInformation("[RadioInitializationService] InitializationStatus set to complete");
                await _hubContext.Clients.All.SendAsync("InitializationStatus", "complete");
            }
            catch (Exception ex)
            {
                AppStatus.InitializationStatus = "error";
                logger?.LogError(ex, "[RadioInitializationService] Radio initialization failed");
                await _hubContext.Clients.All.SendAsync("InitializationStatus", "error");
                await _hubContext.Clients.All.SendAsync("ShowSettingsPage");
            }
        }
    }
}
