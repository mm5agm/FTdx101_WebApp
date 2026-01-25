using System.Collections.Concurrent;
using System.IO.Ports;
using System.Text;

namespace FTdx101_WebApp.Services
{
    /// <summary>
    /// Central CAT multiplexer that owns the serial port and services multiple clients
    /// </summary>
    public class CatMultiplexerService : IDisposable
    {
        private SerialPort? _serialPort;
        private readonly SemaphoreSlim _serialSemaphore = new(1, 1);
        private readonly ILogger<CatMultiplexerService> _logger;
        private readonly ConcurrentQueue<CatRequest> _commandQueue = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private Task? _processingTask;
        private int _nextRequestId = 0;

        // NEW: Auto-Information support
        private readonly CatMessageBuffer _messageBuffer;
        private readonly CatMessageDispatcher _messageDispatcher;
        private bool _autoInformationEnabled = false;

        public bool IsConnected => _serialPort?.IsOpen ?? false;

        public CatMultiplexerService(
            ILogger<CatMultiplexerService> logger,
            CatMessageBuffer messageBuffer,
            CatMessageDispatcher messageDispatcher)
        {
            _logger = logger;
            _messageBuffer = messageBuffer;
            _messageDispatcher = messageDispatcher;

            // Hook up message received event
            _messageBuffer.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(object? sender, CatMessageReceivedEventArgs e)
        {
            _logger.LogDebug("Auto-Info: {Message}", e.Message);
            _messageDispatcher.DispatchMessage(e.Message);
        }

        public async Task<bool> ConnectAsync(string portName, int baudRate = 38400)
        {
            await _serialSemaphore.WaitAsync();
            try
            {
                if (_serialPort?.IsOpen == true)
                {
                    _logger.LogInformation("Port {PortName} already connected", portName);
                    return true;
                }

                var availablePorts = SerialPort.GetPortNames();
                _logger.LogInformation("Available COM ports: {Ports}", string.Join(", ", availablePorts));

                if (!availablePorts.Contains(portName))
                {
                    _logger.LogError("Port {PortName} not found", portName);
                    return false;
                }

                _serialPort = new SerialPort
                {
                    PortName = portName,
                    BaudRate = baudRate,
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.Two,
                    Handshake = Handshake.None,
                    ReadTimeout = 500,
                    WriteTimeout = 500,
                    DtrEnable = true,
                    RtsEnable = true
                };

                _serialPort.Open();
                await Task.Delay(200);

                _logger.LogInformation("✓ Connected to {PortName} at {BaudRate} baud, 8-N-2", portName, baudRate);

                // Start processing queue
                _processingTask = Task.Run(() => ProcessCommandQueueAsync(_cancellationTokenSource.Token));

                _serialPort.DataReceived += (s, e) => {
                    var data = _serialPort.ReadExisting();
                    _messageBuffer.AppendData(data);
                  };

                // --- ADD THIS: Query initial state right after connecting ---
                await QueryInitialStateAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to {PortName}", portName);
                return false;
            }
            finally
            {
                _serialSemaphore.Release();
            }
        }

        /// <summary>
        /// Enable Auto Information mode - radio sends updates automatically
        /// </summary>
        public async Task EnableAutoInformationAsync()
        {
            // Enable Auto Information (AI1;)
            await SendCommandAsync("AI1;", "System");
            // Optionally, send Data Terminal Off (DT0;)
            await SendCommandAsync("DT0;", "System");
        }

        /// <summary>
        /// Disable Auto Information mode
        /// </summary>
        public async Task DisableAutoInformationAsync()
        {
            // Disable Auto Information (AI0;)
            await SendCommandAsync("AI0;", "System");
        }

        /// <summary>
        /// Query all initial radio parameters to populate state
        /// </summary>
        private async Task QueryInitialStateAsync()
        {
            _logger.LogInformation("Querying initial radio state...");

            var commands = new[]
            {
                "FA;",    // VFO A frequency
                "FB;",    // VFO B frequency
                "MD0;",   // VFO A mode
                "MD1;",   // VFO B mode
                "SM0;",   // VFO A S-meter
                "SM1;",   // VFO B S-meter
                "PC;",    // Power
                "AN0;",   // VFO A antenna
                "AN1;",   // VFO B antenna
                "TX;",    // TX status
            };

            foreach (var cmd in commands)
            {
                try
                {
                    var response = await SendCommandAsync(cmd, "InitialQuery");
                    if (!string.IsNullOrEmpty(response))
                    {
                        _messageDispatcher.DispatchMessage(response + ";");
                    }
                    await Task.Delay(50); // Small delay between commands
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to query {Command}", cmd);
                }
            }

            _logger.LogInformation("✓ Initial state queried");
        }

        public async Task<string> SendCommandAsync(string command, string clientId, CancellationToken cancellationToken = default)
        {
            var requestId = Interlocked.Increment(ref _nextRequestId);
            var request = new CatRequest
            {
                RequestId = requestId,
                ClientId = clientId,
                Command = command,
                CompletionSource = new TaskCompletionSource<string>(),
                Timestamp = DateTime.UtcNow
            };

            _commandQueue.Enqueue(request);
            _logger.LogDebug("[{ClientId}] Queued command #{RequestId}: {Command}", clientId, requestId, command.TrimEnd(';'));

            // Wait for response with timeout
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

            try
            {
                return await request.CompletionSource.Task.WaitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[{ClientId}] Command #{RequestId} timed out", clientId, requestId);
                return string.Empty;
            }
        }

        private async Task ProcessCommandQueueAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Command queue processor started");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_commandQueue.TryDequeue(out var request))
                    {
                        await ProcessSingleCommandAsync(request);
                    }
                    else
                    {
                        await Task.Delay(10, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing command queue");
                    await Task.Delay(100, cancellationToken);
                }
            }

            _logger.LogInformation("Command queue processor stopped");
        }

