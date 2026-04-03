using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
/*
| Function | EX Command | What It Returns | Notes |
| --- | --- | --- | --- |
| **S‑Meter (Main VFO)** | ``EX01020300`` | S‑meter level (0–255) | Most‑polled meter value. |
| **S‑Meter (Sub VFO)** | ``EX01020301`` | Sub‑receiver S‑meter | Only valid if SUB RX enabled. |
| **PO – Power Output** | ``EX01020400`` | TX power output (0–255) | Scaled to rig’s power range. |
| **ALC Level** | ``EX01020500`` | ALC meter reading | Useful for digital mode monitoring. |
| **SWR** | ``EX01020600`` | Standing Wave Ratio | Returns raw meter value, not SWR ratio. |
| **ID – Current Draw** | ``EX01020700`` | Current consumption | Raw ADC value. |
| **VDD – Supply Voltage** | ``EX01020800`` | DC input voltage | Good for PSU monitoring. |
| **Temperature (PA Unit)** | ``EX01020900`` | PA temperature | Raw value; rises under TX load. |
| **COMP – Speech Compression** | ``EX01020A00`` | Compression level | Only active in SSB with COMP on. |
| **MIC – Mic Level** | ``EX01020B00`` | Mic input level | Useful for audio diagnostics. |
| **RF – RF Drive Level** | ``EX01020C00`` | RF drive meter | Reflects drive setting. |
| **IF Width / Shift Meter** | ``EX01020D00`` | IF DSP meter | Rarely used by logging software. |
| **AGC Meter** | ``EX01020E00`` | AGC action level | Useful for DSP behaviour analysis. |
| **NB Level** | ``EX01020F00`` | Noise blanker meter | Shows NB activity. |
*/
namespace FTdx101_WebApp.Services
{
    /// <summary>
    /// Background service that polls S-meters, power, and SWR independently of AI mode
    /// </summary>
    public class MeterPollingService : BackgroundService
    {
        private readonly CatMultiplexerService _multiplexer;
        private readonly RadioStateService _stateService;
        private readonly ILogger<MeterPollingService> _logger;

        public MeterPollingService(
            CatMultiplexerService multiplexer,
            RadioStateService stateService,
            ILogger<MeterPollingService> logger)
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
                    _logger.LogInformation("[MeterPolling][DEBUG] Polling TX status...");
                    var txResponse = await _multiplexer.SendCommandAsync("TX;", "MeterPoll", stoppingToken);
                    _logger.LogInformation("[MeterPolling][DEBUG] TX response: {0}", txResponse);
                    bool isTransmitting = !string.IsNullOrEmpty(txResponse) && txResponse.Contains("TX1");
                    _stateService.IsTransmitting = isTransmitting;
                    _logger.LogInformation("[MeterPolling] TX poll: raw='{Raw}', isTransmitting={IsTransmitting}", txResponse, isTransmitting);

                    _logger.LogInformation("[MeterPolling][DEBUG] Polling S-Meter A...");
                    var smAResponse = await _multiplexer.SendCommandAsync(CatCommands.SMeterMain + ";", "MeterPoll", stoppingToken);
                    _logger.LogInformation("[MeterPolling][DEBUG] S-Meter A response: {0}", smAResponse);
                    int sMeterA = CatCommands.ParseSMeter(smAResponse ?? "");
                    _stateService.SMeterA = sMeterA;

                    _logger.LogInformation("[MeterPolling][DEBUG] Polling S-Meter B...");
                    var smBResponse = await _multiplexer.SendCommandAsync(CatCommands.SMeterSub + ";", "MeterPoll", stoppingToken);
                    _logger.LogInformation("[MeterPolling][DEBUG] S-Meter B response: {0}", smBResponse);
                    int sMeterB = CatCommands.ParseSMeter(smBResponse ?? "");
                    _stateService.SMeterB = sMeterB;

