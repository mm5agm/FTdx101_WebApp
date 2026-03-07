namespace FTdx101_WebApp.Models
{
    public class ApplicationSettings
    {
        // Connection Settings
        public string SerialPort { get; set; } = "COM3";
        public int BaudRate { get; set; } = 38400;
        public string WebAddress { get; set; } = "0.0.0.0"; // Bind to all interfaces
        public string RadioModel { get; set; } = "FTdx101MP"; // MP = dual receiver, D = single receiver


        // External Applications - Command Lines
        public string WsjtxCommandLine { get; set; } = @"C:\WSJT\wsjtx\bin\wsjtx.exe --rig-name=WebApp";
        public string JtalertCommandLine { get; set; } = @"C:\HamApps\JTAlert\JTAlert.exe";
        public string Log4omCommandLine { get; set; } = @"C:\Program Files (x86)\Log4OM 2\Log4OM.exe";

        // External Applications - Custom Names (user can rename buttons)
        public string App1Name { get; set; } = "WSJT-X";
        public string App2Name { get; set; } = "JTAlert";
        public string App3Name { get; set; } = "Log4OM";

        // External Applications - Show/Hide buttons (optional apps)
        public bool ShowWsjtxButton { get; set; } = true;
        public bool ShowJtalertButton { get; set; } = true;
        public bool ShowLog4omButton { get; set; } = true;

        // WSJT-X UDP Settings
        public string WsjtxUdpAddress { get; set; } = "127.0.0.1";
        public int WsjtxUdpPort { get; set; } = 2237;

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