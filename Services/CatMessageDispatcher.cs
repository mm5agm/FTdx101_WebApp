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
            _logger.LogInformation("[CatMessageDispatcher] DispatchMessage called with: {Message}", message);

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
                    default:
                        _logger.LogDebug("[CatMessageDispatcher] Unhandled command: {Command} - {Message}", command, message);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CatMessageDispatcher] Error dispatching message: {Message}", message);
            }
        }

        private void HandleInitialization(string message)
        {
            _logger.LogWarning("[CatMessageDispatcher] >>> HandleInitialization called with: {Message}", message);

            if (message.StartsWith("DT") && message.EndsWith(";"))
            {
                initialization_complete = true;
                _logger.LogWarning("[CatMessageDispatcher] >>> Initialization complete: DT response received ({Message})", message);

                if (OnInitializationComplete != null)
                {
                    _logger.LogWarning("[CatMessageDispatcher] >>> OnInitializationComplete callback is set, invoking...");
                    OnInitializationComplete.Invoke();
                }
                else
                {
                    _logger.LogWarning("[CatMessageDispatcher] >>> OnInitializationComplete callback is NOT set.");
                }
            }
            else if (message.Length >= 3)
            {
                _logger.LogWarning("[CatMessageDispatcher] >>> Initialization message received: {Message}", message);
            }
        }
    }
}