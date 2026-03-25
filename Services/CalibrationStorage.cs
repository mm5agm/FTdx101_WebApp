using System.Text.Json;
using FTdx101_WebApp.Models.Calibration;

namespace FTdx101_WebApp.Services;

public static class CalibrationStorage
{
    private static readonly string UserPath =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MM5AGM",
            "FTdx101 WebApp",
            "calibration.user.json");

    // Path to the pristine default calibration template (used only for initial copy)
    private static readonly string DefaultPath =
        Path.Combine(AppContext.BaseDirectory, "wwwroot", "calibration.default.json");

    public static CalibrationFile Load()
    {
        // If user calibration does not exist, copy from pristine default template
        if (!File.Exists(UserPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(UserPath)!);
            File.Copy(DefaultPath, UserPath, overwrite: false);
        }
        // Always load from user calibration file
        var json = File.ReadAllText(UserPath);
        return JsonSerializer.Deserialize<CalibrationFile>(json)!;
    }

    public static CalibrationFile LoadDefault()
    {
        // Loads the pristine default calibration template (for diagnostics or reset only)
        var defJson = File.ReadAllText(DefaultPath);
        return JsonSerializer.Deserialize<CalibrationFile>(defJson)!;
    }

    public static void Save(CalibrationFile file)
    {
        // Always save to user calibration file
        Directory.CreateDirectory(Path.GetDirectoryName(UserPath)!);
        var json = JsonSerializer.Serialize(
            file,
            new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(UserPath, json);
    }
}
