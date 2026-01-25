using Microsoft.AspNetCore.Mvc;
using FTdx101_WebApp.Services;
using System.Text.Json;
using System.Threading;

namespace FTdx101_WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatController : ControllerBase
    {
        private readonly ICatClient _catClient;
        private readonly ISettingsService _settingsService;
        private readonly ILogger<CatController> _logger;
        private readonly RadioStateService _radioStateService;
        private readonly RadioStatePersistenceService _statePersistence;
        private static readonly SemaphoreSlim _requestSemaphore = new(1, 1);
        private static bool _restored = false;
        private RadioState _radioState;

        public CatController(
            ICatClient catClient,
            ISettingsService settingsService,
            ILogger<CatController> logger,
            RadioStateService radioStateService,
            RadioStatePersistenceService statePersistence)
        {
            _catClient = catClient;
            _settingsService = settingsService;
            _logger = logger;
            _radioStateService = radioStateService;
            _statePersistence = statePersistence;
            _radioState = _statePersistence.Load();
        }

        private async Task EnsureConnectedAsync()
        {
            if (!_catClient.IsConnected)
            {
                var settings = await _settingsService.GetSettingsAsync();
                await _catClient.ConnectAsync(settings.SerialPort, settings.BaudRate);
            }
            // Restore state only once per app run
            if (!_restored)
            {
                await RestoreRadioStateAsync();
                _restored = true;
            }
        }

        private async Task RestoreRadioStateAsync()
        {
            var state = _statePersistence.Load();
            if (state.FrequencyA > 0)
                await _catClient.SendCommandAsync($"FA{state.FrequencyA:D9};", "WebUI", CancellationToken.None);
            if (!string.IsNullOrEmpty(state.ModeA))
                await _catClient.SetModeMainAsync(state.ModeA);
            if (!string.IsNullOrEmpty(state.AntennaA))
                await _catClient.SendCommandAsync($"AN0{state.AntennaA};", "WebUI", CancellationToken.None);
            if (state.FrequencyB > 0)
                await _catClient.SendCommandAsync($"FB{state.FrequencyB:D9};", "WebUI", CancellationToken.None);
            if (!string.IsNullOrEmpty(state.ModeB))
                await _catClient.SetModeSubAsync(state.ModeB);
            if (!string.IsNullOrEmpty(state.AntennaB))
                await _catClient.SendCommandAsync($"AN1{state.AntennaB};", "WebUI", CancellationToken.None);
        }

        private async Task<string> GetMainVfoAsync()
        {
            var response = await _catClient.SendCommandAsync("IF;", "WebUI", CancellationToken.None);
            if (!string.IsNullOrEmpty(response) && response.Length > 5)
                return response[5] == '1' ? "B" : "A";
            return "A";
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                await EnsureConnectedAsync();

                // Get current state from reactive state service
                var state = _radioStateService.GetState();

                // Get settings for max power
                var settings = await _settingsService.GetSettingsAsync();
                int maxPower = settings.RadioModel == "FTdx101MP" ? 200 : 100;

                return Ok(new
                {
                    vfoA = new
                    {
                        frequency = _radioStateService.FrequencyA,  // ← FROM REACTIVE STATE
                        mode = _radioStateService.ModeA,
                        sMeter = _radioStateService.SMeterA,
                        band = state.BandA,
                        antenna = _radioStateService.AntennaA,
                        power = _radioStateService.Power
                    },
                    vfoB = new
                    {
                        frequency = _radioStateService.FrequencyB,  // ← FROM REACTIVE STATE
                        mode = _radioStateService.ModeB,
                        sMeter = _radioStateService.SMeterB,
                        band = state.BandB,
                        antenna = _radioStateService.AntennaB,
                        power = _radioStateService.Power
                    },
                    controls = state.Controls,
                    isTransmitting = _radioStateService.IsTransmitting,
                    isConnected = _catClient.IsConnected,
                    mainVfo = "A",
                    maxPower = maxPower,
                    radioModel = settings.RadioModel,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting radio status");
                return StatusCode(500, new { error = "Failed to get radio status", details = ex.Message });
            }
        }

        [HttpPost("frequency/a")]
        public async Task<IActionResult> SetFrequencyA([FromBody] FrequencyRequest request)
        {
            if (!await _requestSemaphore.WaitAsync(2000))
                return StatusCode(503, new { error = "Radio busy" });

            try
            {
                await EnsureConnectedAsync();
                var freq = request.FrequencyHz;
                if (freq < 30000 || freq > 75000000)
                    return BadRequest(new { error = "Frequency out of range" });

                var command = $"FA{freq:D9};";
                await _catClient.SendCommandAsync(command, "WebUI", CancellationToken.None);

                _radioState.FrequencyA = freq;
                _statePersistence.Save(_radioState);

                _logger.LogInformation("Set Receiver A frequency to {Freq}", freq);
                return Ok(new { message = $"Frequency {freq} Hz set for Receiver A" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Receiver A frequency");
                return StatusCode(500, new { error = "Failed to set frequency" });
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }

        [HttpPost("frequency/b")]
        public async Task<IActionResult> SetFrequencyB([FromBody] FrequencyRequest request)
        {
            if (!await _requestSemaphore.WaitAsync(2000))
                return StatusCode(503, new { error = "Radio busy" });

            try
            {
                await EnsureConnectedAsync();
                var freq = request.FrequencyHz;
                if (freq < 30000 || freq > 75000000)
                    return BadRequest(new { error = "Frequency out of range" });

                var command = $"FB{freq:D9};";
                await _catClient.SendCommandAsync(command, "WebUI", CancellationToken.None);

                _radioState.FrequencyB = freq;
                _statePersistence.Save(_radioState);

                _logger.LogInformation("Set Receiver B frequency to {Freq}", freq);
                return Ok(new { message = $"Frequency {freq} Hz set for Receiver B" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Receiver B frequency");
                return StatusCode(500, new { error = "Failed to set frequency" });
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }

        [HttpPost("band/a")]
        public async Task<IActionResult> SetBandA([FromBody] BandRequest request)
        {
            if (!await _requestSemaphore.WaitAsync(2000))
                return StatusCode(503, new { error = "Radio busy" });

            try
            {
                await EnsureConnectedAsync();

                var bandFreqs = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
                {
                    { "160m", 1840000 }, { "80m", 3700000 }, { "60m", 5357000 },
                    { "40m", 7100000 }, { "30m", 10136000 }, { "20m", 14074000 },
                    { "17m", 18110000 }, { "15m", 21074000 }, { "12m", 24915000 },
                    { "10m", 28074000 }, { "6m", 50313000 }, { "4m", 70100000 }
                };

                if (!bandFreqs.TryGetValue(request.Band, out var freq))
                    return BadRequest(new { error = "Invalid band" });

                var command = $"FA{freq:D9};";
                await _catClient.SendCommandAsync(command, "WebUI", CancellationToken.None);

                _radioState.BandA = request.Band;
                _statePersistence.Save(_radioState);

                _radioStateService.SetBand("A", request.Band);  // This line is important!

                _logger.LogInformation("Set Receiver A band to {Band} (freq {Freq})", request.Band, freq);
                return Ok(new { message = $"Band {request.Band} selected" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Receiver A band");
                return StatusCode(500, new { error = "Failed to set band" });
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }

        [HttpPost("band/b")]
        public async Task<IActionResult> SetBandB([FromBody] BandRequest request)
        {
            if (!await _requestSemaphore.WaitAsync(2000))
                return StatusCode(503, new { error = "Radio busy" });

            try
            {
                await EnsureConnectedAsync();

                var bandFreqs = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
                {
                    { "160m", 1840000 }, { "80m", 3700000 }, { "60m", 5357000 },
                    { "40m", 7100000 }, { "30m", 10136000 }, { "20m", 14074000 },
                    { "17m", 18110000 }, { "15m", 21074000 }, { "12m", 24915000 },
                    { "10m", 28074000 }, { "6m", 50313000 }, { "4m", 70100000 }
                };

                if (!bandFreqs.TryGetValue(request.Band, out var freq))
                    return BadRequest(new { error = "Invalid band" });

                var command = $"FB{freq:D9};";
                await _catClient.SendCommandAsync(command, "WebUI", CancellationToken.None);

                _radioState.BandB = request.Band;
                _statePersistence.Save(_radioState);

                _radioStateService.SetBand("B", request.Band);

                _logger.LogInformation("Set Receiver B band to {Band} (freq {Freq})", request.Band, freq);
                return Ok(new { message = $"Band {request.Band} selected" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Receiver B band");
                return StatusCode(500, new { error = "Failed to set band" });
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }

        [HttpPost("antenna/a")]
        public async Task<IActionResult> SetAntennaA([FromBody] AntennaRequest request)
        {
            if (!await _requestSemaphore.WaitAsync(2000))
            {
                return StatusCode(503, new { error = "Radio busy" });
            }

            try
            {
                await EnsureConnectedAsync();
                var command = $"AN0{request.Antenna};";
                await _catClient.SendCommandAsync(command, "WebUI", CancellationToken.None);

                _radioState.AntennaA = request.Antenna;
                _statePersistence.Save(_radioState);

                _radioStateService.SetAntenna("A", request.Antenna);
                _logger.LogInformation("Set Main antenna to {Antenna}", request.Antenna);
                return Ok(new { message = $"Antenna {request.Antenna} selected" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Main antenna");
                return StatusCode(500, new { error = "Failed to set antenna" });
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }

        [HttpPost("antenna/b")]
        public async Task<IActionResult> SetAntennaB([FromBody] AntennaRequest request)
        {
            if (!await _requestSemaphore.WaitAsync(2000))
            {
                return StatusCode(503, new { error = "Radio busy" });
            }

            try
            {
                await EnsureConnectedAsync();
                var command = $"AN1{request.Antenna};";
                await _catClient.SendCommandAsync(command, "WebUI", CancellationToken.None);

                _radioState.AntennaB = request.Antenna;
                _statePersistence.Save(_radioState);

                _radioStateService.SetAntenna("B", request.Antenna);
                _logger.LogInformation("Set Sub antenna to {Antenna}", request.Antenna);
                return Ok(new { message = $"Antenna {request.Antenna} selected" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Sub antenna");
                return StatusCode(500, new { error = "Failed to set antenna" });
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }

        [HttpPost("mode/a")]
        public async Task<IActionResult> SetModeA([FromBody] ModeRequest request)
        {
            if (!await _requestSemaphore.WaitAsync(2000))
                return StatusCode(503, new { error = "Radio busy" });

            try
            {
                await EnsureConnectedAsync();
                await _catClient.SetModeMainAsync(request.Mode);

                _radioState.ModeA = request.Mode;
                _statePersistence.Save(_radioState);

                _logger.LogInformation("Set Receiver A mode to {Mode}", request.Mode);
                return Ok(new { message = $"Mode {request.Mode} selected" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Receiver A mode");
                return StatusCode(500, new { error = "Failed to set mode" });
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }

        [HttpPost("mode/b")]
        public async Task<IActionResult> SetModeB([FromBody] ModeRequest request)
        {
            if (!await _requestSemaphore.WaitAsync(2000))
                return StatusCode(503, new { error = "Radio busy" });

            try
            {
                await EnsureConnectedAsync();
                await _catClient.SetModeSubAsync(request.Mode);

                _radioState.ModeB = request.Mode;
                _statePersistence.Save(_radioState);

                _logger.LogInformation("Set Receiver B mode to {Mode}", request.Mode);
                return Ok(new { message = $"Mode {request.Mode} selected" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Receiver B mode");
                return StatusCode(500, new { error = "Failed to set mode" });
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }

        [HttpPost("power/{receiver}")]
        public async Task<IActionResult> SetPower(string receiver, [FromBody] PowerRequest request)
        {
            if (!await _requestSemaphore.WaitAsync(2000))
                return StatusCode(503, new { error = "Radio busy" });

            try
            {
                await EnsureConnectedAsync();

                // Get radio model from settings to determine max power
                var settings = await _settingsService.GetSettingsAsync();
                int maxPower = settings.RadioModel == "FTdx101MP" ? 200 : 100;

                // Validate power range
                if (request.Watts < 0 || request.Watts > maxPower)
                    return BadRequest(new { error = $"Power out of range (0-{maxPower}W for {settings.RadioModel})" });

                // Format: PC000-PC200 (pad to 3 digits)
                var command = $"PC{request.Watts:D3};";
                await _catClient.SendCommandAsync(command, "WebUI", CancellationToken.None);

                _logger.LogInformation("Set power to {Power}W on {RadioModel}", request.Watts, settings.RadioModel);
                return Ok(new { message = $"Power set to {request.Watts}W", maxPower = maxPower });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting power");
                return StatusCode(500, new { error = "Failed to set power" });
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }

        // Add this helper method (put it near other helper methods like ParseSMeter)
        private int ParsePower(string response)
        {
            // Response format: PC123; (3 digits for watts)
            if (response.Length >= 5 && response.StartsWith("PC"))
            {
                if (int.TryParse(response.Substring(2, 3), out int watts))
                {
                    return watts;
                }
            }
            return 100; // Default to 100W if can't parse
        }

        public class BandRequest { public string Band { get; set; } = string.Empty; }
        public class AntennaRequest { public string Antenna { get; set; } = string.Empty; }
        public class ModeRequest { public string Mode { get; set; } = string.Empty; }
        public class FrequencyRequest { public long FrequencyHz { get; set; } }
        public class PowerRequest
        {
            public int Watts { get; set; }
        }
    }
}