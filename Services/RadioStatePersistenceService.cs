using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace FTdx101_WebApp.Services
{
    public class RadioStatePersistenceService
    {
        private readonly string _filePath;
        private readonly ILogger<RadioStatePersistenceService> _logger;
        private static readonly object _fileLock = new();

        public RadioStatePersistenceService(
            ILogger<RadioStatePersistenceService> logger,
            IWebHostEnvironment env)
        {
            _logger = logger;

            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var stateDir = Path.Combine(appDataPath, "MM5AGM", "FTdx101 WebApp");
            Directory.CreateDirectory(stateDir);
            _filePath = Path.Combine(stateDir, "radio_state.json");
        }

        public RadioState Load()
        {
            try
            {
                lock (_fileLock)
                {
                    if (!File.Exists(_filePath))
                    {
                        _logger.LogInformation("Radio state file not found. Creating default state.");
                        var defaultState = CreateDefaultState();
                        Save(defaultState);
                        return defaultState;
                    }

                    var json = File.ReadAllText(_filePath);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = null // Use PascalCase (default)
                    };
                    var state = JsonSerializer.Deserialize<RadioState>(json, options) ?? CreateDefaultState();
                    _logger.LogInformation("Radio state loaded from {FilePath}", _filePath);
                    _logger.LogInformation("Loaded state: ModeA={ModeA}, ModeB={ModeB}, PowerA={PowerA}, PowerB={PowerB}, AntennaA={AntennaA}, AntennaB={AntennaB}",
                        state.ModeA, state.ModeB, state.PowerA, state.PowerB, state.AntennaA, state.AntennaB);
                    return state;
                }
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
                lock (_fileLock)
                {
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        PropertyNamingPolicy = null // Use PascalCase (default)
                    };

                    var json = JsonSerializer.Serialize(state, options);
                    File.WriteAllText(_filePath, json);
                    _logger.LogDebug("Radio state saved to {FilePath}", _filePath);
                }
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