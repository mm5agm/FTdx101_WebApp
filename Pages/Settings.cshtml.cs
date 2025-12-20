using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FTdx101MP_WebApp.Models;
using FTdx101MP_WebApp.Services;

namespace FTdx101MP_WebApp.Pages
{
    public class SettingsModel : PageModel
    {
        private readonly ISettingsService _settingsService;
        private readonly ILogger<SettingsModel> _logger;

        [BindProperty]
        public ApplicationSettings Settings { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public SettingsModel(ISettingsService settingsService, ILogger<SettingsModel> logger)
        {
            _settingsService = settingsService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Settings = await _settingsService.GetSettingsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Validate port number is in acceptable range
            if (Settings.WebPort < 1024 || Settings.WebPort > 65535)
            {
                ModelState.AddModelError("Settings.WebPort",
                    "Port must be between 1024 and 65535. Ports below 1024 require administrator privileges.");
                return Page();
            }

            // Validate port isn't in blocked range (6000-6063)
            if (Settings.WebPort >= 6000 && Settings.WebPort <= 6063)
            {
                ModelState.AddModelError("Settings.WebPort",
                    $"Port {Settings.WebPort} is blocked by most browsers for security reasons. " +
                    "Please use a different port (recommended: 8080, 5000, or 8000).");
                StatusMessage = $"⚠️ Port validation failed: Port {Settings.WebPort} is unsafe.";
                return Page();
            }

            // Validate Serial Port format
            if (!Settings.SerialPort.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Settings.SerialPort",
                    "Serial port must start with 'COM' (e.g., COM3, COM4).");
                return Page();
            }

            try
            {
                await _settingsService.SaveSettingsAsync(Settings);

                StatusMessage = $"✅ Settings saved successfully! Web server will be available at http://{Settings.WebAddress}:{Settings.WebPort}. " +
                    "Please restart the application for web server changes to take effect.";

                _logger.LogInformation(
                    "Settings updated: SerialPort={SerialPort}, BaudRate={BaudRate}, WebAddress={WebAddress}, WebPort={WebPort}",
                    Settings.SerialPort, Settings.BaudRate, Settings.WebAddress, Settings.WebPort);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving settings");
                StatusMessage = "❌ Error saving settings. Please try again.";
                ModelState.AddModelError(string.Empty, "An error occurred while saving settings.");
                return Page();
            }

            return RedirectToPage();
        }
    }
}