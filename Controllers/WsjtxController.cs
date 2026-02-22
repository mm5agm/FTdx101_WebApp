using Microsoft.AspNetCore.Mvc;
using FTdx101_WebApp.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FTdx101_WebApp.Controllers
{
    // Native methods for window management
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        internal static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        internal const int SW_RESTORE = 9;
        internal const int SW_SHOWNORMAL = 1;
        internal const int SW_SHOW = 5;
        internal const int SW_MAXIMIZE = 3;

        internal static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        internal static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        internal const uint SWP_NOMOVE = 0x0002;
        internal const uint SWP_NOSIZE = 0x0001;
        internal const uint SWP_SHOWWINDOW = 0x0040;
    }

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
            // Check if WSJT-X is already running
            var existingProcesses = Process.GetProcessesByName("wsjtx");
            if (existingProcesses.Length > 0)
            {
                _logger.LogInformation("WSJT-X is already running (PID: {Pid})", existingProcesses[0].Id);

                // Try to bring the existing window to the front
                try
                {
                    foreach (var proc in existingProcesses)
                    {
                        if (!proc.HasExited && proc.MainWindowHandle != IntPtr.Zero)
                        {
                            var hwnd = proc.MainWindowHandle;

                            // Multi-step approach to force window to foreground
                            NativeMethods.ShowWindow(hwnd, NativeMethods.SW_RESTORE);

                            // Make it topmost temporarily
                            NativeMethods.SetWindowPos(hwnd, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0,
                                NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_SHOWWINDOW);

                            // Small delay
                            await Task.Delay(50);

                            // Remove topmost flag
                            NativeMethods.SetWindowPos(hwnd, NativeMethods.HWND_NOTOPMOST, 0, 0, 0, 0,
                                NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_SHOWWINDOW);

                            // Bring to top and set foreground
                            NativeMethods.BringWindowToTop(hwnd);
                            NativeMethods.SetForegroundWindow(hwnd);

                            _logger.LogInformation("Brought existing WSJT-X window to front");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not bring WSJT-X window to front");
                }

                return Ok(new { launched = false, alreadyRunning = true });
            }

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
                var startInfo = new ProcessStartInfo
                {
                    FileName = exe,
                    Arguments = args,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Normal  // Start visible, not minimized
                };

                var process = Process.Start(startInfo);
                _logger.LogInformation("Launched WSJT-X: {Exe} {Args}", exe, args);

                // Wait a moment for the window to be created, then ensure it's visible
                if (process != null)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            // Wait up to 5 seconds for the main window to be created
                            for (int i = 0; i < 50; i++)
                            {
                                await Task.Delay(100);
                                process.Refresh();

                                if (process.MainWindowHandle != IntPtr.Zero)
                                {
                                    var hwnd = process.MainWindowHandle;

                                    // Multi-step approach to force window to foreground
                                    // 1. Show the window (not minimized)
                                    NativeMethods.ShowWindow(hwnd, NativeMethods.SW_SHOW);

                                    // 2. Make it topmost temporarily to ensure visibility
                                    NativeMethods.SetWindowPos(hwnd, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0, 
                                        NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_SHOWWINDOW);

                                    // 3. Small delay to let Windows process the topmost change
                                    await Task.Delay(50);

                                    // 4. Remove topmost flag so it behaves normally
                                    NativeMethods.SetWindowPos(hwnd, NativeMethods.HWND_NOTOPMOST, 0, 0, 0, 0,
                                        NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_SHOWWINDOW);

                                    // 5. Bring to top of z-order
                                    NativeMethods.BringWindowToTop(hwnd);

                                    // 6. Finally, set as foreground window
                                    NativeMethods.SetForegroundWindow(hwnd);

                                    _logger.LogInformation("Made WSJT-X window visible and brought to front (Handle: {Handle})", hwnd);
                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Could not ensure WSJT-X window visibility");
                        }
                    });
                }

                return Ok(new { launched = true, alreadyRunning = false });
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
