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

            try
            {
                await _settingsService.SaveSettingsAsync(Settings);
                StatusMessage = "Settings saved successfully! Please restart the application for changes to take effect.";
                _logger.LogInformation("Settings updated: SerialPort={SerialPort}, BaudRate={BaudRate}",
                    Settings.SerialPort, Settings.BaudRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving settings");
                StatusMessage = "Error saving settings. Please try again.";
                ModelState.AddModelError(string.Empty, "An error occurred while saving settings.");
                return Page();
            }

            return RedirectToPage();
        }
    }
}