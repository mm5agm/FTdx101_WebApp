using FTdx101_WebApp.Models.Calibration;

namespace FTdx101_WebApp.Services;


public interface ICalibrationService
{
    Dictionary<string, List<CalibrationPoint>> GetAllCalibrationTables();
    CalibrationFile Current { get; }
    double CalibrateNumeric(string meterName, double raw);
    string CalibrateSMeterLabel(double raw);
    void Save(CalibrationFile file);
    void ResetToDefault();
}

public class CalibrationService : ICalibrationService
{
    public CalibrationFile Current { get; private set; }

    public CalibrationService()
    {
        Current = CalibrationStorage.Load();
    }
    public Dictionary<string, List<CalibrationPoint>> GetAllCalibrationTables()
{
    var all = Current.Meters
        .GroupBy(m => m.Name)
        .ToDictionary(
            g => g.Key,
            g => g.SelectMany(m => m.Points).ToList()
        );

    // Debug: Write SWR calibration points to file for diagnostics
    if (all.ContainsKey("SWR"))
    {
        try
        {
            var path = "C:\\Temp\\swr_debug.txt";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            System.IO.File.AppendAllText(path, "[DebugBackend] SWR calibration points:\n");
            foreach (var pt in all["SWR"])
                System.IO.File.AppendAllText(path, System.Text.Json.JsonSerializer.Serialize(pt) + "\n");
        }
        catch { }
    }

    return all;
}

    public double CalibrateNumeric(string meterName, double raw)
    {
        var meter = Current.Meters
            .FirstOrDefault(m => m.Name == meterName && m.Type == "numeric");

        if (meter == null || meter.Points.Count == 0)
            return raw;

        var points = meter.Points.OrderBy(p => p.Raw).ToList();

        if (raw <= points.First().Raw) return points.First().Radio ?? raw;
        if (raw >= points.Last().Raw) return points.Last().Radio ?? raw;

        for (int i = 0; i < points.Count - 1; i++)
        {
            var a = points[i];
            var b = points[i + 1];

            if (raw >= a.Raw && raw <= b.Raw)
            {
                var t = (raw - a.Raw) / (b.Raw - a.Raw);
                var ra = a.Radio ?? a.Raw;
                var rb = b.Radio ?? b.Raw;
                return ra + t * (rb - ra);
            }
        }

        return raw;
    }

    public string CalibrateSMeterLabel(double raw)
    {
        var meter = Current.Meters
            .FirstOrDefault(m => m.Name == "S-Meter" && m.Type == "s_meter");

        if (meter == null || meter.Points.Count == 0)
            return raw.ToString("F0");

        var points = meter.Points.OrderBy(p => p.Raw).ToList();

        var nearest = points.OrderBy(p => Math.Abs(p.Raw - raw)).First();
        return nearest.Label ?? raw.ToString("F0");
    }

    public void Save(CalibrationFile file)
    {
        Current = file;
        CalibrationStorage.Save(file);
    }

    public void ResetToDefault()
    {
        Current = CalibrationStorage.LoadDefault();
        CalibrationStorage.Save(Current);
    }
   
}

