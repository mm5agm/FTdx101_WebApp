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
                    // S-Meter A
                    var smAResponse = await _multiplexer.SendCommandAsync(CatCommands.SMeterMain + ";", "MeterPoll", stoppingToken);
                    int sMeterA = CatCommands.ParseSMeter(smAResponse ?? "");
                    _stateService.SMeterA = sMeterA;

                    // S-Meter B
                    var smBResponse = await _multiplexer.SendCommandAsync(CatCommands.SMeterSub + ";", "MeterPoll", stoppingToken);
                    int sMeterB = CatCommands.ParseSMeter(smBResponse ?? "");
                    _stateService.SMeterB = sMeterB;

                    // Power Meter
                    var powerResponse = await _multiplexer.SendCommandAsync(CatCommands.MeterPower + ";", "MeterPoll", stoppingToken);
                    int power = CatCommands.ParseMeterReading(powerResponse ?? "");
                    _stateService.PowerMeter = power;

                    // SWR Meter
                    var swrResponse = await _multiplexer.SendCommandAsync(CatCommands.MeterSWR + ";", "MeterPoll", stoppingToken);
                    int swr = CatCommands.ParseMeterReading(swrResponse ?? "");
                    _stateService.SWRMeter = swr;

                    // IDD (Drain Current)
                    var iddResponse = await _multiplexer.SendCommandAsync(CatCommands.MeterALC + ";", "MeterPoll", stoppingToken);
                    int idd = CatCommands.ParseMeterReading(iddResponse ?? "");
                    _stateService.IDDMeter = idd;

                    // VDD (PA Voltage)
                    var vddResponse = await _multiplexer.SendCommandAsync("RM8;", "MeterPoll", stoppingToken);
                    int vdd = CatCommands.ParseMeterReading(vddResponse ?? "");
                    _stateService.VDDMeter = vdd;

                    // Temperature (RM9)
                    var tempResponse = await _multiplexer.SendCommandAsync("RM9;", "MeterPoll", stoppingToken);
                    int temp = CatCommands.ParseMeterReading(tempResponse ?? "");
                    _stateService.Temperature = temp;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error polling meters");
                }
                await Task.Delay(500, stoppingToken); // Poll every 500ms
            }
        }
    }
}
    
