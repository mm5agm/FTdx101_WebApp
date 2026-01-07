using System.IO;
using System.Text.Json;

namespace FTdx101_WebApp.Services
{
    public class RadioStatePersistenceService
    {
        private readonly string _filePath = "radio_state.json";

        public RadioState Load()
        {
            if (!File.Exists(_filePath)) return new RadioState();
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<RadioState>(json) ?? new RadioState();
        }

        public void Save(RadioState state)
        {
            var json = JsonSerializer.Serialize(state);
            File.WriteAllText(_filePath, json);
        }
    }
}