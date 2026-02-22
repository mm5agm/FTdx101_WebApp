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

            // Wait for radio initialization to complete
            _logger.LogInformation("Waiting for radio initialization to complete...");
            while (AppStatus.InitializationStatus != "complete" && !stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(500, stoppingToken);
            }

            if (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Meter polling service cancelled before radio initialization completed");
                return;
            }

            _logger.LogInformation("Radio initialized. Starting meter polling...");
            await Task.Delay(1000, stoppingToken); // Additional delay to ensure everything is ready

            int lastValidA = 0;
            int lastValidB = 0;
            int lastValidPower = 0;
            int lastValidSWR = 0;
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
                        var sm0Response = await _multiplexer.SendCommandAsync("SM0;", "MeterPoll", stoppingToken);

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

                        var sm1Response = await _multiplexer.SendCommandAsync("SM1;", "MeterPoll", stoppingToken);

                        if (!string.IsNullOrEmpty(sm1Response))
                        {
                            int sMeterB = CatCommands.ParseSMeter(sm1Response);
                            if (sMeterB > 0 || (sMeterB == 0 && sm1Response.Contains("SM1000")))
                            {
                                _stateService.SMeterB = sMeterB;
                                lastValidB = sMeterB;
                            }
                        }

                        // Poll power output meter (RM5 - not RM1!)
                        var powerResponse = await _multiplexer.SendCommandAsync("RM5;", "MeterPoll", stoppingToken);
                        if (!string.IsNullOrEmpty(powerResponse))
                        {
                            int powerMeter = CatCommands.ParseMeterReading(powerResponse);

                            // Log every 10 cycles to debug power meter issues
                            if (cycleCount % 10 == 0)
                            {
                                _logger.LogWarning("[MeterPoll] RM5 Response: '{Response}' -> Parsed value: {Value}", 
                                    powerResponse, powerMeter);
                            }

                            if (powerMeter >= 0)
                            {
                                _stateService.PowerMeter = powerMeter;
                                lastValidPower = powerMeter;
                            }
                        }

                        // Poll SWR meter (RM6 - not RM2!)
                        var swrResponse = await _multiplexer.SendCommandAsync("RM6;", "MeterPoll", stoppingToken);
                        if (!string.IsNullOrEmpty(swrResponse))
                        {
                            int swrMeter = CatCommands.ParseMeterReading(swrResponse);

                            // Log every 10 cycles to debug SWR meter issues  
                            if (cycleCount % 10 == 0)
                            {
                                _logger.LogWarning("[MeterPoll] RM6 Response: '{Response}' -> Parsed value: {Value}", 
                                    swrResponse, swrMeter);
                            }

                            if (swrMeter >= 0)
                            {
                                _stateService.SWRMeter = swrMeter;
                                lastValidSWR = swrMeter;
                            }
                        }

                        var cycleTime = stopwatch.ElapsedMilliseconds - cycleStart;

                        // Log every 50 cycles to diagnose speed
                        if (++cycleCount % 50 == 0)
                        {
                            _logger.LogInformation("Meters: Cycle {Count} took {Time}ms (S:{SA}/{SB} P:{Power} SWR:{SWR})", 
                                cycleCount, cycleTime, lastValidA, lastValidB, lastValidPower, lastValidSWR);
                        }

                        // Minimal delay - let CPU breathe but stay fast
                        await Task.Delay(200, stoppingToken);  // Moderate: ~5 updates/sec
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
                    _logger.LogError(ex, "Error polling meters");
                    await Task.Delay(1000, stoppingToken);
                }
            }

            _logger.LogInformation("S-Meter polling service stopped after {Cycles} cycles", cycleCount);
        }
    }
}