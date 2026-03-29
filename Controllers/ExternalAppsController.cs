using Microsoft.AspNetCore.Mvc;
using FTdx101_WebApp.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FTdx101_WebApp.Controllers
{
    [ApiController]
    [Route("api")]
    public class ExternalAppsController : ControllerBase
    {
        private readonly ISettingsService _settingsService;
        private readonly ILogger<ExternalAppsController> _logger;
        private readonly ProcessStatusCacheService _processStatusCache;

        public ExternalAppsController(ISettingsService settingsService, ILogger<ExternalAppsController> logger, ProcessStatusCacheService processStatusCache)
        {
            _settingsService = settingsService;
            _logger = logger;
            _processStatusCache = processStatusCache;
        }

        [HttpPost("jtalert/launch")]
        public async Task<IActionResult> LaunchJtalert()
        {
            return await LaunchExternalApp("JTAlert", "JTAlertV2", async () =>
            {
                var settings = await _settingsService.GetSettingsAsync();
                return settings.JtalertCommandLine;
            });
        }

        [HttpGet("jtalert/status")]
        public IActionResult JtalertStatus()
        {
            // Check for JTAlertV2 process (main JTAlert process) - uses cached status
            var running = _processStatusCache.IsProcessRunning("JTAlertV2");
            return Ok(new { running });
        }

        [HttpPost("log4om/launch")]
        public async Task<IActionResult> LaunchLog4om()
        {
            return await LaunchExternalApp("Log4OM", "Log4OM", async () =>
            {
                var settings = await _settingsService.GetSettingsAsync();
                return settings.Log4omCommandLine;
            });
        }

        [HttpGet("log4om/status")]
        public IActionResult Log4omStatus()
        {
            // Check for L4ONG process (Log4OM Next Generation) - uses cached status
            var running = _processStatusCache.IsProcessRunning("L4ONG");
            return Ok(new { running });
        }

        private async Task<IActionResult> LaunchExternalApp(string appName, string processName, Func<Task<string>> getCommandLine)
        {
            // Check if the app is already running
            var existingProcesses = Process.GetProcessesByName(processName);
            if (existingProcesses.Length > 0)
            {
                _logger.LogInformation("{AppName} is already running (PID: {Pid})", appName, existingProcesses[0].Id);

                // Try to bring the existing window to the front
                try
                {
                    foreach (var proc in existingProcesses)
                    {
                        if (!proc.HasExited && proc.MainWindowHandle != IntPtr.Zero)
                        {
                            var hwnd = proc.MainWindowHandle;
                            BringWindowToFront(hwnd);
                            _logger.LogInformation("Brought existing {AppName} window to front", appName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not bring {AppName} window to front", appName);
                }

                return Ok(new { launched = false, alreadyRunning = true });
            }

            var commandLine = await getCommandLine();

            if (string.IsNullOrWhiteSpace(commandLine))
                return BadRequest(new { error = $"{appName} command line is not configured. Please check Settings." });

            var (exe, args) = ParseCommandLine(commandLine);

            if (!System.IO.File.Exists(exe))
            {
                _logger.LogWarning("{AppName} executable not found at: {Exe}", appName, exe);
                return BadRequest(new { error = $"{appName} executable not found: {exe}" });
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = exe,
                    Arguments = args,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                var process = Process.Start(startInfo);
                _logger.LogInformation("Launched {AppName}: {Exe} {Args}", appName, exe, args);

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
                                    BringWindowToFront(process.MainWindowHandle);
                                    _logger.LogInformation("Made {AppName} window visible and brought to front", appName);
                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Could not ensure {AppName} window visibility", appName);
                        }
                    });
                }

                // Invalidate cache so status check picks up the newly launched process
                _processStatusCache.InvalidateCache(processName);

                return Ok(new { launched = true, alreadyRunning = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to launch {AppName}", appName);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private async void BringWindowToFront(IntPtr hwnd)
        {
            // Multi-step approach to force window to foreground
            WindowNativeMethods.ShowWindow(hwnd, WindowNativeMethods.SW_RESTORE);

            // Make it topmost temporarily
            WindowNativeMethods.SetWindowPos(hwnd, WindowNativeMethods.HWND_TOPMOST, 0, 0, 0, 0,
                WindowNativeMethods.SWP_NOMOVE | WindowNativeMethods.SWP_NOSIZE | WindowNativeMethods.SWP_SHOWWINDOW);

            // Small delay
            await Task.Delay(50);

            // Remove topmost flag
            WindowNativeMethods.SetWindowPos(hwnd, WindowNativeMethods.HWND_NOTOPMOST, 0, 0, 0, 0,
                WindowNativeMethods.SWP_NOMOVE | WindowNativeMethods.SWP_NOSIZE | WindowNativeMethods.SWP_SHOWWINDOW);

            // Bring to top and set foreground
            WindowNativeMethods.BringWindowToTop(hwnd);
            WindowNativeMethods.SetForegroundWindow(hwnd);
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
    }

    // Native methods for window management (shared)
    internal static class WindowNativeMethods
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

        internal static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        internal static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        internal const uint SWP_NOMOVE = 0x0002;
        internal const uint SWP_NOSIZE = 0x0001;
        internal const uint SWP_SHOWWINDOW = 0x0040;
    }
}
