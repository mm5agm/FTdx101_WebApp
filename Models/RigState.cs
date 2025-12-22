namespace FTdx101_WebApp.Models
{
    public class RigState
    {
        public int SMeterLevel { get; set; }
        public long Frequency { get; set; }
        public string Mode { get; set; } = string.Empty;
        public bool IsTransmitting { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}