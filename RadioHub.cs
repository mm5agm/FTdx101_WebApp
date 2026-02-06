using Microsoft.AspNetCore.SignalR;

namespace FTdx101_WebApp.Hubs
{
    /// <summary>
    /// SignalR hub for real-time radio state updates
    /// </summary>
    public class RadioHub : Hub
    {
        private readonly ILogger<RadioHub> _logger;

        public RadioHub(ILogger<RadioHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendInitializationStatus(string status)
        {
            await Clients.All.SendAsync("InitializationStatus", status);
        }
    }
}