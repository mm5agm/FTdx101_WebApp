using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace FTdx101_WebApp.Services
{
    /// <summary>
    /// Background service that polls S-meters, power, and SWR independently of AI mode
    /// </summary>
    public class SMeterPollingService : BackgroundService
    {
        private readonly CatMultiplexerService _multiplexer;
        private readonly RadioStateService _stateService;
        private readonly ILogger<SMeterPollingService> _logger;

        public SMeterPollingService(
            CatMultiplexerService multiplexer,
            RadioStateService stateService,
            ILogger<SMeterPollingService> logger)
        {
            _multiplexer = multiplexer;
            _stateService = stateService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Meter polling service started (S-Meter, Power, SWR)");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("[SMeterPolling][DEBUG] Polling TX status...");
                    var txResponse = await _multiplexer.SendCommandAsync("TX;", "MeterPoll", stoppingToken);
                    _logger.LogInformation("[SMeterPolling][DEBUG] TX response: {0}", txResponse);
                    bool isTransmitting = !string.IsNullOrEmpty(txResponse) && txResponse.Contains("TX1");
                    _stateService.IsTransmitting = isTransmitting;
                    _logger.LogInformation("[SMeterPolling] TX poll: raw='{Raw}', isTransmitting={IsTransmitting}", txResponse, isTransmitting);

                    _logger.LogInformation("[SMeterPolling][DEBUG] Polling S-Meter A...");
                    var smAResponse = await _multiplexer.SendCommandAsync(CatCommands.SMeterMain + ";", "MeterPoll", stoppingToken);
                    _logger.LogInformation("[SMeterPolling][DEBUG] S-Meter A response: {0}", smAResponse);
                    int sMeterA = CatCommands.ParseSMeter(smAResponse ?? "");
                    _stateService.SMeterA = sMeterA;

                    _logger.LogInformation("[SMeterPolling][DEBUG] Polling S-Meter B...");
                    var smBResponse = await _multiplexer.SendCommandAsync(CatCommands.SMeterSub + ";", "MeterPoll", stoppingToken);
                    _logger.LogInformation("[SMeterPolling][DEBUG] S-Meter B response: {0}", smBResponse);
                    int sMeterB = CatCommands.ParseSMeter(smBResponse ?? "");
                    _stateService.SMeterB = sMeterB;

                    _logger.LogInformation("[SMeterPolling][DEBUG] Polling Power Meter...");
                    var powerResponse = await _multiplexer.SendCommandAsync(CatCommands.MeterPower + ";", "MeterPoll", stoppingToken);
                    _logger.LogInformation("[SMeterPolling][DEBUG] Power Meter response: {0}", powerResponse);
                    int power = CatCommands.ParseMeterReading(powerResponse ?? "");
                    _logger.LogInformation("[SMeterPolling][DEBUG] TX={IsTransmitting} Power raw='{Raw}', parsed={Value}", isTransmitting, powerResponse, power);
                    if (isTransmitting)
                    {
                        _stateService.PowerMeter = power;
                        _logger.LogInformation("[SMeterPolling] Power meter (TX): raw='{Raw}', value={Value}", powerResponse, power);
                    }
                    else
                    {
                        // Always broadcast PowerMeter=0 when not transmitting
                        if (_stateService.PowerMeter != 0)
                        {
                            _stateService.PowerMeter = 0;
                            _logger.LogInformation("[SMeterPolling] Power meter (not TX): value=0");
                        }
                    }

                    _logger.LogInformation("[SMeterPolling][DEBUG] Polling SWR Meter...");
                    var swrResponse = await _multiplexer.SendCommandAsync(CatCommands.MeterSWR + ";", "MeterPoll", stoppingToken);
                    _logger.LogInformation("[SMeterPolling][DEBUG] SWR Meter response: {0}", swrResponse);
                    int swr = CatCommands.ParseMeterReading(swrResponse ?? "");
                    _logger.LogInformation("[SMeterPolling][DEBUG] SWR raw='{Raw}', parsed={Value}", swrResponse, swr);
                    _stateService.SWRMeter = swr;

                    _logger.LogInformation("[SMeterPolling][DEBUG] Polling IDD Meter...");
                    var iddResponse = await _multiplexer.SendCommandAsync(CatCommands.MeterALC + ";", "MeterPoll", stoppingToken);
                    _logger.LogInformation("[SMeterPolling][DEBUG] IDD Meter response: {0}", iddResponse);
                    int idd = CatCommands.ParseMeterReading(iddResponse ?? "");
                    _stateService.IDDMeter = idd;

                    _logger.LogInformation("[SMeterPolling][DEBUG] Polling VDD Meter...");
                    var vddResponse = await _multiplexer.SendCommandAsync("RM8;", "MeterPoll", stoppingToken);
                    _logger.LogInformation("[SMeterPolling][DEBUG] VDD Meter response: {0}", vddResponse);
                    int vdd = CatCommands.ParseMeterReading(vddResponse ?? "");
                    _stateService.VDDMeter = vdd;

                    _logger.LogInformation("[SMeterPolling][DEBUG] Polling Temperature...");
                    var tempResponse = await _multiplexer.SendCommandAsync("RM9;", "MeterPoll", stoppingToken);
                    _logger.LogInformation("[SMeterPolling][DEBUG] Temperature response: {0}", tempResponse);
                    int temp = CatCommands.ParseMeterReading(tempResponse ?? "");
                    _stateService.Temperature = temp;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[SMeterPolling][FATAL] Exception in polling loop: {Message}\nStackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                }
                await Task.Delay(500, stoppingToken); // Poll every 500ms
            }
        }
    }
}
    
