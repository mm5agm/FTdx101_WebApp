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
                // Assign new Guid to any CalibrationPoint with empty Id (for legacy or hand-edited files)
                void EnsureIds(MeterCalibration meter, string type)
                {
                    foreach (var p in meter.Points)
                    {
                        if (p.Id == Guid.Empty)
                        {
                            p.Id = Guid.NewGuid();
                        }
                    }
                }
                EnsureIds(_cachedSettings.SMeter, "SMeter");
                EnsureIds(_cachedSettings.SWR, "SWR");
                EnsureIds(_cachedSettings.Power, "Power");
                EnsureIds(_cachedSettings.ALC, "ALC");

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
            bool IsTrulyEmpty(string? s)
            {
                return string.IsNullOrWhiteSpace(s);
            }
            void Clean(MeterCalibration meter, string type)
            {
                switch (type)
                {
                    case "SMeter":
                        meter.Points = meter.Points
                            .Where(p => !(IsTrulyEmpty(p.SPoint) && IsTrulyEmpty(p.RawValue)))
                            .ToList();
                        break;
                    case "SWR":
                        meter.Points = meter.Points
                            .Where(p => !(IsTrulyEmpty(p.SWR) && IsTrulyEmpty(p.RawValue)))
                            .ToList();
                        break;
                    case "Power":
                        meter.Points = meter.Points
                            .Where(p => !(IsTrulyEmpty(p.Power) && IsTrulyEmpty(p.RawValue)))
                            .ToList();
                        break;
                    case "ALC":
                        meter.Points = meter.Points
                            .Where(p => !(IsTrulyEmpty(p.ALC) && IsTrulyEmpty(p.RawValue)))
                            .ToList();
                        break;
                }
            }
            Clean(settings.SMeter, "SMeter");
            Clean(settings.SWR, "SWR");
            Clean(settings.Power, "Power");
            Clean(settings.ALC, "ALC");
        }

        public async Task SaveCalibrationAsync(CalibrationSettings settings)
        {
            await _semaphore.WaitAsync();
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                // Save the full CalibrationSettings object, including Ids
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
