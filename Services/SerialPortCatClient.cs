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

                var commandBytes = Encoding.ASCII.GetBytes(command + ";");
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

        public async Task<int> ReadSMeterAsync()
        {
            try
            {
                var response = await SendCommandAsync("SM0");
                if (response.StartsWith("SM0"))
                {
                    var value = response.Substring(3);
                    if (int.TryParse(value, out var sMeter))
                    {
                        return sMeter;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading S-meter");
            }
            return 0;
        }

        public async Task<long> ReadFrequencyAsync()
        {
            try
            {
                var response = await SendCommandAsync("FA");
                if (response.StartsWith("FA"))
                {
                    var value = response.Substring(2);
                    if (long.TryParse(value, out var frequency))
                    {
                        return frequency;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading frequency");
            }
            return 0;
        }

        public async Task<string> ReadModeAsync()
        {
            try
            {
                var response = await SendCommandAsync("MD0");
                if (response.StartsWith("MD0"))
                {
                    var modeCode = response.Substring(3);
                    return modeCode switch
                    {
                        "1" => "LSB",
                        "2" => "USB",
                        "3" => "CW",
                        "4" => "FM",
                        "5" => "AM",
                        "6" => "RTTY-LSB",
                        "7" => "CW-R",
                        "8" => "DATA-LSB",
                        "9" => "RTTY-USB",
                        "A" => "DATA-FM",
                        "B" => "FM-N",
                        "C" => "DATA-USB",
                        "D" => "AM-N",
                        "E" => "C4FM",
                        _ => "UNKNOWN"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading mode");
            }
            return "UNKNOWN";
        }

        public async Task<bool> ReadTransmitStatusAsync()
        {
            try
            {
                var response = await SendCommandAsync("TX");
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