        private async Task ProcessSingleCommandAsync(CatRequest request)
        {
            try
            {
                if (_serialPort?.IsOpen != true)
                {
                    _logger.LogError("[{ClientId}] Command #{RequestId} failed: Serial port not open", request.ClientId, request.RequestId);
                    request.CompletionSource.SetResult(string.Empty);
                    return;
                }

                // Clear stale data
                if (_serialPort.BytesToRead > 0)
                {
                    _serialPort.DiscardInBuffer();
                }

                // Send command
                var fullCommand = request.Command.EndsWith(";") ? request.Command : request.Command + ";";
                var commandBytes = Encoding.ASCII.GetBytes(fullCommand);

                _logger.LogDebug("[{ClientId}] >>> #{RequestId}: {Command}", request.ClientId, request.RequestId, fullCommand.TrimEnd(';'));

                _serialPort.Write(commandBytes, 0, commandBytes.Length);
                await Task.Delay(50);

                // Read response
                var response = await ReadResponseAsync(request);

                if (string.IsNullOrWhiteSpace(response))
                {
                    _logger.LogWarning("[{ClientId}] Command #{RequestId} received empty response for '{Command}'", request.ClientId, request.RequestId, fullCommand.TrimEnd(';'));
                }
                else if (response.Contains("?") || response.Contains("ERROR", StringComparison.OrdinalIgnoreCase))
                {
               }

                request.CompletionSource.SetResult(response);

                _logger.LogDebug("[{ClientId}] <<< #{RequestId}: '{Response}' ({ElapsedMs}ms)",
                    request.ClientId, request.RequestId, response,
                    (DateTime.UtcNow - request.Timestamp).TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ClientId}] Error processing command #{RequestId}: '{Command}'",
                    request.ClientId, request.RequestId, request.Command);
                request.CompletionSource.SetException(ex);
            }
        }

        private async Task<string> ReadResponseAsync(CatRequest request)
        {
            return await Task.Run(() =>
            {
                var startTime = DateTime.Now;
                var timeout = TimeSpan.FromMilliseconds(1000);

                while (_serialPort!.BytesToRead == 0 && (DateTime.Now - startTime) < timeout)
                {
                    Thread.Sleep(20);
                }

                if (_serialPort.BytesToRead > 0)
                {
                    var buffer = new byte[_serialPort.BytesToRead];
                    _serialPort.Read(buffer, 0, buffer.Length);
                    var data = Encoding.ASCII.GetString(buffer);

                    // NEW: Feed all received data to message buffer for AI processing
                    if (_autoInformationEnabled && !string.IsNullOrEmpty(data))
                    {
                        _messageBuffer.AppendData(data);
                    }

                    return data.Trim(';', '\r', '\n', '?', ' ');
                }

                return string.Empty;
            });
        }

        public async Task DisconnectAsync()
        {
            await _serialSemaphore.WaitAsync();
            try
            {
                _cancellationTokenSource.Cancel();

                if (_processingTask != null)
                {
                    await _processingTask;
                }

                if (_serialPort?.IsOpen == true)
                {
                    // Disable AI mode before disconnect
                    if (_autoInformationEnabled)
                    {
                        try
                        {
                            var cmdBytes = Encoding.ASCII.GetBytes("AI0;");
                            _serialPort.Write(cmdBytes, 0, cmdBytes.Length);
                            await Task.Delay(100);
                        }
                        catch { /* Ignore errors during disconnect */ }
                    }

                    _serialPort.DtrEnable = false;
                    _serialPort.RtsEnable = false;
                    _serialPort.DiscardInBuffer();
                    _serialPort.DiscardOutBuffer();
                    _serialPort.Close();
                    _logger.LogInformation("Disconnected from serial port");
                }

                _serialPort?.Dispose();
                _serialPort = null;
                _autoInformationEnabled = false;
            }
            finally
            {
                _serialSemaphore.Release();
            }
        }

        public void Dispose()
        {
            DisconnectAsync().GetAwaiter().GetResult();
            _serialSemaphore?.Dispose();
            _cancellationTokenSource?.Dispose();
        }

        private class CatRequest
        {
            public int RequestId { get; set; }
            public string ClientId { get; set; } = string.Empty;
            public string Command { get; set; } = string.Empty;
            public TaskCompletionSource<string> CompletionSource { get; set; } = null!;
            public DateTime Timestamp { get; set; }
        }

        public async Task SendCommand(string command, bool processResult = false, int delay = 0)
        {
            var response = await SendCommandAsync(command, "InitialValues");
            _logger.LogDebug("Command {Command} got response: {Response}", command, response);
            if (processResult && !string.IsNullOrEmpty(response))
            {
                _messageDispatcher.DispatchMessage(response + ";");
            }
            if (delay > 0)
                await Task.Delay(delay);
        }

        public async Task SendCommandPause(string command, bool processResult = false)
        {
            // Always pause 100ms after sending
            await SendCommand(command, processResult, delay: 100);
        }

        public async Task GetInitialValues()
        {
            // Enable Auto Information mode first
            await SendCommand("AI1;", true);

            // The following is a direct translation of the original command sequence:
            await SendCommand("ID;", true);
            await SendCommand("AG0;", true);
            await SendCommand("AG1;", true);
            await SendCommand("RG0;", true);
            await SendCommand("RG1;", true);
            await SendCommand("FA;", true);
            await SendCommand("FB;", true);
            await SendCommand("FR;", true);
            await SendCommand("FT;", true);
            await SendCommand("SS04;", true);
            await SendCommand("SS14;", true);
            await SendCommand("AO;", true);
            await SendCommand("MG;", true);
            await SendCommand("PL;", true);
            await SendCommand("PR0;", true);
            await SendCommand("PR1;", true);
            await SendCommand("MD0;", true);
            await SendCommand("MD1;", true);
            await SendCommand("VS;", true);
            await SendCommand("KP;", true);
            await SendCommand("PC;", true);
            await SendCommand("RL0;", true);
            await SendCommand("RL1;", true);
            await SendCommand("NR0;", true);
            await SendCommand("NR1;", true);
            await SendCommand("NB0;", true);
            await SendCommand("NB1;", true);
            await SendCommand("NL0;", true);
            await SendCommand("CO00;", true);
            await SendCommand("CO10;", true);
            await SendCommand("CO01;", true);
            await SendCommand("CO11;", true);
            await SendCommand("CO02;", true);
            await SendCommand("CO12;", true);
            await SendCommand("CO03;", true);
            await SendCommand("CO13;", true);
            await SendCommand("CN00;", true);
            await SendCommand("CN10;", true);
            await SendCommand("CT0;", true);
            await SendCommand("CT1;", true);
            await SendCommandPause("EX030203;", true);
            await SendCommandPause("EX030202;", true);
            await SendCommandPause("EX030102;", true);
            await SendCommandPause("EX030103;", true);
            await SendCommandPause("EX040105;", true);
            await SendCommandPause("EX030201;", true);
            await SendCommandPause("EX010111;", true);
            await SendCommandPause("EX010112;", true);
            await SendCommandPause("EX030405;", true);
            await SendCommandPause("EX010111;", true);
            await SendCommandPause("EX010211;", true);
            await SendCommandPause("EX010310;", true);
            await SendCommandPause("EX010413;", true);
            await SendCommandPause("EX010112;", true);
            await SendCommandPause("EX010213;", true);
            await SendCommandPause("EX010312;", true);
            await SendCommandPause("EX010414;", true);
            await SendCommandPause("EX0403021;", false);
            await SendCommand("SH0;", true);
            await SendCommand("SH1;", true);
            await SendCommand("IS0;", true);
            await SendCommand("SS06;", true);
            await SendCommand("IS1;", true);
            await SendCommand("AC;", true);
            await SendCommand("KP;", true);
            await SendCommand("FT;", true);
            await SendCommand("IF;", true);
            await SendCommand("BP00;", true);
            await SendCommand("BP01;", true);
            await SendCommand("BP10;", true);
            await SendCommand("BP11;", true);
            await SendCommand("GT0;", true);
            await SendCommand("GT1;", true);
            await SendCommand("AN0;", true);
            await SendCommand("AN1;", true);
            await SendCommand("PA0;", true);
            await SendCommand("PA1;", true);
            await SendCommand("RF0;", true);
            await SendCommand("RF1;", true);
            await SendCommand("ID;", true);
            await SendCommand("CS;", true);
            await SendCommand("ML0;", true);
            await SendCommand("ML1;", true);
            await SendCommand("BI;", true);
            await SendCommand("MS;", true);
            await SendCommand("KS;", true);
            await SendCommand("SS05;", true);
            await SendCommand("SS15;", true);
            await SendCommand("SS06;", true);
            await SendCommand("SS16;", true);
            await SendCommand("VT0;", true);
            await SendCommand("VX;", true);
            await SendCommand("VG;", true);
            await SendCommand("AV;", true);
            await SendCommand("CF000;", true);
            await SendCommand("CF100;", true);
            await SendCommand("CF001;", true);
            await SendCommand("CF101;", true);
            await SendCommand("BC0;", true);
            await SendCommand("BC1;", true);
            await SendCommand("KR;", true);
            await SendCommand("RA0;", true);
            await SendCommand("RA1;", true);
            await SendCommand("SY;", true);
            await SendCommandPause("VD;", true);
            await SendCommandPause("DT0;", true);
        }

        public async Task InitializeRadioAsync()
        {
            await SendCommandAsync("AI1;","Initialization", CancellationToken.None);
            foreach (var cmd in CatCommands.InitializationCommands)
            {
                await SendCommandAsync(cmd, "Initialization", CancellationToken.None);
            }
            await SendCommandAsync("DT0;","Initialization", CancellationToken.None);
        } 
        public async Task ShutdownRadioAsync()
        {
            // Implementation (if needed) or leave empty
        }
    }
}