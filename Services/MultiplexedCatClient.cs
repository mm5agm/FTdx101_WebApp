namespace FTdx101_WebApp.Services
{
    /// <summary>
    /// ICatClient implementation that uses the multiplexer instead of direct serial access
    /// </summary>
    public class MultiplexedCatClient : ICatClient
    {
        private readonly CatMultiplexerService _multiplexer;
        private readonly ILogger<MultiplexedCatClient> _logger;
        private const string ClientId = "WebUI";

        public bool IsConnected => _multiplexer.IsConnected;

        public MultiplexedCatClient(CatMultiplexerService multiplexer, ILogger<MultiplexedCatClient> logger)
        {
            _multiplexer = multiplexer;
            _logger = logger;
        }

        public Task<bool> ConnectAsync(string portName, int baudRate = 38400)
        {
            // Connection is managed by multiplexer
            return Task.FromResult(_multiplexer.IsConnected);
        }

        public Task DisconnectAsync()
        {
            // Don't disconnect the multiplexer from individual client
            return Task.CompletedTask;
        }

        public async Task<string> SendCommandAsync(string command)
        {
            return await _multiplexer.SendCommandAsync(command, ClientId);
        }

        public async Task<long> ReadFrequencyAsync() => await ReadFrequencyAAsync();

        public async Task<long> ReadFrequencyAAsync()
        {
            var response = await SendCommandAsync(CatCommands.FrequencyVfoA);
            return CatCommands.ParseFrequency(response);
        }

        public async Task<long> ReadFrequencyBAsync()
        {
            var response = await SendCommandAsync(CatCommands.FrequencyVfoB);
            return CatCommands.ParseFrequency(response);
        }

        public async Task<bool> SetFrequencyAAsync(long frequencyHz)
        {
            await SendCommandAsync(CatCommands.FormatFrequencyA(frequencyHz));
            return true;
        }

        public async Task<bool> SetFrequencyBAsync(long frequencyHz)
        {
            await SendCommandAsync(CatCommands.FormatFrequencyB(frequencyHz));
            return true;
        }

        public async Task<int> ReadSMeterAsync() => await ReadSMeterMainAsync();

        public async Task<int> ReadSMeterMainAsync()
        {
            var response = await SendCommandAsync(CatCommands.SMeterMain);
            return CatCommands.ParseSMeter(response);
        }

        public async Task<int> ReadSMeterSubAsync()
        {
            var response = await SendCommandAsync(CatCommands.SMeterSub);
            return CatCommands.ParseSMeter(response);
        }

        public async Task<string> ReadModeAsync() => await ReadModeMainAsync();

        public async Task<string> ReadModeMainAsync()
        {
            var response = await SendCommandAsync(CatCommands.ModeMain);
            return CatCommands.ParseMode(response);
        }

        public async Task<string> ReadModeSubAsync()
        {
            var response = await SendCommandAsync(CatCommands.ModeSub);
            return CatCommands.ParseMode(response);
        }

        public async Task<bool> SetModeMainAsync(string mode)
        {
            await SendCommandAsync(CatCommands.FormatMode(mode, false));
            return true;
        }

        public async Task<bool> SetModeSubAsync(string mode)
        {
            await SendCommandAsync(CatCommands.FormatMode(mode, true));
            return true;
        }

        public async Task<bool> ReadTransmitStatusAsync()
        {
            var response = await SendCommandAsync(CatCommands.TransmitStatus);
            return response.Contains("TX1");
        }

        public void Dispose()
        {
            // Multiplexer handles disposal
        }
    }
}