namespace FTdx101_WebApp.Services
{
    /// <summary>
    /// Routes CAT messages to appropriate handlers and updates radio state
    /// </summary>
    public class CatMessageDispatcher
    {
        private readonly RadioStateService _stateService;
        private readonly ILogger<CatMessageDispatcher> _logger;

        // Callback for initialization complete
        public Action? OnInitializationComplete { get; set; }

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
            // Suppress logging RM messages (meter readings poll frequently)
            // But DO log TX messages for debugging
            if (!message.StartsWith("RM"))
            {
                _logger.LogInformation("[CatMessageDispatcher] Received: {Message}", message);
            }

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
                           _stateService.PowerA = watts;
                        }
                        break;
                    case "TX":
                        // Example: TX0; (not transmitting), TX1; (transmitting)
                        if (message.Length >= 4)
                        {
                            var txStatus = message[2];
                            _stateService.IsTransmitting = (txStatus == '1' || txStatus == '2');
                            _logger.LogInformation("[CatMessageDispatcher] TX status: {Status} (IsTransmitting={IsTransmitting})", 
                                txStatus, _stateService.IsTransmitting);
                        }
                        break;
                    // No debug logging for unhandled commands
                }
            }
            catch (Exception ex)
            {
                // Optionally keep error logging
                _logger.LogError(ex, "[CatMessageDispatcher] Error dispatching message: {Message}", message);
            }
        }

        private void HandleInitialization(string message)
        {
            _logger.LogWarning("[CatMessageDispatcher] HandleInitialization called with message: {Message}", message);

            // Only signal initialization complete for DT0; message
            if (message.StartsWith("DT0;") || message.StartsWith("DT0"))
            {
                _logger.LogWarning("[CatMessageDispatcher] DT0 detected! Completing initialization...");
                _stateService.CompleteInitialization(); // Optionally update radio state
                _logger.LogWarning("[CatMessageDispatcher] About to invoke OnInitializationComplete callback");
                OnInitializationComplete?.Invoke();     // Notify any listeners
                _logger.LogWarning("[CatMessageDispatcher] OnInitializationComplete callback invoked");
            }
            else
            {
                _logger.LogWarning("[CatMessageDispatcher] Message did not match DT0 pattern: {Message}", message);
            }
        }
    }
}