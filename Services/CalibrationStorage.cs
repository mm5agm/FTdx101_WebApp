using System.Text.Json;
using FTdx101_WebApp.Models.Calibration;
using Microsoft.Extensions.Hosting;

namespace FTdx101_WebApp.Services;

public class CalibrationStorage
{
    private readonly bool _isDevelopment;
    private readonly string _defaultPath;
    private readonly string _userPath;

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true
    };

    public CalibrationStorage(IHostEnvironment hostEnvironment)
    {
        _isDevelopment = hostEnvironment.IsDevelopment();
        _defaultPath = Path.Combine(hostEnvironment.ContentRootPath, "wwwroot", "calibration.default.json");
        _userPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MM5AGM",
            "FTdx101",
            "WebApp",
            "calibration.user.json");
    }

    public bool IsDevelopmentMode => _isDevelopment;

    public string GetActivePath() => _isDevelopment ? _defaultPath : _userPath;

    public CalibrationFile Load()
    {
        if (_isDevelopment)
        {
            return LoadFromPath(_defaultPath);
        }

        EnsureUserCalibrationExists();
        return LoadFromPath(_userPath);
    }

    public CalibrationFile LoadDefault()
    {
        return LoadFromPath(_defaultPath);
    }

    public void Save(CalibrationFile file)
    {
        var targetPath = GetActivePath();
        var directory = Path.GetDirectoryName(targetPath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(file, WriteOptions);
        File.WriteAllText(targetPath, json);
    }

    private void EnsureUserCalibrationExists()
    {
        if (File.Exists(_userPath))
        {
            return;
        }

        var userDir = Path.GetDirectoryName(_userPath);
        if (!string.IsNullOrWhiteSpace(userDir))
        {
            Directory.CreateDirectory(userDir);
        }

        File.Copy(_defaultPath, _userPath, overwrite: false);
    }

    private static CalibrationFile LoadFromPath(string path)
    {
        var json = File.ReadAllText(path);
        var file = JsonSerializer.Deserialize<CalibrationFile>(json, ReadOptions) ?? new CalibrationFile();

        foreach (var meter in file.Meters)
        {
            meter.Normalize();
            meter.Points = meter.Points.OrderBy(p => p.Raw).ToList();
        }

        return file;
    }
}
