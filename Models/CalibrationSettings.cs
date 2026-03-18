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
        public List<CalibrationPoint> Points { get; set; } = new();
    }

    public class CalibrationPoint
    {
        // For S-Meter, SPoint is a string (e.g., 'S9+20'), for others it's numeric but stored as string for model binding
        public string SPoint { get; set; } = string.Empty; // For S-Meter
        public string SWR { get; set; } = string.Empty; // For SWR
        public string Power { get; set; } = string.Empty; // For Power
        public string ALC { get; set; } = string.Empty; // For ALC
        public string RawValue { get; set; } = string.Empty; // Raw value for all meters
    }
}
