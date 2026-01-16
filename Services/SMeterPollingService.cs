using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace FTdx101_WebApp.Services
{
    /// <summary>
    /// Background service that polls S-meters independently of AI mode
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
            _logger.LogInformation("S-Meter polling service started (aggressive mode)");

            // Wait for initial connection
            await Task.Delay(2000, stoppingToken);

            int lastValidA = 0;
            int lastValidB = 0;
            int cycleCount = 0;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_multiplexer.IsConnected)
                    {
                        var cycleStart = stopwatch.ElapsedMilliseconds;

                        // Poll SM0 and SM1 sequentially with NO delay between
                        var sm0Response = await _multiplexer.SendCommandAsync("SM0;", "SMeterPoll", stoppingToken);

                        if (!string.IsNullOrEmpty(sm0Response))
                        {
                            int sMeterA = CatCommands.ParseSMeter(sm0Response);
                            // Only update if we got a reasonable value OR it's genuinely zero
                            if (sMeterA > 0 || (sMeterA == 0 && sm0Response.Contains("SM0000")))
                            {
                                _stateService.SMeterA = sMeterA;
                                lastValidA = sMeterA;
                            }
                            // Else keep the last valid value (don't update on parse failures)
                        }

                        var sm1Response = await _multiplexer.SendCommandAsync("SM1;", "SMeterPoll", stoppingToken);

                        if (!string.IsNullOrEmpty(sm1Response))
                        {
                            int sMeterB = CatCommands.ParseSMeter(sm1Response);
                            // CHANGE FROM:
                            // if (sMeterB >= 0)
                            
                            // TO (match SM0 validation):
                            if (sMeterB > 0 || (sMeterB == 0 && sm1Response.Contains("SM1000")))
                            {
                                _stateService.SMeterB = sMeterB;
                                lastValidB = sMeterB;
                            }
                            // Else keep the last valid value (don't update on parse failures)
                        }

                        var cycleTime = stopwatch.ElapsedMilliseconds - cycleStart;
                        
                        // Log every 50 cycles to diagnose speed
                        if (++cycleCount % 50 == 0)
                        {
                            _logger.LogInformation("S-Meter: Cycle {Count} took {Time}ms", cycleCount, cycleTime);
                        }

                        // Minimal delay - let CPU breathe but stay fast
                        await Task.Delay(200, stoppingToken);  // Moderate: ~340ms cycle = 3 updates/sec
                    }
                    else
                    {
                        await Task.Delay(1000, stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error polling S-meters");
                    await Task.Delay(1000, stoppingToken);
                }
            }

            _logger.LogInformation("S-Meter polling service stopped after {Cycles} cycles", cycleCount);
        }
    }
}