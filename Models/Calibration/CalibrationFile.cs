
using System.Text.Json.Serialization;
namespace FTdx101_WebApp.Models.Calibration;

public class CalibrationFile
{
    [JsonPropertyName("meters")]
    public List<MeterCalibration> Meters { get; set; } = new();
}
