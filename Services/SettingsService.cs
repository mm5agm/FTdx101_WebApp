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
        }

        public async Task<ApplicationSettings> GetSettingsAsync()
        {
            if (_cachedSettings != null)
            {
                return _cachedSettings;
            }

            await _semaphore.WaitAsync();
            try
            {
                if (_cachedSettings != null)
                {
                    return _cachedSettings;
                }

                if (File.Exists(_settingsFilePath))
                {
                    var json = await File.ReadAllTextAsync(_settingsFilePath);
                    _cachedSettings = JsonSerializer.Deserialize<ApplicationSettings>(json) ?? new ApplicationSettings();
                    _logger.LogInformation("Settings loaded from {Path}", _settingsFilePath);
                }
                else
                {
                    _cachedSettings = new ApplicationSettings();
                    await SaveSettingsAsync(_cachedSettings);
                    _logger.LogInformation("Created new settings file at {Path}", _settingsFilePath);
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
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(settings, options);
                await File.WriteAllTextAsync(_settingsFilePath, json);
                _cachedSettings = settings;
                _logger.LogInformation("Settings saved to {Path}", _settingsFilePath);
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