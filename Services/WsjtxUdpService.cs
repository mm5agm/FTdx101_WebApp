using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FTdx101_WebApp.Services
{
    /// <summary>
    /// Listens for WSJT-X UDP status broadcasts and syncs frequency/TX state to the app.
    /// WSJT-X default UDP address: 224.0.0.1:2237 (multicast).
    /// Frequency changes from WSJT-X are also handled via the rigctld TCP connection (port 4532);
    /// this service provides real-time redundancy and exposes TX status to the web UI.
    /// </summary>
    public class WsjtxUdpService : BackgroundService
    {
        private const int WsjtxPort = 2237;
        private const uint MagicNumber = 0xADBCCBDA;
        private const uint MessageTypeHeartbeat = 0;
        private const uint MessageTypeStatus = 1;
        private const uint MessageTypeClose = 6;

        private readonly ICatClient _catClient;
        private readonly RadioStateService _radioStateService;
        private readonly ILogger<WsjtxUdpService> _logger;

        private readonly object _lock = new();
        private DateTime _lastSeen = DateTime.MinValue;
        private bool _isTransmitting;
        private string _wsjtxMode = "";
        private string _wsjtxId = "";

        public bool IsConnected { get { lock (_lock) return (DateTime.UtcNow - _lastSeen).TotalSeconds < 30; } }
        public bool IsTransmitting { get { lock (_lock) return _isTransmitting; } }
        public string WsjtxMode { get { lock (_lock) return _wsjtxMode; } }
        public string WsjtxId { get { lock (_lock) return _wsjtxId; } }

        public WsjtxUdpService(
            ICatClient catClient,
            RadioStateService radioStateService,
            ILogger<WsjtxUdpService> logger)
        {
            _catClient = catClient;
            _radioStateService = radioStateService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            UdpClient? udpClient = null;
            try
            {
                udpClient = CreateUdpListener();
                _logger.LogInformation("WSJT-X UDP listener started on port {Port}", WsjtxPort);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = await udpClient.ReceiveAsync(stoppingToken);
                        await ProcessMessageAsync(result.Buffer, stoppingToken);
                    }
                    catch (OperationCanceledException) { break; }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error processing WSJT-X UDP packet");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WSJT-X UDP service failed to start on port {Port}", WsjtxPort);
            }
            finally
            {
                udpClient?.Close();
                _logger.LogInformation("WSJT-X UDP listener stopped");
            }
        }

        private UdpClient CreateUdpListener()
        {
            var udp = new UdpClient();
            udp.ExclusiveAddressUse = false;
            udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udp.Client.Bind(new IPEndPoint(IPAddress.Any, WsjtxPort));
            try
            {
                udp.JoinMulticastGroup(IPAddress.Parse("224.0.0.1"));
                _logger.LogInformation("Joined WSJT-X multicast group 224.0.0.1");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not join multicast group; will receive unicast packets only");
            }
            return udp;
        }

        private async Task ProcessMessageAsync(byte[] data, CancellationToken ct)
        {
            // Parse synchronously (no spans across await boundaries)
            var msg = TryParseMessage(data);
            if (msg == null) return;

            lock (_lock)
            {
                _lastSeen = DateTime.UtcNow;
                _wsjtxId = msg.Id;
            }

            switch (msg.Type)
            {
                case MessageTypeHeartbeat:
                    _logger.LogDebug("WSJT-X Heartbeat from '{Id}'", msg.Id);
                    break;

                case MessageTypeStatus:
                    lock (_lock)
                    {
                        _isTransmitting = msg.Transmitting;
                        _wsjtxMode = msg.Mode;
                    }
                    _logger.LogDebug("WSJT-X Status: Id={Id}, Freq={Freq}, Mode={Mode}, TX={TX}",
                        msg.Id, msg.DialFrequency, msg.Mode, msg.Transmitting);

                    // Sync frequency to radio if meaningfully different (>100 Hz).
                    // The rigctld TCP connection also handles this when WSJT-X sends F <freq>;
                    // this path provides real-time redundancy.
                    if (msg.DialFrequency > 0 && Math.Abs(msg.DialFrequency - _radioStateService.FrequencyA) > 100)
                    {
                        _logger.LogInformation("WSJT-X frequency → radio: {Freq} Hz", msg.DialFrequency);
                        await _catClient.SetFrequencyAAsync(msg.DialFrequency);
                    }
                    break;

                case MessageTypeClose:
                    _logger.LogInformation("WSJT-X '{Id}' closed", msg.Id);
                    lock (_lock)
                    {
                        _isTransmitting = false;
                        _lastSeen = DateTime.MinValue;
                    }
                    break;
            }
        }

        // --- Message parsing (all synchronous — spans are safe here) ---

        private record ParsedMessage(uint Type, string Id, long DialFrequency, bool Transmitting, string Mode);

        private static ParsedMessage? TryParseMessage(byte[] data)
        {
            if (data.Length < 8) return null;
            try
            {
                var span = data.AsSpan();
                int offset = 0;

                uint magic = ReadUInt32(span, ref offset);
                if (magic != MagicNumber) return null;

                ReadUInt32(span, ref offset); // schema version
                uint type = ReadUInt32(span, ref offset);
                string id = ReadQString(span, ref offset) ?? "";

                long dialFrequency = 0;
                bool transmitting = false;
                string mode = "";

                if (type == MessageTypeStatus)
                {
                    dialFrequency = (long)ReadUInt64(span, ref offset);
                    mode = ReadQString(span, ref offset) ?? "";
                    ReadQString(span, ref offset); // DX Call
                    ReadQString(span, ref offset); // Report
                    ReadQString(span, ref offset); // TX Mode
                    ReadBool(span, ref offset);    // TX Enabled
                    transmitting = ReadBool(span, ref offset);
                }

                return new ParsedMessage(type, id, dialFrequency, transmitting, mode);
            }
            catch
            {
                return null;
            }
        }

        private static uint ReadUInt32(ReadOnlySpan<byte> data, ref int offset)
        {
            var value = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(offset, 4));
            offset += 4;
            return value;
        }

        private static ulong ReadUInt64(ReadOnlySpan<byte> data, ref int offset)
        {
            var value = BinaryPrimitives.ReadUInt64BigEndian(data.Slice(offset, 8));
            offset += 8;
            return value;
        }

        private static bool ReadBool(ReadOnlySpan<byte> data, ref int offset)
        {
            return data[offset++] != 0;
        }

        private static string? ReadQString(ReadOnlySpan<byte> data, ref int offset)
        {
            if (offset + 4 > data.Length) return null;
            uint rawLength = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(offset, 4));
            offset += 4;
            int length = (int)rawLength;
            if (length == -1) return null;       // null Qt string
            if (length == 0) return string.Empty;
            if (offset + length > data.Length) return null;
            var str = Encoding.UTF8.GetString(data.Slice(offset, length));
            offset += length;
            return str;
        }
    }
}
