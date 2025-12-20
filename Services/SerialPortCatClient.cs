using System.IO.Ports;
using System.Text;

namespace FTdx101MP_WebApp.Services
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
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    ReadTimeout = 1000,
                    WriteTimeout = 1000
                };

                _serialPort.Open();
                _logger.LogInformation("Connected to {PortName} at {BaudRate} baud", portName, baudRate);
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
                    throw new InvalidOperationException("Serial port is not open");
                }

                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();

                // Add semicolon if not present
                var fullCommand = command.EndsWith(";") ? command : command + ";";
                var commandBytes = Encoding.ASCII.GetBytes(fullCommand);
                await _serialPort.BaseStream.WriteAsync(commandBytes);

                await Task.Delay(50);

                var buffer = new byte[256];
                var bytesRead = await _serialPort.BaseStream.ReadAsync(buffer);
                var response = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim(';', '\r', '\n');

                return response;
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("Timeout reading response for command: {Command}", command);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending command: {Command}", command);
                return string.Empty;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // VFO-A Frequency (Main)
        public async Task<long> ReadFrequencyAsync()
        {
            return await ReadFrequencyAAsync();
        }

        public async Task<long> ReadFrequencyAAsync()
        {
            try
            {
                var response = await SendCommandAsync(CatCommands.FrequencyVfoA);
                return CatCommands.ParseFrequency(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading VFO-A frequency");
            }
            return 0;
        }

        // VFO-B Frequency (Sub)
        public async Task<long> ReadFrequencyBAsync()
        {
            try
            {
                var response = await SendCommandAsync(CatCommands.FrequencyVfoB);
                return CatCommands.ParseFrequency(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading VFO-B frequency");
            }
            return 0;
        }

        // Set VFO-A Frequency
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

        // Set VFO-B Frequency
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

        // S-Meter Main (VFO-A)
        public async Task<int> ReadSMeterAsync()
        {
            return await ReadSMeterMainAsync();
        }

        public async Task<int> ReadSMeterMainAsync()
        {
            try
            {
                var response = await SendCommandAsync(CatCommands.SMeterMain);
                return CatCommands.ParseSMeter(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading Main S-meter");
            }
            return 0;
        }

        // S-Meter Sub (VFO-B)
        public async Task<int> ReadSMeterSubAsync()
        {
            try
            {
                var response = await SendCommandAsync(CatCommands.SMeterSub);
                return CatCommands.ParseSMeter(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading Sub S-meter");
            }
            return 0;
        }

        // Mode Main (VFO-A)
        public async Task<string> ReadModeAsync()
        {
            return await ReadModeMainAsync();
        }

        public async Task<string> ReadModeMainAsync()
        {
            try
            {
                var response = await SendCommandAsync(CatCommands.ModeMain);
                return CatCommands.ParseMode(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading Main mode");
            }
            return "UNKNOWN";
        }

        // Mode Sub (VFO-B)
        public async Task<string> ReadModeSubAsync()
        {
            try
            {
                var response = await SendCommandAsync(CatCommands.ModeSub);
                return CatCommands.ParseMode(response);
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
                return response.Contains("TX1");
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