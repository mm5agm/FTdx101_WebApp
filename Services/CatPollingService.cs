using FTdx101MP_WebApp.Models;

namespace FTdx101MP_WebApp.Services
{
    public class CatPollingService : BackgroundService
    {
        private readonly ICatClient _catClient;
        private readonly IRigStateService _rigStateService;
        private readonly ISettingsService _settingsService;
        private readonly ILogger<CatPollingService> _logger;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromMilliseconds(500);

        public CatPollingService(
            ICatClient catClient,
            IRigStateService rigStateService,
            ISettingsService settingsService,
            ILogger<CatPollingService> logger)
        {
            _catClient = catClient;
            _rigStateService = rigStateService;
            _settingsService = settingsService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CAT Polling Service starting...");

            var settings = await _settingsService.GetSettingsAsync();
            var portName = settings.SerialPort;
            var baudRate = settings.BaudRate;

            var connected = await _catClient.ConnectAsync(portName, baudRate);
            if (!connected)
            {
                _logger.LogError("Failed to connect to CAT interface on {PortName} at {BaudRate} baud", portName, baudRate);
                return;
            }

            _logger.LogInformation("CAT Polling Service started on {PortName} at {BaudRate} baud", portName, baudRate);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var sMeter = await _catClient.ReadSMeterAsync();
                    var frequency = await _catClient.ReadFrequencyAsync();
                    var mode = await _catClient.ReadModeAsync();
                    var isTransmitting = await _catClient.ReadTransmitStatusAsync();

                    var rigState = new RigState
                    {
                        SMeterLevel = sMeter,
                        Frequency = frequency,
                        Mode = mode,
                        IsTransmitting = isTransmitting
                    };

                    _rigStateService.UpdateState(rigState);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error polling CAT interface");
                }

                await Task.Delay(_pollingInterval, stoppingToken);
            }

            await _catClient.DisconnectAsync();
            _logger.LogInformation("CAT Polling Service stopped");
        }
    }
}