                    _logger.LogInformation("[MeterPolling][DEBUG] Polling Power Meter...");
                    var powerResponse = await _multiplexer.SendCommandAsync(CatCommands.MeterPower + ";", "MeterPoll", stoppingToken);
                    _logger.LogInformation("[MeterPolling][DEBUG] Power Meter response: {0}", powerResponse);
                    int power = CatCommands.ParseMeterReading(powerResponse ?? "");
                    _logger.LogInformation("[MeterPolling][DEBUG] TX={IsTransmitting} Power raw='{Raw}', parsed={Value}", isTransmitting, powerResponse, power);
                    if (isTransmitting)
                    {
                        _stateService.PowerMeter = power;
                        _logger.LogInformation("[MeterPolling] Power meter (TX): raw='{Raw}', value={Value}", powerResponse, power);
                    }
                    else
                    {
                        // Always broadcast PowerMeter=0 when not transmitting
                        if (_stateService.PowerMeter != 0)
                        {
                            _stateService.PowerMeter = 0;
                            _logger.LogInformation("[MeterPolling] Power meter (not TX): value=0");
                        }
                    }

                    _logger.LogInformation("[MeterPolling][DEBUG] Polling SWR Meter... TX={IsTransmitting}", isTransmitting);
                    var swrResponse = await _multiplexer.SendCommandAsync(CatCommands.MeterSWR + ";", "MeterPoll", stoppingToken);
                    _logger.LogInformation("[MeterPolling][DebugSWR] TX={IsTransmitting} RM6 raw response: '{Raw}'", isTransmitting, swrResponse);
                    int swr = CatCommands.ParseMeterReading(swrResponse ?? "");
                    _logger.LogInformation("[MeterPolling][DebugSWR] TX={IsTransmitting} RM6 parsed value: {Value}", isTransmitting, swr);
                    _stateService.SWRMeter = swr;

                    _logger.LogInformation("[MeterPolling][DEBUG] Polling Compression Meter...");
                    var compResponse = await _multiplexer.SendCommandAsync(CatCommands.MeterComp + ";", "MeterPoll", stoppingToken);
                    _logger.LogInformation("[MeterPolling][DEBUG] Compression Meter response: {0}", compResponse);
                    int compression = CatCommands.ParseMeterReading(compResponse ?? "");
                    _stateService.CompressionMeter = compression;

                    _logger.LogInformation("[MeterPolling][DEBUG] Polling IDD Meter...");
                    var iddResponse = await _multiplexer.SendCommandAsync(CatCommands.MeterIDD + ";", "MeterPoll", stoppingToken);
                    _logger.LogInformation("[MeterPolling][DEBUG] IDD Meter response: {0}", iddResponse);
                    int idd = CatCommands.ParseMeterReading(iddResponse ?? "");
                    _stateService.IDDMeter = idd;

                    _logger.LogInformation("[MeterPolling][DEBUG] Polling ALC Meter...");
                    var alcResponse = await _multiplexer.SendCommandAsync(CatCommands.MeterALC + ";", "MeterPoll", stoppingToken);
                    _logger.LogInformation("[MeterPolling][DEBUG] ALC Meter response: {0}", alcResponse);
                    int alc = CatCommands.ParseMeterReading(alcResponse ?? "");
                    _stateService.ALCMeter = alc;

                    _logger.LogInformation("[MeterPolling][DEBUG] Polling VDD Meter...");
                    var vddResponse = await _multiplexer.SendCommandAsync(CatCommands.MeterVDD + ";", "MeterPoll", stoppingToken);
                    _logger.LogInformation("[MeterPolling][DEBUG] VDD Meter response: {0}", vddResponse);
                    int vdd = CatCommands.ParseMeterReading(vddResponse ?? "");
                    _stateService.VDDMeter = vdd;

                    _logger.LogInformation("[MeterPolling][DEBUG] Polling Temperature...");
                    var tempResponse = await _multiplexer.SendCommandAsync(CatCommands.MeterTemp + ";", "MeterPoll", stoppingToken);
                    _logger.LogInformation("[MeterPolling][DEBUG] Temperature response: {0}", tempResponse);
                    int temp = CatCommands.ParseMeterReading(tempResponse ?? "");
                    _stateService.Temperature = temp;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[MeterPolling][FATAL] Exception in polling loop: {Message}\nStackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                }
                await Task.Delay(500, stoppingToken); // Poll every 500ms
            }
        }
    }
}
