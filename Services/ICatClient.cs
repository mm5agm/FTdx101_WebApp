namespace FTdx101_WebApp.Services
{
    public interface ICatClient : IDisposable
    {
        Task<bool> ConnectAsync(string portName, int baudRate = 38400);
        Task DisconnectAsync();
        bool IsConnected { get; }
        Task<string> SendCommandAsync(string command);

        // VFO-A (Main) Methods
        Task<long> ReadFrequencyAsync();
        Task<long> ReadFrequencyAAsync();
        Task<bool> SetFrequencyAAsync(long frequencyHz);
        Task<int> ReadSMeterAsync();
        Task<int> ReadSMeterMainAsync();
        Task<string> ReadModeAsync();
        Task<string> ReadModeMainAsync();

        // VFO-B (Sub) Methods
        Task<long> ReadFrequencyBAsync();
        Task<bool> SetFrequencyBAsync(long frequencyHz);
        Task<int> ReadSMeterSubAsync();
        Task<string> ReadModeSubAsync();

        // Common Methods
        Task<bool> ReadTransmitStatusAsync();
    }
}