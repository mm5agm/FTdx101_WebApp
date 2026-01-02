using System.Text.Json;

public class RadioState
{
    public string BandA { get; set; } = "20m";
    public string BandB { get; set; } = "20m";
    public string AntennaA { get; set; } = "1";
    public string AntennaB { get; set; } = "1";
    public Dictionary<string, object> Controls { get; set; } = new();
}

public class RadioStateService
{
    private readonly string _filePath = "radio_state.json";
    private RadioState _state = new();

    public RadioStateService()
    {
        Load();
    }

    public RadioState GetState() => _state;

    public void SetBand(string receiver, string band)
    {
        if (receiver == "A") _state.BandA = band;
        else _state.BandB = band;
        Save();
    }

    public void SetAntenna(string receiver, string antenna)
    {
        if (receiver == "A") _state.AntennaA = antenna;
        else _state.AntennaB = antenna;
        Save();
    }

    public void SetControl(string key, object value)
    {
        _state.Controls[key] = value;
        Save();
    }

    public void Save()
    {
        File.WriteAllText(_filePath, JsonSerializer.Serialize(_state));
    }

    public void Load()
    {
        if (File.Exists(_filePath))
        {
            _state = JsonSerializer.Deserialize<RadioState>(File.ReadAllText(_filePath)) ?? new RadioState();
        }
    }
}