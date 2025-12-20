using System.Text.Json;
using FTdx101MP_WebApp.Models;

namespace FTdx101MP_WebApp.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsFilePath;
        private readonly ILogger<SettingsService> _logger;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private ApplicationSettings? _cachedSettings;

        public SettingsService(IWebHostEnvironment environment, ILogger<SettingsService> logger)
        {
            _settingsFilePath = Path.Combine(environment.ContentRootPath, "appsettings.user.json");
            _logger = logger;
            _logger.LogInformation("SettingsService initialized. File path: {Path}", _settingsFilePath);
        }

        public async Task<ApplicationSettings> GetSettingsAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                _logger.LogInformation("GetSettingsAsync called. File exists: {Exists}", File.Exists(_settingsFilePath));

                if (File.Exists(_settingsFilePath))
                {
                    var json = await File.ReadAllTextAsync(_settingsFilePath);
                    _logger.LogInformation("Raw JSON read: {Json}", json);

                    _cachedSettings = JsonSerializer.Deserialize<ApplicationSettings>(json) ?? new ApplicationSettings();

                    _logger.LogInformation("Settings deserialized: SerialPort={SerialPort}, BaudRate={BaudRate}, WebAddress={WebAddress}, WebPort={WebPort}",
                        _cachedSettings.SerialPort, _cachedSettings.BaudRate, _cachedSettings.WebAddress, _cachedSettings.WebPort);
                }
                else
                {
                    _cachedSettings = new ApplicationSettings();
                    _logger.LogWarning("Settings file does not exist at {Path}. Using defaults: SerialPort={SerialPort}, WebAddress={WebAddress}",
                        _settingsFilePath, _cachedSettings.SerialPort, _cachedSettings.WebAddress);
                }

                return _cachedSettings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading settings from {Path}", _settingsFilePath);
                return new ApplicationSettings();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task SaveSettingsAsync(ApplicationSettings settings)
        {
            await _semaphore.WaitAsync();
            try
            {
                _logger.LogInformation("SaveSettingsAsync called with: SerialPort={SerialPort}, BaudRate={BaudRate}, WebAddress={WebAddress}, WebPort={WebPort}",
                    settings.SerialPort, settings.BaudRate, settings.WebAddress, settings.WebPort);

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(settings, options);
                _logger.LogInformation("Serialized to JSON: {Json}", json);

                await File.WriteAllTextAsync(_settingsFilePath, json);
                _cachedSettings = settings;

                _logger.LogInformation("Settings saved successfully to {Path}", _settingsFilePath);

                // Verify
                if (File.Exists(_settingsFilePath))
                {
                    var verify = await File.ReadAllTextAsync(_settingsFilePath);
                    _logger.LogInformation("Verification: File content after save: {Content}", verify);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving settings to {Path}", _settingsFilePath);
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}