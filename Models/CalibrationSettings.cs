using System.Collections.Generic;

namespace FTdx101_WebApp.Models
{
    public class CalibrationSettings
    {
        public MeterCalibration SMeter { get; set; } = new();
        public MeterCalibration SWR { get; set; } = new();
        public MeterCalibration Power { get; set; } = new();
        public MeterCalibration ALC { get; set; } = new();
    }

    public class MeterCalibration
    {
        public List<CalibrationPoint> Points { get; set; } = new()
        {
            new CalibrationPoint(),
            new CalibrationPoint(),
            new CalibrationPoint(),
            new CalibrationPoint(),
            new CalibrationPoint()
        };
    }

    public class CalibrationPoint
    {
        // For S-Meter, GaugeValue is a string (e.g., 'S9+20'), for others it's numeric but stored as string for model binding
        public string GaugeValue { get; set; } = string.Empty;
        public string ActualValue { get; set; } = string.Empty;
    }
}
