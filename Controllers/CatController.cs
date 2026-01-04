using Microsoft.AspNetCore.Mvc;
using FTdx101_WebApp.Services;

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
        private static readonly SemaphoreSlim _requestSemaphore = new(1, 1);

        public CatController(
            ICatClient catClient,
            ISettingsService settingsService,
            ILogger<CatController> logger,
            RadioStateService radioStateService)
        {
            _catClient = catClient;
            _settingsService = settingsService;
            _logger = logger;
            _radioStateService = radioStateService;
        }

        private async Task EnsureConnectedAsync()
        {
            if (!_catClient.IsConnected)
            {
                var settings = await _settingsService.GetSettingsAsync();
                await _catClient.ConnectAsync(settings.SerialPort, settings.BaudRate);
            }
        }

        private async Task<string> GetMainVfoAsync()
        {
            var response = await _catClient.SendCommandAsync("IF;");
            if (!string.IsNullOrEmpty(response) && response.Length > 5)
                return response[5] == '1' ? "B" : "A";
            return "A";
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var acquired = await _requestSemaphore.WaitAsync(500);
            if (!acquired)
            {
                _logger.LogDebug("Status request skipped - radio busy");
                return StatusCode(503, new
                {
                    error = "Radio busy",
                    message = "Radio is processing another command. Please retry.",
                    isConnected = _catClient.IsConnected
                });
            }

            try
            {
                await EnsureConnectedAsync();

                if (!_catClient.IsConnected)
                {
                    var state = _radioStateService.GetState();
                    return Ok(new
                    {
                        vfoA = new { frequency = 0L, mode = "UNKNOWN", sMeter = 0, band = state.BandA, antenna = state.AntennaA },
                        vfoB = new { frequency = 0L, mode = "UNKNOWN", sMeter = 0, band = state.BandB, antenna = state.AntennaB },
                        controls = state.Controls,
                        isTransmitting = false,
                        isConnected = false,
                        mainVfo = "A",
                        timestamp = DateTime.Now,
                        message = "Radio not connected"
                    });
                }

                var statusTask = Task.Run(async () =>
                {
                    var freqA = await _catClient.ReadFrequencyAAsync();
                    var modeA = await _catClient.ReadModeMainAsync();
                    var sMeterA = await _catClient.ReadSMeterMainAsync();

                    var freqB = await _catClient.ReadFrequencyBAsync();
                    var modeB = await _catClient.ReadModeSubAsync();
                    var sMeterB = await _catClient.ReadSMeterSubAsync();

                    var isTx = await _catClient.ReadTransmitStatusAsync();
                    var mainVfo = await GetMainVfoAsync();

                    var state = _radioStateService.GetState();

                    return new
                    {
                        vfoA = new { frequency = freqA, mode = modeA, sMeter = sMeterA, band = state.BandA, antenna = state.AntennaA },
                        vfoB = new { frequency = freqB, mode = modeB, sMeter = sMeterB, band = state.BandB, antenna = state.AntennaB },
                        controls = state.Controls,
                        isTransmitting = isTx,
                        isConnected = _catClient.IsConnected,
                        mainVfo = mainVfo,
                        timestamp = DateTime.Now
                    };
                });

                if (await Task.WhenAny(statusTask, Task.Delay(3000)) == statusTask)
                {
                    return Ok(await statusTask);
                }
                else
                {
                    _logger.LogWarning("GetStatus serial operations timed out after 3 seconds");
                    return StatusCode(408, new
                    {
                        error = "Request timed out",
                        message = "Radio is taking too long to respond.",
                        isConnected = _catClient.IsConnected
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting radio status");
                return StatusCode(500, new { error = "Failed to get radio status", details = ex.Message });
            }
            finally
            {
                _requestSemaphore.Release();
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
                await _catClient.SendCommandAsync(command);

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
                await _catClient.SendCommandAsync(command);

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
                await _catClient.SendCommandAsync(command);

                _radioStateService.SetBand("A", request.Band);

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
                await _catClient.SendCommandAsync(command);

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
                await _catClient.SendCommandAsync(command);
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
                await _catClient.SendCommandAsync(command);
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

        public class BandRequest { public string Band { get; set; } = string.Empty; }
        public class AntennaRequest { public string Antenna { get; set; } = string.Empty; }
        public class ModeRequest { public string Mode { get; set; } = string.Empty; }
        public class FrequencyRequest { public long FrequencyHz { get; set; } }
    }
}