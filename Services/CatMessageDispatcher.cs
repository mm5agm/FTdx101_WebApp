namespace FTdx101_WebApp.Services
{
    /// <summary>
    /// Routes CAT messages to appropriate handlers and updates radio state
    /// </summary>
    public class CatMessageDispatcher
    {
        private readonly RadioStateService _stateService;
        private readonly ILogger<CatMessageDispatcher> _logger;

        public CatMessageDispatcher(
            RadioStateService stateService,
            ILogger<CatMessageDispatcher> logger)
        {
            _stateService = stateService;
            _logger = logger;
        }

        private bool initialization_complete = false;

        /// <summary>
        /// Process a complete CAT message and update state
        /// </summary>
        public void DispatchMessage(string message)
        {
            _logger.LogDebug("DispatchMessagereceived: {Message}", message);

            if (string.IsNullOrEmpty(message) || message.Length < 3)
                return;

            // Only split if there are multiple commands in the message (more than one ';')
            int semicolonCount = message.Count(c => c == ';');
            if (semicolonCount > 1)
            {
                var parts = message.Split(';');
                foreach (var part in parts)
                {
                    if (!string.IsNullOrWhiteSpace(part))
                        DispatchMessage(part + ";");
                }
                return; // Prevent further processing in this call
            }

            string command = message.Substring(0, 2);

            try
            {
                switch (command)
                {
                    case "FA": // VFO A Frequency
                        HandleFrequencyA(message);
                        break;
                    case "FB": // VFO B Frequency
                        _logger.LogWarning("=== DISPATCH_FB === About to call HandleFrequencyB with: {Message}", message);
                        HandleFrequencyB(message);
                        break;
                    case "MD": // Mode
                        HandleMode(message);
                        break;
                    case "SM": // S-Meter
                         HandleSMeter(message);
                         break;
                    case "PC": // Power
                        HandlePower(message);
                        break;
                    case "IF": // Information
                        HandleInformation(message);
                        break;
                    case "AN": // Antenna
                        HandleAntenna(message);
                        break;
                    case "TX": // TX Status
                        HandleTxStatus(message);
                        break;
                    case "AI": // Auto Information status
                        _logger.LogWarning("=== AI (Auto Information) message received: {Message} ===", message);
                        break;
                    case "DT": // Dummy Trigram for initialization
                        HandleInitialization(message);
                        break;
                    case "OI": // Opposite Band Information
                        HandleOppositeInformation(message);
                        break;
                    default:
                        _logger.LogDebug("Unhandled command: {Command} - {Message}", command, message);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching message: {Message}", message);
            }
        }

        private void HandleFrequencyA(string message)
        {
            if (message.Length >= 11)
            {
                long freq = CatCommands.ParseFrequency(message);
                _logger.LogDebug("Parsed FA message: {Message} -> {Freq}", message, freq);
                if (freq > 0)
                {
                    _stateService.FrequencyA = freq;
                }
            }
        }

        private void HandleFrequencyB(string message)
        {
            _logger.LogWarning("=== HANDLE_FREQUENCY_B_CALLED === FB message: {Message}", message);
            if (message.Length >= 11)
            {
                long freq = CatCommands.ParseFrequency(message);
                _logger.LogDebug("=== VFOB_DEBUG === FB message: {Message} -> {Freq}", message, freq);
                if (freq > 0)
                {
                    _stateService.FrequencyB = freq;
                }
            }
        }

        private void HandleMode(string message)
        {
            if (message.Length >= 4)
            {
                string mode = CatCommands.ParseMode(message);
                if (message[2] == '0')
                    _stateService.ModeA = mode;
                else
                    _stateService.ModeB = mode;
            }
        }

        private void HandleSMeter(string message)
        {
            if (message.Length >= 4)
            {
                int sMeter = CatCommands.ParseSMeter(message);
                if (message[2] == '0')
                {
                    _stateService.SMeterA = sMeter;
                }
                else
                {
                    _stateService.SMeterB = sMeter;
                }
            }
        }

        private void HandlePower(string message)
        {
            if (message.Length >= 5 && int.TryParse(message.Substring(2, 3), out int power))
            {
                _stateService.Power = power;
            }
        }

        private void HandleInformation(string message)
        {
            _logger.LogWarning("=== IF (Information) message received: {Message} ===", message);

            // Do NOT update FrequencyA here unless you are certain the IF message is for VFO A.
            // If you want to parse other info from IF, do it here.
            // Remove or comment out the frequency update below:
            // if (message.Length >= 13)
            // {
            //     string freqStr = message.Substring(2, 10);
            //     if (long.TryParse(freqStr, out long freq) && freq > 0)
            //     {
            //         _logger.LogWarning("=== IF Frequency parsed: {Freq} ===", freq);
            //         _stateService.FrequencyA = freq;
            //     }
            // }
        }

        private void HandleAntenna(string message)
        {
            if (message.Length >= 4)
            {
                string antenna = message.Substring(3, 1);
                if (message[2] == '0')
                    _stateService.AntennaA = antenna;
                else
                    _stateService.AntennaB = antenna;
            }
        }

        private void HandleTxStatus(string message)
        {
            if (message.Length >= 3)
            {
                _stateService.IsTransmitting = message[2] == '1';
            }
        }

        public void HandleResponse(string response)
        {
            if (response.StartsWith("DT0") && response.EndsWith(";"))
            {
                initialization_complete = true;
                // Optionally log or trigger UI update
            }
            else if (response.StartsWith("FA")) // Example for frequency response
            {
                // Parse frequency and update RadioStateService.FrequencyA
            }
            // Handle frequency and other responses as usual
        }

        private void HandleInitialization(string message)
        {
            if (message.StartsWith("DT0") && message.EndsWith(";"))
            {
                initialization_complete = true;
                _logger.LogInformation("Initialization complete: DT0 response received ({Message})", message);
                // Optionally, trigger any post-initialization logic here
            }
            else if (message.Length >= 3)
            {
                _logger.LogInformation("Initialization message received: {Message}", message);
            }
        }

        private void HandleOppositeInformation(string message)
        {
            // OI message: OI + VFO-B frequency at a fixed position (see your radio's CAT manual)
            // Do NOT update FrequencyB here!
            _logger.LogInformation("Received OI (Opposite Band) message: {Message}", message);
        }

        // Example: Only process frequency messages that start with "FA" or "FB" and are the correct length
        public void HandleCatMessage(string message)
        {
            // Example for Receiver A
            if (message.StartsWith("FA") && message.Length >= 11)
            {
                // FA + 9 digits + ;
                var freqStr = message.Substring(2, 9);
                if (long.TryParse(freqStr, out var freq))
                {
                    // Update FrequencyA in your state
                    _stateService.FrequencyA = freq;
                }
                return;
            }

            // Example for Receiver B
            if (message.StartsWith("FB") && message.Length >= 11)
            {
                // FB + 9 digits + ;
                var freqStr = message.Substring(2, 9);
                if (long.TryParse(freqStr, out var freq))
                {
                    // Update FrequencyB in your state
                    _stateService.FrequencyB = freq;
                }
                return;
            }

            // Ignore all other message types for frequency updates
        }
    }
}