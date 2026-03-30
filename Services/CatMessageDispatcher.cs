namespace FTdx101_WebApp.Services
{
    /// <summary>
    /// Routes CAT messages to appropriate handlers and updates radio state
    /// </summary>
    public class CatMessageDispatcher
    {
        private readonly RadioStateService _stateService;

        // Callback for initialization complete
        public Action? OnInitializationComplete { get; set; }

        public CatMessageDispatcher(RadioStateService stateService)
        {
            _stateService = stateService;
        }

        /// <summary>
        /// Process a complete CAT message and update state
        /// </summary>
        public void DispatchMessage(string message)
        {
            // Debug logging removed as part of cleanup

            if (string.IsNullOrEmpty(message) || message.Length < 3)
                return;

            int semicolonCount = message.Count(c => c == ';');
            if (semicolonCount > 1)
            {
                var parts = message.Split(';');
                foreach (var part in parts)
                {
                    if (!string.IsNullOrWhiteSpace(part))
                        DispatchMessage(part + ";");
                }
                return;
            }

            string command = message.Substring(0, 2);

            try
            {
                switch (command)
                {
                    case "FA":
                        // Example: FA01420000;
                        if (long.TryParse(message.Substring(2).TrimEnd(';'), out var freqA))
                        {
                            _stateService.FrequencyA = freqA;
                        }
                        break;
                    case "FB":
                        if (long.TryParse(message.Substring(2).TrimEnd(';'), out var freqB))
                        {
                            _stateService.FrequencyB = freqB;
                        }
                        break;
                    case "DT":
                        HandleInitialization(message);
                        break;
                    case "MD":
                        // Example: MD01; (VFO A, LSB), MD12; (VFO B, USB)
                        if (message.Length >= 5)
                        {
                            var vfo = message[2]; // '0' for A, '1' for B
                            var modeCode = message[3];
                            string? mode = modeCode switch
                            {
                                '1' => "LSB",
                                '2' => "USB",
                                '3' => "CW-U",
                                '4' => "FM",
                                '5' => "AM",
                                '6' => "RTTY-L",
                                '7' => "CW-L",
                                '8' => "DATA-L",
                                '9' => "RTTY-U",
                                'A' => "DATA-FM",
                                'B' => "FM-N",
                                'C' => "DATA-U",
                                'D' => "AM-N",
                                'E' => "PSK",
                                'F' => "DATA-FM-N",
                                _ => null
                            };
                            if (mode != null)
                            {
                                if (vfo == '0')
                                    _stateService.ModeA = mode;
                                else if (vfo == '1')
                                    _stateService.ModeB = mode;
                            }
                        }
                        break;
                    case "PC":
                        // Example: PC100; (100W)
                        if (message.Length >= 5 && int.TryParse(message.Substring(2, 3), out var watts))
                        {
                            _stateService.Power = watts;
                        }
                        break;
                    case "TX":
                        // Example: TX0; (not transmitting), TX1; (transmitting)
                        if (message.Length >= 4)
                        {
                            var txStatus = message[2];
                            _stateService.IsTransmitting = (txStatus == '1' || txStatus == '2');
                        }
                        break;
                    case "RF":
                        // Example: RF06; (VFO A, 12kHz filter), RF19; (VFO B, 600Hz filter)
                        // Response format: RF + P1 (0=Main/A, 1=Sub/B) + P3 (filter code 6-A)
                        if (message.Length >= 4)
                        {
                            var vfo = message[2]; // '0' for A, '1' for B
                            var filterCode = message[3].ToString();
                            if (vfo == '0')
                                _stateService.RoofingFilterA = filterCode;
                            else if (vfo == '1')
                                _stateService.RoofingFilterB = filterCode;
                        }
                        break;
                    // No debug logging for unhandled commands
                }
            }
            catch (Exception)
            {
                // Suppress diagnostics
            }
        }

        private void HandleInitialization(string message)
        {

            // Only signal initialization complete for DT0; message
            if (message.StartsWith("DT0;") || message.StartsWith("DT0"))
            {
                _stateService.CompleteInitialization(); // Optionally update radio state
                OnInitializationComplete?.Invoke();     // Notify any listeners
            }
        }
    }
}