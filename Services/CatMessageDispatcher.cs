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
                    case "GT":
                        // GT0P2; or GT1P2; — P2: 0=OFF 1=FAST 2=MID 3=SLOW 4=AUTO 5/6=AUTO variant
                        // Values 5 and 6 (AUTO-FAST / AUTO-MID / AUTO-SLOW) are read-only settled
                        // states; normalise them to "4" (AUTO) so the UI dropdown stays consistent.
                        if (message.Length >= 4)
                        {
                            var agcVfo = message[2];
                            var agcCode = message[3].ToString();
                            if (agcCode == "5" || agcCode == "6") agcCode = "4";
                            if (agcVfo == '0') _stateService.AgcA = agcCode;
                            else if (agcVfo == '1') _stateService.AgcB = agcCode;
                        }
                        break;
                    case "PA":
                        // PA{vfo}{code}; — vfo: 0=Main 1=Sub; code: 0=IPO 1=AMP1 2=AMP2
                        if (message.Length >= 4)
                        {
                            var vfo = message[2];
                            var code = message[3].ToString();
                            if (vfo == '0') _stateService.IpoA = code;
                            else if (vfo == '1') _stateService.IpoB = code;
                        }
                        break;
                    case "BC":
                        // BC{vfo}{code}; — vfo: 0=Main 1=Sub; code: 0=OFF 1=ON
                        if (message.Length >= 4)
                        {
                            var vfo = message[2];
                            var code = message[3].ToString();
                            if (vfo == '0') _stateService.AutoNotchA = code;
                            else if (vfo == '1') _stateService.AutoNotchB = code;
                        }
                        break;
                    case "NR":
                        // NR{vfo}{code}; — vfo: 0=Main 1=Sub; code: 0=OFF 1=NR1 2=NR2
                        if (message.Length >= 4)
                        {
                            var vfo = message[2];
                            var code = message[3].ToString();
                            if (vfo == '0') _stateService.NrA = code;
                            else if (vfo == '1') _stateService.NrB = code;
                        }
                        break;
                    case "NB":
                        // NB{vfo}{code}; — vfo: 0=Main 1=Sub; code: 0=OFF 1=ON
                        if (message.Length >= 4)
                        {
                            var vfo = message[2];
                            var code = message[3].ToString();
                            if (vfo == '0') _stateService.NbA = code;
                            else if (vfo == '1') _stateService.NbB = code;
                        }
                        break;
                    case "RA":
                        // RA{vfo}{nn}; — vfo: 0=Main 1=Sub; nn: 00=OFF 06=6dB 12=12dB 18=18dB
                        if (message.Length >= 5)
                        {
                            var vfo = message[2];
                            var code = message.Substring(3, 2);
                            if (vfo == '0') _stateService.AttA = code;
                            else if (vfo == '1') _stateService.AttB = code;
                        }
                        break;
                    case "BP":
                        // BP{vfo}{param}{3-digit value};
                        // param 0 = on/off: 000=OFF 001=ON
                        // param 1 = frequency: value × 10 = Hz (001=10Hz … 320=3200Hz)
                        if (message.Length >= 7)
                        {
                            var vfo = message[2];
                            var param = message[3];
                            var val = message.Substring(4, 3);
                            if (param == '0')
                            {
                                var isOn = val == "001" ? "1" : "0";
                                if (vfo == '0') _stateService.ManualNotchA = isOn;
                                else if (vfo == '1') _stateService.ManualNotchB = isOn;
                            }
                            else if (param == '1' && int.TryParse(val, out int raw) && raw > 0)
                            {
                                var hz = raw * 10;
                                if (vfo == '0') _stateService.ManualNotchFreqA = hz;
                                else if (vfo == '1') _stateService.ManualNotchFreqB = hz;
                            }
                        }
                        break;
                    case "SH":
                        // SH{vfo}{n}; — vfo: 0=Main 1=Sub; n: 0-8 (200Hz to 3000Hz)
                        if (message.Length >= 4)
                        {
                            var vfo = message[2];
                            var code = message[3].ToString();
                            if (vfo == '0') _stateService.IfWidthA = code;
                            else if (vfo == '1') _stateService.IfWidthB = code;
                        }
                        break;
                    case "IS":
                        // IS{vfo}{nnnn}; — 0000=min(-1000Hz) 5000=center(0Hz) 9999=max(+1000Hz)
                        if (message.Length >= 7 && int.TryParse(message.Substring(3, 4), out int rawShift))
                        {
                            var vfo = message[2];
                            var shiftHz = (int)Math.Round(rawShift / 9999.0 * 2000 - 1000);
                            if (vfo == '0') _stateService.IfShiftA = shiftHz;
                            else if (vfo == '1') _stateService.IfShiftB = shiftHz;
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