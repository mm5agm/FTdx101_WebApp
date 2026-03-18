using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;

namespace FTdx101_WebApp.Hubs
{
    /// <summary>
    /// SignalR hub for real-time radio state updates
    /// </summary>
    public class RadioHub : Hub
    {
        private readonly ILogger<RadioHub> _logger;
        private readonly IHostApplicationLifetime _lifetime;

        // Track connected clients (thread-safe)
        private static readonly ConcurrentDictionary<string, byte> connections = new();
        // Track last heartbeat per connection
        private static readonly ConcurrentDictionary<string, DateTime> lastHeartbeats = new();
        private static readonly TimeSpan HeartbeatTimeout = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan HeartbeatCheckInterval = TimeSpan.FromSeconds(5);
        private static System.Threading.Timer? heartbeatTimer;

        public RadioHub(ILogger<RadioHub> logger, IHostApplicationLifetime lifetime)
        {
            _logger = logger;
            _lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
            _logger.LogInformation("RadioHub constructed. IHostApplicationLifetime injected: true");
            _logger.LogInformation("RadioHub constructor executed.");
            // Start heartbeat timer only once (static)
            if (heartbeatTimer is null)
            {
                heartbeatTimer = new System.Threading.Timer(CheckHeartbeats, null, HeartbeatCheckInterval, HeartbeatCheckInterval);
            }
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("RadioHub OnConnectedAsync executed. Client connected: {ConnectionId}", Context.ConnectionId);
            connections.TryAdd(Context.ConnectionId, 0);
            lastHeartbeats[Context.ConnectionId] = DateTime.UtcNow;
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("RadioHub OnDisconnectedAsync executed. Client disconnected: {ConnectionId}", Context.ConnectionId);
            connections.TryRemove(Context.ConnectionId, out _);
            lastHeartbeats.TryRemove(Context.ConnectionId, out _);
            await base.OnDisconnectedAsync(exception);

            // If no clients remain, shut down the app
            if (connections.IsEmpty)
            {
                _logger.LogInformation("No clients remain. Attempting shutdown. IHostApplicationLifetime injected: true");
                _logger.LogInformation("RadioHub: Calling StopApplication.");
                _lifetime.StopApplication();
            }
        }

        // Heartbeat method called by client every 5 seconds
        public Task Heartbeat()
        {
            lastHeartbeats[Context.ConnectionId] = DateTime.UtcNow;
            _logger.LogDebug("Heartbeat received from {ConnectionId}", Context.ConnectionId);
            return Task.CompletedTask;
        }

        // Timer callback to check for stale clients
        private void CheckHeartbeats(object? state)
        {
            var now = DateTime.UtcNow;
            foreach (var kvp in lastHeartbeats.ToArray())
            {
                if (now - kvp.Value > HeartbeatTimeout)
                {
                    _logger.LogWarning("Heartbeat timeout for {ConnectionId}. Removing.", kvp.Key);
                    connections.TryRemove(kvp.Key, out _);
                    lastHeartbeats.TryRemove(kvp.Key, out _);
                }
            }
            // If no clients remain, shut down the app
            if (connections.IsEmpty)
            {
                _logger.LogInformation("[Heartbeat] No clients remain. Attempting shutdown. IHostApplicationLifetime injected: true");
                _logger.LogInformation("[Heartbeat] RadioHub: Calling StopApplication.");
                _lifetime.StopApplication();
            }
        }


        public async Task SendInitializationStatus(string status)
        {
            await Clients.All.SendAsync("InitializationStatus", status);
        }
    }
}
    
