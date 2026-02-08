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

                AppStatus.InitializationStatus = "initializing";
                logger.LogInformation("[RadioInitializationService] InitializationStatus set to 'initializing'");
                await _hubContext.Clients.All.SendAsync("InitializationStatus", "Initializing radio, please wait...");

                var settings = await settingsService.GetSettingsAsync();

                AppStatus.InitializationStatus = "Connecting to radio...";
                logger.LogInformation("[RadioInitializationService] InitializationStatus set to 'Connecting to radio...'");
                await _hubContext.Clients.All.SendAsync("InitializationStatus", "Connecting to radio...");
                await multiplexer.ConnectAsync(settings.SerialPort, settings.BaudRate);

                // Send AI0 to stop auto information
                await multiplexer.SendCommand("AI0;", false);

                // Query VFO A to check radio responsiveness with timeout
                logger.LogInformation("[RadioInitializationService] Sending FA; to check radio responsiveness...");
                var faResponse = await multiplexer.SendCommandAsync("FA;", "InitialValues", stoppingToken);
                logger.LogInformation("[RadioInitializationService] FA; response: {Response}", faResponse);

                if (string.IsNullOrWhiteSpace(faResponse) || !faResponse.StartsWith("FA"))
                {
                    logger?.LogError("[RadioInitializationService] No valid response from radio to FA; command after COM port connection.");
                    throw new Exception("No valid response from radio to FA; command after COM port connection.");
                }

                AppStatus.InitializationStatus = "Initializing radio...";
                logger.LogInformation("[RadioInitializationService] InitializationStatus set to 'Initializing radio...'");
                await _hubContext.Clients.All.SendAsync("InitializationStatus", "Initializing radio...");

                logger.LogInformation("[RadioInitializationService] Calling InitializeRadioAsync...");
                await multiplexer.InitializeRadioAsync();
                logger.LogInformation("[RadioInitializationService] InitializeRadioAsync completed");

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
