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
            // Suppress logging RM messages
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
            // Only signal initialization complete for DT0; message
            if (message.StartsWith("DT0;"))
            {
                _stateService.CompleteInitialization(); // Optionally update radio state
                OnInitializationComplete?.Invoke();     // Notify any listeners
            }
        }
    }
}