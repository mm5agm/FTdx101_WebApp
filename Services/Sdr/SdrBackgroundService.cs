// FTdx101 WebApp – SDR Background Service
// Manages the SDR lifecycle: open device → configure → stream IQ → FFT → broadcast.
// No UI, no DOM. Publishes spectrum data via SignalR using the existing RadioHub
// and the same { property, value } message envelope that WsUpdatePipeline handles.
//
// Device selection is driven by ApplicationSettings.SdrDeviceKey.
// Keys prefixed with "sdrplay:" are handled by SdrplayDevice (sdrplay_api.dll).

using FTdx101_WebApp.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FTdx101_WebApp.Services.Sdr
{
    public sealed class SdrBackgroundService : BackgroundService
    {
        private readonly ISettingsService              _settings;
        private readonly IHubContext<RadioHub>          _hub;
        private readonly ILogger<SdrBackgroundService>  _logger;

        // Target broadcast rate.
        private const int FrameIntervalMs = 100;   // 10 fps

        // Delay before retrying after a device error.
        private const int RetryDelayMs = 5_000;

        // How long to wait between re-checking settings when no device is configured.
        private const int UnconfiguredPollMs = 10_000;

        public SdrBackgroundService(
            ISettingsService              settings,
            IHubContext<RadioHub>          hub,
            ILogger<SdrBackgroundService>  logger)
        {
            _settings = settings;
            _hub      = hub;
            _logger   = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var config = await _settings.GetSettingsAsync().ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(config.SdrDeviceKey))
                {
                    await BroadcastStatus("unconfigured", stoppingToken).ConfigureAwait(false);
                    await Task.Delay(UnconfiguredPollMs, stoppingToken).ConfigureAwait(false);
                    continue;
                }

                await RunStreamingSession(config, stoppingToken).ConfigureAwait(false);

                if (!stoppingToken.IsCancellationRequested)
                    await Task.Delay(RetryDelayMs, stoppingToken).ConfigureAwait(false);
            }
        }

        // ── Streaming session ────────────────────────────────────────────────────

        private async Task RunStreamingSession(
            Models.ApplicationSettings config,
            CancellationToken stoppingToken)
        {
            ISdrDevice? device = null;
            try
            {
                _logger.LogInformation("SDR: Opening '{Key}'", config.SdrDeviceKey);
                await BroadcastStatus("connecting", stoppingToken).ConfigureAwait(false);

                device = CreateDevice(config.SdrDeviceKey);
                device.Configure(config.SdrIfFrequencyHz, config.SdrSampleRateHz, config.SdrFftSize);
                device.StartStreaming();

                int     fftSize  = config.SdrFftSize;
                float[] iqBuffer = new float[fftSize * 2];

                _logger.LogInformation(
                    "SDR: Streaming '{Label}' — IF {IfHz} Hz, SR {Sr} Hz, FFT {Fft} pts",
                    device.Label, config.SdrIfFrequencyHz, config.SdrSampleRateHz, fftSize);

                await BroadcastStatus("streaming", stoppingToken).ConfigureAwait(false);

                while (!stoppingToken.IsCancellationRequested)
                {
                    bool got = await device
                        .TryReadIqFrameAsync(iqBuffer, FrameIntervalMs * 2, stoppingToken)
                        .ConfigureAwait(false);

                    if (!got) continue;   // timeout — keep waiting

                    float[] bins = FftProcessor.ComputeSpectrum(iqBuffer, fftSize);

                    // Round to 1 decimal place before serialisation to keep JSON compact.
                    for (int i = 0; i < bins.Length; i++)
                        bins[i] = MathF.Round(bins[i], 1);

                    await _hub.Clients.All.SendAsync(
                        "RadioStateUpdate",
                        new
                        {
                            property = "SpectrumUpdate",
                            value = new
                            {
                                bins,
                                centreHz = config.SdrIfFrequencyHz,
                                spanHz   = config.SdrSampleRateHz
                            }
                        },
                        stoppingToken).ConfigureAwait(false);

                    await Task.Yield();
                }
            }
            catch (DllNotFoundException ex)
            {
                _logger.LogWarning("SDR: Required DLL not found — {Message}", ex.Message);
                await BroadcastStatus("nodll", stoppingToken).ConfigureAwait(false);

                // Long sleep — no point retrying if the DLL is missing.
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown — nothing to do.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SDR: Streaming error");
                await BroadcastStatus("disconnected", stoppingToken).ConfigureAwait(false);
            }
            finally
            {
                device?.Stop();
                device?.Dispose();
            }
        }

        // ── Device factory ────────────────────────────────────────────────────────

        private static ISdrDevice CreateDevice(string key)
        {
            if (key.StartsWith(SdrplayDevice.KeyPrefix, StringComparison.OrdinalIgnoreCase))
                return new SdrplayDevice(key);

            // Treat everything else as a SoapySDR kwargs string
            // (e.g. "driver=rtlsdr,label=...", "driver=airspy,serial=...")
            return new SoapySdrDevice(key);
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private async Task BroadcastStatus(string status, CancellationToken ct)
        {
            try
            {
                await _hub.Clients.All.SendAsync(
                    "RadioStateUpdate",
                    new { property = "SdrStatus", value = status },
                    ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "SDR: Failed to broadcast status '{Status}'", status);
            }
        }
    }
}
