using System;
using System.Threading;
using System.Threading.Tasks;

namespace FTdx101_WebApp.Services
{
    public interface ICatClient : IDisposable
    {
        Task<bool> ConnectAsync(string portName, int baudRate = 38400);
        Task DisconnectAsync();
        bool IsConnected { get; }
        Task<string> SendCommandAsync(string command, string clientId, CancellationToken cancellationToken = default);

        // VFO-A (Main) Methods
        Task<long> ReadFrequencyAsync();
        Task<long> ReadFrequencyAAsync();
        Task<bool> SetFrequencyAAsync(long frequencyHz);
        Task<int> ReadSMeterAsync();
        Task<int> ReadSMeterMainAsync();
        Task<string> ReadModeAsync();
        Task<string> ReadModeMainAsync();
        Task<bool> SetModeMainAsync(string mode);

        // VFO-B (Sub) Methods
        Task<long> ReadFrequencyBAsync();
        Task<bool> SetFrequencyBAsync(long frequencyHz);
        Task<int> ReadSMeterSubAsync();
        Task<string> ReadModeSubAsync();
        Task<bool> SetModeSubAsync(string mode);

        // Common Methods
        Task<bool> ReadTransmitStatusAsync();
    }
}