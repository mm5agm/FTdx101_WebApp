namespace FTdx101_WebApp.Services
{
    public class RadioState
    {
        public long FrequencyA { get; set; }
        public string BandA { get; set; } = "";
        public string ModeA { get; set; } = "";
        public string AntennaA { get; set; } = "";
        public long FrequencyB { get; set; }
        public string BandB { get; set; } = "";
        public string ModeB { get; set; } = "";
        public string AntennaB { get; set; } = "";
        public Dictionary<string, object> Controls { get; set; } = new();
    }
}