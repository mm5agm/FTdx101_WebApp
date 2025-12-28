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

        // State for advanced commands
        private string _currentVfo = "VFOA";
        private bool _splitEnabled = false;
        private long _splitFrequency = 0;
        private int _ritOffset = 0;
        private int _xitOffset = 0;

        private static readonly HashSet<string> SupportedModes = new()
        {
            "LSB", "USB", "CW", "CW-R", "RTTY", "RTTY-R", "AM", "FM"
        };

        private const long MinFrequency = 30000;
        private const long MaxFrequency = 75000000;

        // Band mapping for set_band support (using BSxx; command codes)
        private static readonly Dictionary<string, string> BandCodes = new()
        {
            { "160m", "00" },
            { "80m",  "01" },
            { "60m",  "02" },
            { "40m",  "03" },
            { "30m",  "04" },
            { "20m",  "05" },
            { "17m",  "06" },
            { "15m",  "07" },
            { "12m",  "08" },
            { "10m",  "09" },
            { "6m",   "10" },
            { "4m",   "11" } // Added 4m band
        };

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
            // Gracefully handle Hamlib meta-commands (backslash)
            if (command.StartsWith("\\")) return "";

            var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "RPRT 0"; // OK

            switch (parts[0].ToLower())
            {
                // Frequency
                case "f": return await GetFrequencyAsync(clientId);
                case "F": return parts.Length > 1 ? await SetFrequencyAsync(parts[1], clientId) : "RPRT -1";
                // Mode
                case "m": return await GetModeAsync(clientId);
                case "M": return parts.Length > 1 ? await SetModeAsync(parts[1], parts.Length > 2 ? parts[2] : null, clientId) : "RPRT -1";
                // VFO
                case "v": return GetVfo();
                case "V": return parts.Length > 1 ? SetVfo(parts[1]) : "RPRT -1";
                // PTT
                case "t": return await GetPttAsync(clientId);
                case "T": return parts.Length > 1 ? await SetPttAsync(parts[1], clientId) : "RPRT -1";
                // Level (S-meter, AF, RF, SQL, NR, NB, AGC, etc.)
                case "l": return parts.Length > 1 ? await GetLevelAsync(parts[1], clientId) : "RPRT -1";
                case "L": return parts.Length > 2 ? SetLevel(parts[1], parts[2]) : "RPRT -1";
                // Split
                case "x": return GetSplit();
                case "X": return parts.Length > 1 ? SetSplit(parts[1]) : "RPRT -1";
                // Split frequency
                case "z": return GetSplitFrequency();
                case "Z": return parts.Length > 1 ? SetSplitFrequency(parts[1]) : "RPRT -1";
                // RIT/XIT
                case "r": return GetRit();
                case "R": return parts.Length > 1 ? SetRit(parts[1]) : "RPRT -1";
                case "c": return GetXit();
                case "C": return parts.Length > 1 ? SetXit(parts[1]) : "RPRT -1";
                // Info
                case "get_info": return GetInfo();
                // Band change support (use BSxx; command)
                case "set_band": return parts.Length > 1 ? await SetBandAsync(parts[1], clientId) : "RPRT -1";
                // Power, attenuator, preamp, AGC, etc. (stubs)
                case "get_powerstat": return "1"; // Always on
                case "chk_vfo": return "RPRT 0";
                case "dump_state": return ""; // For Hamlib meta-command
                // Memory, band, filter, etc. (stubs)
                case "get_mem": return "RPRT -1";
                case "set_mem": return "RPRT -1";
                case "get_band": return "RPRT -1";
                case "get_filter": return "RPRT -1";
                case "set_filter": return "RPRT -1";
                // Quit
                case "q":
                case "quit": return "RPRT 0";
                // Add more Hamlib commands as needed
                default: return "RPRT -1";
            }
        }

        // --- Command Implementations and Stubs ---

        private async Task<string> GetFrequencyAsync(string clientId)
        {
            var response = await _multiplexer.SendCommandAsync("FA", clientId);
            var freq = CatCommands.ParseFrequency(response);
            return freq > 0 ? freq.ToString() : "RPRT -1";
        }

        private async Task<string> SetFrequencyAsync(string freqStr, string clientId)
        {
            if (long.TryParse(freqStr, out var freq))
            {
                if (freq < MinFrequency || freq > MaxFrequency)
                    return "RPRT -1 // E_RANGE: Value out of valid range for this rig.";
                var command = CatCommands.FormatFrequencyA(freq);
                await _multiplexer.SendCommandAsync(command, clientId);
                return "RPRT 0";
            }
            return "RPRT -1 // E_NOTIMPL: Invalid frequency format.";
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
                "CW-R" => "CW-R",
                "FM" => "FM",
                "AM" => "AM",
                "RTTY" => "RTTY",
                "RTTY-R" => "RTTY-R",
                "DATA-USB" or "DATA-LSB" => "PKTUSB",
                _ => "USB"
            };

            return $"{hamlibMode}\n{bandwidth}";
        }

        private async Task<string> SetModeAsync(string mode, string? passband, string clientId)
        {
            if (!SupportedModes.Contains(mode.ToUpper()))
                return "RPRT -1 // E_MODE: Unsupported mode for this rig.";

            var yaesuMode = mode.ToUpper();
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
            var command = ptt == "1" ? "TX1;" : "TX0;";
            await _multiplexer.SendCommandAsync(command, clientId);
            return "RPRT 0";
        }

        private string GetVfo()
        {
            return _currentVfo;
        }

        private string SetVfo(string vfo)
        {
            if (vfo.Equals("VFOA", StringComparison.OrdinalIgnoreCase) ||
                vfo.Equals("VFOB", StringComparison.OrdinalIgnoreCase))
            {
                _currentVfo = vfo.ToUpper();
                return "RPRT 0";
            }
            return "RPRT -1";
        }

        private string GetSplit() => _splitEnabled ? "1" : "0";
        private string SetSplit(string enabled)
        {
            _splitEnabled = enabled == "1";
            return "RPRT 0";
        }

        private string GetSplitFrequency() => _splitFrequency.ToString();
        private string SetSplitFrequency(string freqStr)
        {
            if (long.TryParse(freqStr, out var freq) && freq >= MinFrequency && freq <= MaxFrequency)
            {
                _splitFrequency = freq;
                return "RPRT 0";
            }
            return "RPRT -1";
        }

        private string GetRit() => _ritOffset.ToString();
        private string SetRit(string offsetStr)
        {
            if (int.TryParse(offsetStr, out var offset) && offset >= -9990 && offset <= 9990)
            {
                _ritOffset = offset;
                return "RPRT 0";
            }
            return "RPRT -1";
        }

        private string GetXit() => _xitOffset.ToString();
        private string SetXit(string offsetStr)
        {
            if (int.TryParse(offsetStr, out var offset) && offset >= -9990 && offset <= 9990)
            {
                _xitOffset = offset;
                return "RPRT 0";
            }
            return "RPRT -1";
        }

        private async Task<string> GetLevelAsync(string level, string clientId)
        {
            if (level.ToUpper() == "STRENGTH")
                return await GetSignalStrengthAsync(clientId);
            // Add more levels as needed
            return "0";
        }

        private string SetLevel(string level, string value)
        {
            // Stub: accept but do nothing
            return "RPRT 0";
        }

        private async Task<string> GetSignalStrengthAsync(string clientId)
        {
            var response = await _multiplexer.SendCommandAsync("SM0", clientId);
            var sMeter = CatCommands.ParseSMeter(response);
            var dbm = (sMeter / 255.0) * 60 - 60;
            return $"{dbm:F0}";
        }

        private string GetInfo()
        {
            // Example info string for FTdx101MP (Hamlib expects: "mfg;model;version;serial;id")
            return "Yaesu;FTDX101MP;1.0.0;000000;3115";
        }

        // --- Band change support using BSxx; command ---
        private async Task<string> SetBandAsync(string band, string clientId)
        {
            // Accept band as "20m", "14", "14074000", etc.
            if (BandCodes.TryGetValue(band.ToLower(), out var code))
            {
                await _multiplexer.SendCommandAsync($"BS{code};", clientId);
                return "RPRT 0";
            }
            // Try parsing as frequency in Hz
            if (long.TryParse(band, out var freqHz) && freqHz >= MinFrequency && freqHz <= MaxFrequency)
            {
                var command = CatCommands.FormatFrequencyA(freqHz);
                await _multiplexer.SendCommandAsync(command, clientId);
                return "RPRT 0";
            }
            return "RPRT -1";
        }
    }
}