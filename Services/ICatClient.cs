namespace FTdx101MP_WebApp.Services
{
    public interface ICatClient : IDisposable
    {
        Task<bool> ConnectAsync(string portName, int baudRate = 38400);
        Task DisconnectAsync();
        bool IsConnected { get; }
        Task<string> SendCommandAsync(string command);
        Task<int> ReadSMeterAsync();
        Task<long> ReadFrequencyAsync();
        Task<string> ReadModeAsync();
        Task<bool> ReadTransmitStatusAsync();
    }
}