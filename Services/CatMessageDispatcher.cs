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

        /// <summary>
        /// Process a complete CAT message and update state
        /// </summary>
        public void DispatchMessage(string message)
        {
            if (string.IsNullOrEmpty(message) || message.Length < 3)
                return;

            string command = message.Substring(0, 2);

            try
            {
                switch (command)
                {
                    case "FA": // VFO A Frequency
                        HandleFrequencyA(message);
                        break;

                    case "FB": // VFO B Frequency
                        HandleFrequencyB(message);
                        break;

                    case "MD": // Mode
                        HandleMode(message);
                        break;

                    // COMMENT THIS OUT AGAIN - polling service handles it better:
                    // case "SM": // S-Meter
                    //     HandleSMeter(message);
                    //     break;

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
                        _logger.LogInformation("AI mode: {Message}", message);
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
            long freq = CatCommands.ParseFrequency(message);
            if (freq > 0)
            {
                _stateService.FrequencyA = freq;
            }
        }

        private void HandleFrequencyB(string message)
        {
            long freq = CatCommands.ParseFrequency(message);
            if (freq > 0)
            {
                _stateService.FrequencyB = freq;
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
                    _logger.LogDebug("Dispatcher updated SMeterA: {Value}", sMeter);
                }
                else
                {
                    _stateService.SMeterB = sMeter;
                    _logger.LogDebug("Dispatcher updated SMeterB: {Value}", sMeter);
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
            var (freq, mode) = IFCommandParser.ParseIFResponse(message);
            if (freq > 0)
            {
                _stateService.FrequencyA = freq;
                _stateService.ModeA = mode;
            }
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
    }
}