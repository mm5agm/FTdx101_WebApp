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
            int freqPollCounter = 0; // Poll frequency every 2-3 cycles to reduce CAT traffic
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

                        // Poll power output meter (RM5 - not RM1!) - only when transmitting
                        // Check TX status first (TX; command returns TX0; when receiving, TX1; when transmitting)
                        var txResponse = await _multiplexer.SendCommandAsync("TX;", "MeterPoll", stoppingToken);
                        bool isTransmitting = !string.IsNullOrEmpty(txResponse) && txResponse.Contains("TX1");

                        // Update TX state in RadioStateService so it can broadcast to SignalR
                        _stateService.IsTransmitting = isTransmitting;

                        if (isTransmitting)
                        {
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
                        }
                        else
                        {
                            // Not transmitting - send zero to clear meters
                            _stateService.PowerMeter = 0;
                            lastValidPower = 0;
                        }

                        // Poll SWR meter (RM6 - not RM2!) - only when transmitting
                        if (isTransmitting)
                        {
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
                        }
                        else
                        {
                            // Not transmitting - send zero to clear SWR meter
                            _stateService.SWRMeter = 0;
                            lastValidSWR = 0;
                        }

                        // Poll frequencies every 2-3 cycles to detect WSJT-X changes (AI doesn't report CAT changes)
                        if (++freqPollCounter >= 3)
                        {
                            freqPollCounter = 0;

                            var faResponse = await _multiplexer.SendCommandAsync("FA;", "MeterPoll", stoppingToken);
                            if (!string.IsNullOrEmpty(faResponse) && faResponse.StartsWith("FA"))
                            {
                                var freqStr = faResponse.Substring(2).TrimEnd(';');
                                if (int.TryParse(freqStr, out int freqHz) && freqHz != _stateService.FrequencyA)
                                {
                                    _logger.LogInformation("[FreqPoll] VFO A changed (likely WSJT-X): {OldFreq} Hz -> {NewFreq} Hz", 
                                        _stateService.FrequencyA, freqHz);
                                    _stateService.FrequencyA = freqHz;

                                    // Update band too
                                    var newBand = _stateService.GetBandFromFrequency(freqHz);
                                    if (newBand != _stateService.BandA)
                                    {
                                        _stateService.BandA = newBand;
                                        _logger.LogInformation("[FreqPoll] VFO A band updated: {Band}", newBand);
                                    }
                                }
                            }

                            var fbResponse = await _multiplexer.SendCommandAsync("FB;", "MeterPoll", stoppingToken);
                            if (!string.IsNullOrEmpty(fbResponse) && fbResponse.StartsWith("FB"))
                            {
                                var freqStr = fbResponse.Substring(2).TrimEnd(';');
                                if (int.TryParse(freqStr, out int freqHz) && freqHz != _stateService.FrequencyB)
                                {
                                    _logger.LogInformation("[FreqPoll] VFO B changed (likely WSJT-X): {OldFreq} Hz -> {NewFreq} Hz", 
                                        _stateService.FrequencyB, freqHz);
                                    _stateService.FrequencyB = freqHz;

                                    // Update band too
                                    var newBand = _stateService.GetBandFromFrequency(freqHz);
                                    if (newBand != _stateService.BandB)
                                    {
                                        _stateService.BandB = newBand;
                                        _logger.LogInformation("[FreqPoll] VFO B band updated: {Band}", newBand);
                                    }
                                }
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