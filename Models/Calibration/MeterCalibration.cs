using System.Collections.Generic;

namespace FTdx101_WebApp.Models.Calibration
{
    using System.Text.Json.Serialization;

    // Represents a single calibration point for a meter
    public class CalibrationPoint
    {
        [JsonPropertyName("radio")]
        public double? Radio { get; set; }

        [JsonPropertyName("label")]
        public string? Label { get; set; }

        [JsonPropertyName("raw")]
        public double Raw { get; set; }
    }

    // Represents calibration and metadata for a meter
    public class MeterCalibration
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = "numeric";

        [JsonPropertyName("points")]
        public List<CalibrationPoint> Points { get; set; } = new();

        // For value meters, you can add a formula or scale property if needed
        [JsonPropertyName("isGauge")]
        public bool IsGauge { get; set; }

        [JsonPropertyName("units")]
        public string Units { get; set; } = string.Empty;
    }
}
