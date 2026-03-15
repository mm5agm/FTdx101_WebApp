using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using FTdx101_WebApp.Models;

namespace FTdx101_WebApp.Services
{
    public class CalibrationService
    {
        private readonly string _calibrationFilePath;
        private readonly ILogger<CalibrationService> _logger;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private CalibrationSettings? _cachedSettings;

        public CalibrationService(IWebHostEnvironment environment, ILogger<CalibrationService> logger)
        {
            var appData = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
                "MM5AGM", "FTdx101 WebApp");
            Directory.CreateDirectory(appData);
            _calibrationFilePath = Path.Combine(appData, "calibration.user.json");
            _logger = logger;
        }

        public async Task<CalibrationSettings> GetCalibrationAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (File.Exists(_calibrationFilePath))
                {
                    var json = await File.ReadAllTextAsync(_calibrationFilePath);
                    _cachedSettings = JsonSerializer.Deserialize<CalibrationSettings>(json) ?? new CalibrationSettings();
                }
                else
                {
                    _cachedSettings = new CalibrationSettings();
                }
                // Cleanup: Remove empty points from all meters
                CleanupEmptyPoints(_cachedSettings);
                return _cachedSettings;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // Remove points where both GaugeValue and ActualValue are empty or zero
        private void CleanupEmptyPoints(CalibrationSettings settings)
        {
            bool IsEmptyOrZero(string? s)
            {
                if (string.IsNullOrWhiteSpace(s)) return true;
                var trimmed = s.Trim();
                return trimmed == "0" || trimmed == "0.0";
            }
            void Clean(MeterCalibration meter)
            {
                meter.Points = meter.Points
                    .Where(p => !(IsEmptyOrZero(p.GaugeValue) && IsEmptyOrZero(p.ActualValue)))
                    .ToList();
            }
            Clean(settings.SMeter);
            Clean(settings.SWR);
            Clean(settings.Power);
            Clean(settings.ALC);
        }

        public async Task SaveCalibrationAsync(CalibrationSettings settings)
        {
            await _semaphore.WaitAsync();
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(settings, options);
                await File.WriteAllTextAsync(_calibrationFilePath, json);
                _cachedSettings = settings;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
