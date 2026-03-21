using System.Text.Json.Serialization;

namespace FTdx101_WebApp.Models.Calibration;

public class CalibrationFile
{
    [JsonPropertyName("meters")]
    public List<MeterCalibration> Meters { get; set; } = new();
}

public class MeterCalibration
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "numeric";

    [JsonPropertyName("points")]
    public List<CalibrationPoint> Points { get; set; } = new();
}

public class CalibrationPoint
{
    [JsonPropertyName("radio")]
    public double? Radio { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("raw")]
    public double Raw { get; set; }
}
