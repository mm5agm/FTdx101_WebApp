using Microsoft.AspNetCore.Mvc;
using FTdx101_WebApp.Services;
using System.Diagnostics;

namespace FTdx101_WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WsjtxController : ControllerBase
    {
        private readonly ISettingsService _settingsService;
        private readonly ILogger<WsjtxController> _logger;
        private readonly WsjtxUdpService _udpService;

        public WsjtxController(ISettingsService settingsService, ILogger<WsjtxController> logger, WsjtxUdpService udpService)
        {
            _settingsService = settingsService;
            _logger = logger;
            _udpService = udpService;
        }

        [HttpPost("launch")]
        public async Task<IActionResult> Launch()
        {
            var settings = await _settingsService.GetSettingsAsync();
            var commandLine = settings.WsjtxCommandLine;

            if (string.IsNullOrWhiteSpace(commandLine))
                return BadRequest(new { error = "WSJT-X command line is not configured. Please check Settings." });

            var (exe, args) = ParseCommandLine(commandLine);

            if (!System.IO.File.Exists(exe))
            {
                _logger.LogWarning("WSJT-X executable not found at: {Exe}", exe);
                return BadRequest(new { error = $"WSJT-X executable not found: {exe}" });
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = exe,
                    Arguments = args,
                    UseShellExecute = true
                });
                _logger.LogInformation("Launched WSJT-X: {Exe} {Args}", exe, args);
                return Ok(new { launched = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to launch WSJT-X");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private static (string exe, string args) ParseCommandLine(string commandLine)
        {
            commandLine = commandLine.Trim();
            if (commandLine.StartsWith('"'))
            {
                var closeQuote = commandLine.IndexOf('"', 1);
                if (closeQuote > 0)
                    return (commandLine[1..closeQuote], commandLine[(closeQuote + 1)..].Trim());
            }
            var spaceIndex = commandLine.IndexOf(' ');
            return spaceIndex < 0
                ? (commandLine, string.Empty)
                : (commandLine[..spaceIndex], commandLine[(spaceIndex + 1)..].Trim());
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            var running = Process.GetProcessesByName("wsjtx").Length > 0;
            return Ok(new
            {
                running,
                connected = _udpService.IsConnected,
                transmitting = _udpService.IsTransmitting,
                mode = _udpService.WsjtxMode
            });
        }
    }
}
