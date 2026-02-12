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

        private bool initialization_complete = false;

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
            // No extra logging here unless you want to keep error logs
            if (message.StartsWith("DT"))
            {
                initialization_complete = true;
                _stateService.CompleteInitialization(); // <-- Add this line
                OnInitializationComplete?.Invoke();
            }
        }
    }
}