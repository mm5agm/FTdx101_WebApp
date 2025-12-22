using System.IO.Ports;
using System.Text;

namespace FTdx101_WebApp.Services
{
    public class SerialPortCatClient : ICatClient
    {
        private SerialPort? _serialPort;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly ILogger<SerialPortCatClient> _logger;

        public bool IsConnected => _serialPort?.IsOpen ?? false;

        public SerialPortCatClient(ILogger<SerialPortCatClient> logger)
        {
            _logger = logger;
        }

        public async Task<bool> ConnectAsync(string portName, int baudRate = 38400)
        {
            try
            {
                await _semaphore.WaitAsync();

                if (_serialPort?.IsOpen == true)
                {
                    return true;
                }

                _serialPort = new SerialPort
                {
                    PortName = portName,
                    BaudRate = baudRate,
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.Two,  // 8N2
                    Handshake = Handshake.None,
                    ReadTimeout = 500,
                    WriteTimeout = 500,
                    DtrEnable = true,   // CHANGED: Try enabling DTR (RealTerm might do this)
                    RtsEnable = true    // CHANGED: Try enabling RTS (RealTerm might do this)
                };

                _serialPort.Open();

                // Wait for port to stabilize
                await Task.Delay(200);

                _logger.LogInformation("Connected to {PortName} at {BaudRate} baud, 8-N-2 (DTR/RTS enabled)", portName, baudRate);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to {PortName}", portName);
                return false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task DisconnectAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_serialPort?.IsOpen == true)
                {
                    _serialPort.Close();
                    _logger.LogInformation("Disconnected from serial port");
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<string> SendCommandAsync(string command)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_serialPort?.IsOpen != true)
                {
                    _logger.LogWarning("Attempted to send command but serial port is not open");
                    throw new InvalidOperationException("Serial port is not open");
                }

                // Only clear old data if present
                if (_serialPort.BytesToRead > 0)
                {
                    var oldBytes = _serialPort.BytesToRead;
                    _serialPort.DiscardInBuffer();
                    _logger.LogDebug("Cleared {Bytes} stale bytes from input buffer", oldBytes);
                }

                // Add semicolon if not present
                var fullCommand = command.EndsWith(";") ? command : command + ";";

                _logger.LogInformation(">>> Sending CAT command: {Command}", fullCommand.TrimEnd(';'));

                // Write as bytes explicitly
                var commandBytes = Encoding.ASCII.GetBytes(fullCommand);
                _serialPort.Write(commandBytes, 0, commandBytes.Length);

                // Small delay for radio processing
                await Task.Delay(50);

                // Read response
                var response = await Task.Run(() =>
                {
                    try
                    {
                        var startTime = DateTime.Now;
                        var timeout = TimeSpan.FromMilliseconds(1000);  // Back to 1 second

                        // Wait for response
                        int iterations = 0;
                        while (_serialPort.BytesToRead == 0 && (DateTime.Now - startTime) < timeout)
                        {
                            Thread.Sleep(20);
                            iterations++;
                        }

                        var elapsedMs = (DateTime.Now - startTime).TotalMilliseconds;
                        var bytesAvailable = _serialPort.BytesToRead;

                        if (bytesAvailable > 0)
                        {
                            // Read all data
                            var buffer = new byte[bytesAvailable];
                            _serialPort.Read(buffer, 0, bytesAvailable);
                            var data = Encoding.ASCII.GetString(buffer);

                            _logger.LogInformation("<<< Received ({ElapsedMs}ms, {Iterations} iterations): '{RawResponse}' ({Bytes} bytes)",
                                elapsedMs, iterations, data, bytesAvailable);

                            // Clean response
                            var cleaned = data.Trim(';', '\r', '\n', '?', ' ');
                            return cleaned;
                        }

                        _logger.LogWarning("?? TIMEOUT: No response for {Command} after {ElapsedMs}ms ({Iterations} iterations)",
                            fullCommand.TrimEnd(';'), elapsedMs, iterations);
                        return string.Empty;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Exception reading response");
                        return string.Empty;
                    }
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendCommandAsync");
                return string.Empty;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // [All other methods remain the same...]

        public async Task<long> ReadFrequencyAsync()
        {
            return await ReadFrequencyAAsync();
        }

        public async Task<long> ReadFrequencyAAsync()
        {
            try
            {
                var response = await SendCommandAsync(CatCommands.FrequencyVfoA);
                var freq = CatCommands.ParseFrequency(response);
                _logger.LogDebug("VFO-A Frequency: {Frequency} Hz", freq);
                return freq;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading VFO-A frequency");
            }
            return 0;
        }

        public async Task<long> ReadFrequencyBAsync()
        {
            try
            {
                var response = await SendCommandAsync(CatCommands.FrequencyVfoB);
                var freq = CatCommands.ParseFrequency(response);
                _logger.LogDebug("VFO-B Frequency: {Frequency} Hz", freq);
                return freq;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading VFO-B frequency");
            }
            return 0;
        }

        public async Task<bool> SetFrequencyAAsync(long frequencyHz)
        {
            try
            {
                var command = CatCommands.FormatFrequencyA(frequencyHz);
                await SendCommandAsync(command);
                _logger.LogInformation("Set VFO-A frequency to {Frequency} Hz", frequencyHz);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting VFO-A frequency");
                return false;
            }
        }

        public async Task<bool> SetFrequencyBAsync(long frequencyHz)
        {
            try
            {
                var command = CatCommands.FormatFrequencyB(frequencyHz);
                await SendCommandAsync(command);
                _logger.LogInformation("Set VFO-B frequency to {Frequency} Hz", frequencyHz);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting VFO-B frequency");
                return false;
            }
        }

        public async Task<int> ReadSMeterAsync()
        {
            return await ReadSMeterMainAsync();
        }

        public async Task<int> ReadSMeterMainAsync()
        {
            try
            {
                var response = await SendCommandAsync(CatCommands.SMeterMain);
                var sMeter = CatCommands.ParseSMeter(response);
                _logger.LogDebug("S-Meter Main: {SMeter}", sMeter);
                return sMeter;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading Main S-meter");
            }
            return 0;
        }

        public async Task<int> ReadSMeterSubAsync()
        {
            try
            {
                var response = await SendCommandAsync(CatCommands.SMeterSub);
                var sMeter = CatCommands.ParseSMeter(response);
                _logger.LogDebug("S-Meter Sub: {SMeter}", sMeter);
                return sMeter;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading Sub S-meter");
            }
            return 0;
        }

        public async Task<string> ReadModeAsync()
        {
            return await ReadModeMainAsync();
        }

        public async Task<string> ReadModeMainAsync()
        {
            try
            {
                var response = await SendCommandAsync(CatCommands.ModeMain);
                var mode = CatCommands.ParseMode(response);
                _logger.LogDebug("Mode Main: {Mode}", mode);
                return mode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading Main mode");
            }
            return "UNKNOWN";
        }

        public async Task<string> ReadModeSubAsync()
        {
            try
            {
                var response = await SendCommandAsync(CatCommands.ModeSub);
                var mode = CatCommands.ParseMode(response);
                _logger.LogDebug("Mode Sub: {Mode}", mode);
                return mode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading Sub mode");
            }
            return "UNKNOWN";
        }

        public async Task<bool> ReadTransmitStatusAsync()
        {
            try
            {
                var response = await SendCommandAsync(CatCommands.TransmitStatus);
                var isTx = response.Contains("TX1");
                _logger.LogDebug("Transmit Status: {Status}", isTx ? "TX" : "RX");
                return isTx;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading transmit status");
            }
            return false;
        }

        public void Dispose()
        {
            _serialPort?.Dispose();
            _semaphore?.Dispose();
        }
    }
}