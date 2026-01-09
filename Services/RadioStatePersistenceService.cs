using System.IO;
using System.Text.Json;

namespace FTdx101_WebApp.Services
{
    public class RadioStatePersistenceService
    {
        private readonly string _filePath = "radio_state.json";
        private readonly ILogger<RadioStatePersistenceService> _logger;

        public RadioStatePersistenceService(ILogger<RadioStatePersistenceService> logger)
        {
            _logger = logger;
        }

        public RadioState Load()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    _logger.LogInformation("Radio state file not found. Creating default state.");
                    var defaultState = CreateDefaultState();
                    Save(defaultState);
                    return defaultState;
                }

                var json = File.ReadAllText(_filePath);
                var state = JsonSerializer.Deserialize<RadioState>(json) ?? CreateDefaultState();
                _logger.LogInformation("Radio state loaded from {FilePath}", _filePath);
                return state;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading radio state. Using defaults.");
                return CreateDefaultState();
            }
        }

        public void Save(RadioState state)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(state, options);
                File.WriteAllText(_filePath, json);
                _logger.LogDebug("Radio state saved to {FilePath}", _filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving radio state to {FilePath}", _filePath);
            }
        }

        private RadioState CreateDefaultState()
        {
            return new RadioState
            {
                FrequencyA = 14074000, // 14.074 MHz (FT8)
                BandA = "20m",
                ModeA = "USB",
                AntennaA = "1",
                FrequencyB = 14074000,
                BandB = "20m",
                ModeB = "USB",
                AntennaB = "1"
            };
        }
    }
}