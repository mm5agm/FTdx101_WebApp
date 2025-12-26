namespace FTdx101_WebApp.Models
{
    public class ApplicationSettings
    {
        // Connection Settings
        public string SerialPort { get; set; } = "COM3";
        public int BaudRate { get; set; } = 38400;
        public string WebAddress { get; set; } = "0.0.0.0"; // Changed to bind to all interfaces
        public int WebPort { get; set; } = 8080;
        public string RadioModel { get; set; } = "FTdx101MP"; // MP = dual receiver, D = single receiver
        
        // Last Radio State (persisted between sessions)
        public RadioState LastRadioState { get; set; } = new();
    }

    public class RadioState
    {
        // VFO-A State
        public long FrequencyA { get; set; } = 14074000; // Default: 14.074 MHz (FT8)
        public string ModeA { get; set; } = "USB";
        public string AntennaA { get; set; } = "1";
        
        // VFO-B State
        public long FrequencyB { get; set; } = 14074000; // Default: 14.074 MHz (FT8)
        public string ModeB { get; set; } = "USB";
        public string AntennaB { get; set; } = "1";
    }
}