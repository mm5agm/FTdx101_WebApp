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

        public bool IsConnected => _serialPort?.IsOpen ?? false;

        public CatMultiplexerService(ILogger<CatMultiplexerService> logger)
        {
            _logger = logger;
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

                _logger.LogDebug("[{ClientId}] >>> #{RequestId}: {Command}",
                    request.ClientId, request.RequestId, fullCommand.TrimEnd(';'));

                _serialPort.Write(commandBytes, 0, commandBytes.Length);
                await Task.Delay(50);

                // Read response
                var response = await ReadResponseAsync(request);
                request.CompletionSource.SetResult(response);

                _logger.LogDebug("[{ClientId}] <<< #{RequestId}: '{Response}' ({ElapsedMs}ms)",
                    request.ClientId, request.RequestId, response,
                    (DateTime.UtcNow - request.Timestamp).TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ClientId}] Error processing command #{RequestId}",
                    request.ClientId, request.RequestId);
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
                    _serialPort.DtrEnable = false;
                    _serialPort.RtsEnable = false;
                    _serialPort.DiscardInBuffer();
                    _serialPort.DiscardOutBuffer();
                    _serialPort.Close();
                    _logger.LogInformation("Disconnected from serial port");
                }

                _serialPort?.Dispose();
                _serialPort = null;
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
    }
}