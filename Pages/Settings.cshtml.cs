using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FTdx101_WebApp.Models;
using FTdx101_WebApp.Services;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace FTdx101_WebApp.Pages
{
    public class SettingsModel : PageModel
    {
        private readonly ISettingsService _settingsService;
        private readonly ILogger<SettingsModel> _logger;
        private readonly RadioInitializationService _radioInitializationService;

        [BindProperty]
        public ApplicationSettings Settings { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; } = string.Empty;

        public List<string> NetworkAddresses { get; set; } = new();

        public SettingsModel(
            ISettingsService settingsService,
            ILogger<SettingsModel> logger,
            RadioInitializationService radioInitializationService)
        {
            _settingsService = settingsService;
            _logger = logger;
            _radioInitializationService = radioInitializationService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Settings = await _settingsService.GetSettingsAsync();
            NetworkAddresses = GetLocalIPAddresses();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                NetworkAddresses = GetLocalIPAddresses();
                return Page();
            }

            // Validate Serial Port format
            if (!Settings.SerialPort.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Settings.SerialPort",
                    "Serial port must start with 'COM' (e.g., COM3, COM4).");
                NetworkAddresses = GetLocalIPAddresses();
                return Page();
            }

            try
            {
                await _settingsService.SaveSettingsAsync(Settings);

                // Reset initialization status so app will try again
                FTdx101_WebApp.Services.AppStatus.InitializationStatus = "initializing";

                // Automatic retry: trigger radio initialization
                await _radioInitializationService.InitializeRadioAsync();

                var accessInfo = Settings.WebAddress == "0.0.0.0"
                    ? "all network interfaces"
                    : Settings.WebAddress == "localhost"
                        ? "localhost only"
                        : $"{Settings.WebAddress}";

                StatusMessage = $"✓ Settings saved successfully! Web server will be available on {accessInfo} at port 8080. " +
                    "Please restart the application for web server changes to take effect.";

                _logger.LogInformation(
                    "Settings updated: RadioModel={RadioModel}, SerialPort={SerialPort}, BaudRate={BaudRate}, WebAddress={WebAddress}",
                    Settings.RadioModel, Settings.SerialPort, Settings.BaudRate, Settings.WebAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving settings");
                StatusMessage = "❌ Error saving settings. Please try again.";
                ModelState.AddModelError(string.Empty, "An error occurred while saving settings.");
                NetworkAddresses = GetLocalIPAddresses();
                return Page();
            }

            return RedirectToPage();
        }

        private List<string> GetLocalIPAddresses()
        {
            var addresses = new List<string>();

            try
            {
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback);

                foreach (var networkInterface in networkInterfaces)
                {
                    var ipProperties = networkInterface.GetIPProperties();
                    foreach (var ip in ipProperties.UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork) // IPv4 only
                        {
                            addresses.Add(ip.Address.ToString());
                        }
                    }
                }

                _logger.LogDebug("Detected network addresses: {Addresses}", string.Join(", ", addresses));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to detect network addresses");
            }

            return addresses;
        }
    }
}