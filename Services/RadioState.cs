namespace FTdx101_WebApp.Services
{
    /// <summary>
    /// Represents the persistent state of the radio (both receivers A and B).
    /// This state is saved to radio_state.json and can be manually edited.
    /// </summary>
    public class RadioState
    {
        /// <summary>
        /// Receiver A frequency in Hz (e.g., 14074000 for 14.074 MHz)
        /// </summary>
        public long FrequencyA { get; set; }

        /// <summary>
        /// Receiver B frequency in Hz (e.g., 7074000 for 7.074 MHz)
        /// </summary>
        public long FrequencyB { get; set; }

        /// <summary>
        /// Receiver A band (e.g., "20m", "40m", "80m")
        /// </summary>
        public string BandA { get; set; } = "";

        /// <summary>
        /// Receiver B band (e.g., "20m", "40m", "80m")
        /// </summary>
        public string BandB { get; set; } = "";

        /// <summary>
        /// Receiver A mode (e.g., "USB", "LSB", "CW", "FM", "AM", "DATA-USB")
        /// </summary>
        public string ModeA { get; set; } = "";

        /// <summary>
        /// Receiver B mode (e.g., "USB", "LSB", "CW", "FM", "AM", "DATA-USB")
        /// </summary>
        public string ModeB { get; set; } = "";

        /// <summary>
        /// Receiver A antenna selection (1, 2, or 3)
        /// </summary>
        public string AntennaA { get; set; } = "";

        /// <summary>
        /// Receiver B antenna selection (1, 2, or 3)
        /// </summary>
        public string AntennaB { get; set; } = "";

        /// <summary>
        /// Receiver A power level (0-100)
        /// </summary>
        public int PowerA { get; set; } = 0;

        /// <summary>
        /// Receiver B power level (0-100)
        /// </summary>
        public int PowerB { get; set; }

        /// <summary>
        /// Additional controls and settings (for future expansion)
        /// </summary>
        public Dictionary<string, object> Controls { get; set; } = new();
    }
}