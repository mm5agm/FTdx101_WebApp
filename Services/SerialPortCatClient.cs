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
        public Task<string> SendCommandAsync(string command, CancellationToken cancellationToken = default)
        {
            if (_serialPort == null || !_serialPort.IsOpen)
                throw new InvalidOperationException("Serial port is not open.");

            _serialPort.WriteLine(command);
            // Implement response reading logic as needed
            return Task.FromResult(string.Empty);
        }

        // Implement the rest of the ICatClient interface as needed...
        public Task<long> ReadFrequencyAsync() => Task.FromResult(0L);
        public Task<long> ReadFrequencyAAsync() => Task.FromResult(0L);
        public Task<bool> SetFrequencyAAsync(long frequencyHz) => Task.FromResult(true);
        public Task<int> ReadSMeterAsync() => Task.FromResult(0);
        public Task<int> ReadSMeterMainAsync() => Task.FromResult(0);
        public Task<string> ReadModeAsync() => Task.FromResult(string.Empty);
        public Task<string> ReadModeMainAsync() => Task.FromResult(string.Empty);
        public Task<bool> SetModeMainAsync(string mode) => Task.FromResult(true);
        public Task<long> ReadFrequencyBAsync() => Task.FromResult(0L);
        public Task<bool> SetFrequencyBAsync(long frequencyHz) => Task.FromResult(true);
        public Task<int> ReadSMeterSubAsync() => Task.FromResult(0);
        public Task<string> ReadModeSubAsync() => Task.FromResult(string.Empty);
        public Task<bool> SetModeSubAsync(string mode) => Task.FromResult(true);
        public Task<bool> ReadTransmitStatusAsync() => Task.FromResult(false);
    }
}