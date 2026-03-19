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
            void Clean(MeterCalibration meter, string type)
            {
                switch (type)
                {
                    case "SMeter":
                        meter.Points = meter.Points
                            .Where(p => !(IsEmptyOrZero(p.SPoint) && IsEmptyOrZero(p.RawValue)))
                            .ToList();
                        break;
                    case "SWR":
                        meter.Points = meter.Points
                            .Where(p => !(IsEmptyOrZero(p.SWR) && IsEmptyOrZero(p.RawValue)))
                            .ToList();
                        break;
                    case "Power":
                        meter.Points = meter.Points
                            .Where(p => !(IsEmptyOrZero(p.Power) && IsEmptyOrZero(p.RawValue)))
                            .ToList();
                        break;
                    case "ALC":
                        meter.Points = meter.Points
                            .Where(p => !(IsEmptyOrZero(p.ALC) && IsEmptyOrZero(p.RawValue)))
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
                // Project to only relevant fields for each meter
                var minimal = new
                {
                    SMeter = new
                    {
                        Points = settings.SMeter.Points.Select(p => new { p.SPoint, p.RawValue }).ToList()
                    },
                    SWR = new
                    {
                        Points = settings.SWR.Points.Select(p => new { p.SWR, p.RawValue }).ToList()
                    },
                    Power = new
                    {
                        Points = settings.Power.Points.Select(p => new { p.Power, p.RawValue }).ToList()
                    },
                    ALC = new
                    {
                        Points = settings.ALC.Points.Select(p => new { p.ALC, p.RawValue }).ToList()
                    }
                };
                var json = JsonSerializer.Serialize(minimal, options);
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
