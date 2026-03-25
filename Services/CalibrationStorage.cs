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

    private static readonly string DefaultPath =
    Path.Combine(AppContext.BaseDirectory, "wwwroot", "calibration.default.json");

    public static CalibrationFile Load()
    {
        if (!File.Exists(UserPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(UserPath)!);
            File.Copy(DefaultPath, UserPath, overwrite: false);
        }
        var json = File.ReadAllText(UserPath);
        return JsonSerializer.Deserialize<CalibrationFile>(json)!;
    }

    public static CalibrationFile LoadDefault()
    {
        var defJson = File.ReadAllText(DefaultPath);
        return JsonSerializer.Deserialize<CalibrationFile>(defJson)!;
    }

    public static void Save(CalibrationFile file)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(UserPath)!);

        var json = JsonSerializer.Serialize(
            file,
            new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(UserPath, json);
    }
}
