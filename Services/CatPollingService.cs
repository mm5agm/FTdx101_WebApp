using FTdx101_WebApp.Models;

namespace FTdx101_WebApp.Services
{
    public class CatPollingService : BackgroundService
    {
        private readonly ICatClient _catClient;
        private readonly IRigStateService _rigStateService;
        private readonly ISettingsService _settingsService;
        private readonly ILogger<CatPollingService> _logger;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromMilliseconds(500);
        private readonly RadioStateService _radioStateService;

        public CatPollingService(
            ICatClient catClient,
            IRigStateService rigStateService,
            ISettingsService settingsService,
            ILogger<CatPollingService> logger,
            RadioStateService radioStateService)
        {
            _catClient = catClient;
            _rigStateService = rigStateService;
            _settingsService = settingsService;
            _logger = logger;
            _radioStateService = radioStateService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CAT Polling Service starting...");

            // Do not call ConnectAsync or DisconnectAsync here.
            _logger.LogInformation("CAT Polling Service started");

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

                    var freqA = await _catClient.ReadFrequencyAAsync();
                    var freqB = await _catClient.ReadFrequencyBAsync();
                    _logger.LogInformation("Polled frequencies: A={FreqA}, B={FreqB}", freqA, freqB);

                    _radioStateService.FrequencyA = freqA;
                    _radioStateService.FrequencyB = freqB;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error polling CAT interface");
                }

                await Task.Delay(_pollingInterval, stoppingToken);
            }

            _logger.LogInformation("CAT Polling Service stopped");
        }
    }
}