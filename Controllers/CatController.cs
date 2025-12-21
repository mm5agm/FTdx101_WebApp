using Microsoft.AspNetCore.Mvc;
using FTdx101MP_WebApp.Services;

namespace FTdx101MP_WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatController : ControllerBase
    {
        private readonly ICatClient _catClient;
        private readonly ISettingsService _settingsService;
        private readonly ILogger<CatController> _logger;

        public CatController(ICatClient catClient, ISettingsService settingsService, ILogger<CatController> logger)
        {
            _catClient = catClient;
            _settingsService = settingsService;
            _logger = logger;
        }

        private async Task EnsureConnectedAsync()
        {
            if (!_catClient.IsConnected)
            {
                var settings = await _settingsService.GetSettingsAsync();
                await _catClient.ConnectAsync(settings.SerialPort, settings.BaudRate);
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                await EnsureConnectedAsync();

                if (!_catClient.IsConnected)
                {
                    return Ok(new
                    {
                        vfoA = new { frequency = 0L, mode = "UNKNOWN", sMeter = 0 },
                        vfoB = new { frequency = 0L, mode = "UNKNOWN", sMeter = 0 },
                        isTransmitting = false,
                        isConnected = false,
                        timestamp = DateTime.Now,
                        message = "Radio not connected"
                    });
                }

                // Read BOTH VFOs with timeout
                var statusTask = Task.Run(async () =>
                {
                    // VFO-A (Main)
                    var freqA = await _catClient.ReadFrequencyAAsync();
                    var modeA = await _catClient.ReadModeMainAsync();
                    var sMeterA = await _catClient.ReadSMeterMainAsync();

                    // VFO-B (Sub)
                    var freqB = await _catClient.ReadFrequencyBAsync();
                    var modeB = await _catClient.ReadModeSubAsync();
                    var sMeterB = await _catClient.ReadSMeterSubAsync();

                    // TX Status
                    var isTx = await _catClient.ReadTransmitStatusAsync();

                    return new
                    {
                        vfoA = new { frequency = freqA, mode = modeA, sMeter = sMeterA },
                        vfoB = new { frequency = freqB, mode = modeB, sMeter = sMeterB },
                        isTransmitting = isTx,
                        isConnected = _catClient.IsConnected,
                        timestamp = DateTime.Now
                    };
                });

                // Wait max 2 seconds
                if (await Task.WhenAny(statusTask, Task.Delay(2000)) == statusTask)
                {
                    return Ok(await statusTask);
                }
                else
                {
                    _logger.LogWarning("GetStatus timed out after 2 seconds");
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
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new
            {
                message = "API Controller is working!",
                timestamp = DateTime.Now,
                isConnected = _catClient.IsConnected
            });
        }

        [HttpGet("frequency/a")]
        public async Task<IActionResult> GetFrequencyA()
        {
            try
            {
                await EnsureConnectedAsync();
                var freq = await _catClient.ReadFrequencyAAsync();
                return Ok(new { frequency = freq, timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading VFO-A frequency");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("frequency/b")]
        public async Task<IActionResult> GetFrequencyB()
        {
            try
            {
                await EnsureConnectedAsync();
                var freq = await _catClient.ReadFrequencyBAsync();
                return Ok(new { frequency = freq, timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading VFO-B frequency");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("frequency/a")]
        public async Task<IActionResult> SetFrequencyA([FromBody] FrequencyRequest request)
        {
            try
            {
                await EnsureConnectedAsync();
                var success = await _catClient.SetFrequencyAAsync(request.FrequencyHz);
                return success ? Ok(new { message = "Frequency set successfully" }) :
                    BadRequest(new { error = "Failed to set frequency" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting VFO-A frequency");
                return StatusCode(500, new { error = "Failed to set frequency" });
            }
        }

        [HttpPost("frequency/b")]
        public async Task<IActionResult> SetFrequencyB([FromBody] FrequencyRequest request)
        {
            try
            {
                await EnsureConnectedAsync();
                var success = await _catClient.SetFrequencyBAsync(request.FrequencyHz);
                return success ? Ok(new { message = "Frequency set successfully" }) :
                    BadRequest(new { error = "Failed to set frequency" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting VFO-B frequency");
                return StatusCode(500, new { error = "Failed to set frequency" });
            }
        }

        [HttpPost("command")]
        public async Task<IActionResult> SendCommand([FromBody] CommandRequest request)
        {
            try
            {
                await EnsureConnectedAsync();
                var response = await _catClient.SendCommandAsync(request.Command);
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending CAT command");
                return StatusCode(500, new { error = "Failed to send command" });
            }
        }

        public class FrequencyRequest { public long FrequencyHz { get; set; } }
        public class CommandRequest { public string Command { get; set; } = string.Empty; }
    }
}