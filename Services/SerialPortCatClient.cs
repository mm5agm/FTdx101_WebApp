using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FTdx101_WebApp.Services
{
    public class SerialPortCatClient : ICatClient
    {
        private readonly ILogger<SerialPortCatClient> _logger;
        private readonly CatMessageBuffer _catMessageBuffer;
        private SerialPort? _serialPort;

        public SerialPortCatClient(ILogger<SerialPortCatClient> logger, CatMessageBuffer catMessageBuffer)
        {
            _logger = logger;
            _catMessageBuffer = catMessageBuffer;
        }

        public Task<bool> ConnectAsync(string portName, int baudRate = 38400)
        {
            if (_serialPort != null && _serialPort.IsOpen)
                return Task.FromResult(true);

            _serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.Two);
            _serialPort.DataReceived += SerialPort_DataReceived;
            _serialPort.Open();
            return Task.FromResult(_serialPort.IsOpen);
        }

        private void SerialPort_DataReceived(object? sender, SerialDataReceivedEventArgs e)
        {
            if (_serialPort == null) return;
            try
            {
                var data = _serialPort.ReadExisting();
                // Only log responses starting with FA or FB
                if (data.StartsWith("FA") || data.StartsWith("FB"))
                {
                    _logger.LogWarning("RAW SERIAL (FA/FB): {Data}", data);
                }
                _catMessageBuffer.AppendData(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading from serial port");
            }
        }

        public Task DisconnectAsync()
        {
            if (_serialPort != null)
            {
                _serialPort.DataReceived -= SerialPort_DataReceived;
                _serialPort.Close();
                _serialPort.Dispose();
                _serialPort = null;
            }
            return Task.CompletedTask;
        }

        public bool IsConnected => _serialPort != null && _serialPort.IsOpen;

        public void Dispose() => _serialPort?.Dispose();

        public Task<string> SendCommandAsync(string command, string clientId, CancellationToken cancellationToken = default)
        {
            // clientId is not used for serial, but required by interface
            return SendCommandAsync(command, cancellationToken);
        }

        // Legacy overload for internal use
        public async Task<string> SendCommandAsync(string command, CancellationToken cancellationToken = default)
        {
            if (_serialPort == null || !_serialPort.IsOpen)
                throw new InvalidOperationException("Serial port is not open.");

            // Optionally clear any previous buffer for this command prefix
            // If you have a ClearForCommand method, uncomment the next line:
            // _catMessageBuffer.ClearForCommand(command.Substring(0, 2));

            _serialPort.WriteLine(command);

            // Wait for the response matching the command prefix (e.g., "FA" or "FB")
            var prefix = command.Substring(0, 2);
            var response = await _catMessageBuffer.WaitForResponseAsync(prefix, cancellationToken);

            return response ?? string.Empty;
        }

        // Implement the rest of the ICatClient interface as needed...
        public Task<long> ReadFrequencyAsync() => Task.FromResult(0L);
        public async Task<long> ReadFrequencyAAsync()
        {
            var response = await SendCommandAsync("FA;", CancellationToken.None);
            if (!string.IsNullOrEmpty(response) && response.StartsWith("FA"))
            {
                _logger.LogWarning("Frequency response (A): {Response}", response);
                return CatCommands.ParseFrequency(response);
            }
            return 0L;
        }

        public Task<bool> SetFrequencyAAsync(long frequencyHz) => Task.FromResult(true);
        public Task<int> ReadSMeterAsync() => Task.FromResult(0);
        public Task<int> ReadSMeterMainAsync() => Task.FromResult(0);
        public Task<string> ReadModeAsync() => Task.FromResult(string.Empty);
        public Task<string> ReadModeMainAsync() => Task.FromResult(string.Empty);
        public Task<bool> SetModeMainAsync(string mode) => Task.FromResult(true);
        public async Task<long> ReadFrequencyBAsync()
        {
            var response = await SendCommandAsync("FB;", CancellationToken.None);
            if (!string.IsNullOrEmpty(response) && response.StartsWith("FB"))
            {
                _logger.LogWarning("Frequency response (B): {Response}", response);
                return CatCommands.ParseFrequency(response);
            }
            return 0L;
        }

        public Task<bool> SetFrequencyBAsync(long frequencyHz) => Task.FromResult(true);
        public Task<int> ReadSMeterSubAsync() => Task.FromResult(0);
        public Task<string> ReadModeSubAsync() => Task.FromResult(string.Empty);
        public Task<bool> SetModeSubAsync(string mode) => Task.FromResult(true);
        public Task<bool> ReadTransmitStatusAsync() => Task.FromResult(false);
        public async Task<long> QueryFrequencyAAsync(string clientId, CancellationToken cancellationToken = default)
        {
            // Send "FA;" to the serial port and parse the response
            await SendCommandAsync("FA;", clientId, cancellationToken);
            var response = await _catMessageBuffer.WaitForResponseAsync("FA", cancellationToken);
            if (!string.IsNullOrEmpty(response) && response.StartsWith("FA"))
            {
                var freqStr = response.Substring(2, 9);
                if (long.TryParse(freqStr, out var freq))
                    return freq;
            }
            return 0;
        }

        public async Task<long> QueryFrequencyBAsync(string clientId, CancellationToken cancellationToken = default)
        {
            // Send "FB;" to the serial port and parse the response
            await SendCommandAsync("FB;", clientId, cancellationToken);
            var response = await _catMessageBuffer.WaitForResponseAsync("FB", cancellationToken);
            if (!string.IsNullOrEmpty(response) && response.StartsWith("FB"))
            {
                var freqStr = response.Substring(2, 9);
                if (long.TryParse(freqStr, out var freq))
                    return freq;
            }
            return 0;
        }
    }
  
}