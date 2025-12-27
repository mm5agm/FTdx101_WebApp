using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FTdx101_WebApp.Services
{
    /// <summary>
    /// Implements a rigctld-compatible TCP server for WSJT-X and other Hamlib clients
    /// </summary>
    public class RigctldServer : BackgroundService
    {
        private readonly CatMultiplexerService _multiplexer;
        private readonly ILogger<RigctldServer> _logger;
        private TcpListener? _listener;
        private readonly List<TcpClient> _clients = new();
        private readonly object _clientsLock = new();
        private const int RigctldPort = 4532;

        public RigctldServer(CatMultiplexerService multiplexer, ILogger<RigctldServer> logger)
        {
            _multiplexer = multiplexer;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, RigctldPort);
                _listener.Start();
                _logger.LogInformation("✓ rigctld server listening on port {Port}", RigctldPort);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var client = await _listener.AcceptTcpClientAsync(stoppingToken);
                    var clientEndpoint = client.Client.RemoteEndPoint?.ToString() ?? "unknown";
                    _logger.LogInformation("rigctld client connected: {Endpoint}", clientEndpoint);

                    lock (_clientsLock)
                    {
                        _clients.Add(client);
                    }

                    _ = Task.Run(() => HandleClientAsync(client, stoppingToken), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "rigctld server error");
            }
            finally
            {
                _listener?.Stop();
                _logger.LogInformation("rigctld server stopped");
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            var clientId = $"rigctld-{client.Client.RemoteEndPoint}";
            var stream = client.GetStream();
            var buffer = new byte[1024];

            try
            {
                while (!cancellationToken.IsCancellationRequested && client.Connected)
                {
                    var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
                    if (bytesRead == 0) break;

                    var command = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                    _logger.LogDebug("[{ClientId}] Received: {Command}", clientId, command);

                    var response = await ProcessRigctldCommandAsync(command, clientId);

                    if (!string.IsNullOrEmpty(response))
                    {
                        var responseBytes = Encoding.ASCII.GetBytes(response + "\n");
                        await stream.WriteAsync(responseBytes, cancellationToken);
                        _logger.LogDebug("[{ClientId}] Sent: {Response}", clientId, response.TrimEnd());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ClientId}] Client handler error", clientId);
            }
            finally
            {
                lock (_clientsLock)
                {
                    _clients.Remove(client);
                }
                client.Close();
                _logger.LogInformation("[{ClientId}] Client disconnected", clientId);
            }
        }

        private async Task<string> ProcessRigctldCommandAsync(string command, string clientId)
        {
            // Parse rigctld commands (Hamlib protocol)
            var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "RPRT 0"; // OK

            return parts[0].ToLower() switch
            {
                "f" or "get_freq" => await GetFrequencyAsync(clientId),
                "F" or "set_freq" when parts.Length > 1 => await SetFrequencyAsync(parts[1], clientId),
                "m" or "get_mode" => await GetModeAsync(clientId),
                "M" or "set_mode" when parts.Length > 1 => await SetModeAsync(parts[1], clientId),
                "t" or "get_ptt" => await GetPttAsync(clientId),
                "T" or "set_ptt" when parts.Length > 1 => await SetPttAsync(parts[1], clientId),
                "l" or "get_level" when parts.Length > 1 && parts[1] == "STRENGTH" => await GetSignalStrengthAsync(clientId),
                "q" or "quit" => "RPRT 0", // Client will close connection
                _ => await ForwardRawCommandAsync(command, clientId)
            };
        }

        private async Task<string> GetFrequencyAsync(string clientId)
        {
            var response = await _multiplexer.SendCommandAsync("FA", clientId);
            var freq = CatCommands.ParseFrequency(response);
            return freq > 0 ? freq.ToString() : "RPRT -1"; // Error
        }

        private async Task<string> SetFrequencyAsync(string freqStr, string clientId)
        {
            if (long.TryParse(freqStr, out var freq))
            {
                var command = CatCommands.FormatFrequencyA(freq);
                await _multiplexer.SendCommandAsync(command, clientId);
                return "RPRT 0"; // OK
            }
            return "RPRT -1"; // Error
        }

        private async Task<string> GetModeAsync(string clientId)
        {
            var response = await _multiplexer.SendCommandAsync("MD0", clientId);
            var mode = CatCommands.ParseMode(response);
            var bandwidth = 0; // FT-dx101 manages bandwidth automatically

            // Convert to Hamlib mode names
            var hamlibMode = mode switch
            {
                "USB" => "USB",
                "LSB" => "LSB",
                "CW" => "CW",
                "FM" => "FM",
                "AM" => "AM",
                "DATA-USB" or "DATA-LSB" => "PKTUSB",
                "RTTY-USB" or "RTTY-LSB" => "RTTY",
                _ => "USB"
            };

            return $"{hamlibMode}\n{bandwidth}";
        }

        private async Task<string> SetModeAsync(string mode, string clientId)
        {
            // Convert from Hamlib mode names
            var yaesuMode = mode.ToUpper() switch
            {
                "USB" => "USB",
                "LSB" => "LSB",
                "CW" => "CW",
                "FM" => "FM",
                "AM" => "AM",
                "PKTUSB" or "PKTLSB" => "DATA-USB",
                "RTTY" => "RTTY-USB",
                _ => "USB"
            };

            var command = CatCommands.FormatMode(yaesuMode, false);
            await _multiplexer.SendCommandAsync(command, clientId);
            return "RPRT 0";
        }

        private async Task<string> GetPttAsync(string clientId)
        {
            var response = await _multiplexer.SendCommandAsync("TX", clientId);
            return response.Contains("TX1") ? "1" : "0";
        }

        private async Task<string> SetPttAsync(string ptt, string clientId)
        {
            // PTT control - FT-dx101 uses TX0 (RX) or TX1 (TX)
            var command = ptt == "1" ? "TX1;" : "TX0;";
            await _multiplexer.SendCommandAsync(command, clientId);
            return "RPRT 0";
        }

        private async Task<string> GetSignalStrengthAsync(string clientId)
        {
            var response = await _multiplexer.SendCommandAsync("SM0", clientId);
            var sMeter = CatCommands.ParseSMeter(response);
            // Convert to dBm (-60 to 0 dBm scale for rigctld)
            var dbm = (sMeter / 255.0) * 60 - 60;
            return $"{dbm:F0}";
        }

        private async Task<string> ForwardRawCommandAsync(string command, string clientId)
        {
            // For unsupported commands, try forwarding raw CAT command
            _logger.LogWarning("[{ClientId}] Unsupported rigctld command: {Command}", clientId, command);
            return "RPRT -1"; // Not implemented
        }
    }
}