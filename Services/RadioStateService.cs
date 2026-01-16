using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR;
using FTdx101_WebApp.Hubs;

namespace FTdx101_WebApp.Services
{
    public class RadioStateService : INotifyPropertyChanged
    {
        private readonly ILogger<RadioStateService> _logger;
        private readonly IHubContext<RadioHub>? _hubContext;
        
        // Frequency properties with change notification
        private long _frequencyA;
        private long _frequencyB;
        
        public long FrequencyA
        {
            get => _frequencyA;
            set
            {
                if (_frequencyA != value)
                {
                    _frequencyA = value;
                    OnPropertyChanged();
                    BroadcastUpdate("frequencyA", value);
                    _logger.LogDebug("FrequencyA updated: {Freq} Hz", value);
                }
            }
        }

        public long FrequencyB
        {
            get => _frequencyB;
            set
            {
                if (_frequencyB != value)
                {
                    _frequencyB = value;
                    OnPropertyChanged();
                    BroadcastUpdate("frequencyB", value);
                }
            }
        }

        // Mode properties
        private string _modeA = "USB";
        private string _modeB = "USB";
        
        public string ModeA
        {
            get => _modeA;
            set
            {
                if (_modeA != value)
                {
                    _modeA = value;
                    OnPropertyChanged();
                    BroadcastUpdate("modeA", value);
                }
            }
        }

        public string ModeB
        {
            get => _modeB;
            set
            {
                if (_modeB != value)
                {
                    _modeB = value;
                    OnPropertyChanged();
                    BroadcastUpdate("modeB", value);
                }
            }
        }

        // S-Meter properties
        private int _sMeterA;
        private int _sMeterB;
        
        public int SMeterA
        {
            get => _sMeterA;
            set
            {
                if (_sMeterA != value)
                {
                    _sMeterA = value;
                    OnPropertyChanged();
                    BroadcastUpdate("sMeterA", value);  // REAL-TIME PUSH!
                }
            }
        }

        public int SMeterB
        {
            get => _sMeterB;
            set
            {
                if (_sMeterB != value)
                {
                    _sMeterB = value;
                    OnPropertyChanged();
                    BroadcastUpdate("sMeterB", value);  // REAL-TIME PUSH!
                }
            }
        }

        // Power property
        private int _power = 100;
        
        public int Power
        {
            get => _power;
            set
            {
                if (_power != value)
                {
                    _power = value;
                    OnPropertyChanged();
                    BroadcastUpdate("power", value);
                }
            }
        }

        // Antenna properties
        private string _antennaA = "1";
        private string _antennaB = "1";
        
        public string AntennaA
        {
            get => _antennaA;
            set
            {
                if (_antennaA != value)
                {
                    _antennaA = value;
                    OnPropertyChanged();
                    BroadcastUpdate("antennaA", value);
                }
            }
        }

        public string AntennaB
        {
            get => _antennaB;
            set
            {
                if (_antennaB != value)
                {
                    _antennaB = value;
                    OnPropertyChanged();
                    BroadcastUpdate("antennaB", value);
                }
            }
        }

        // Transmit status
        private bool _isTransmitting;
        
        public bool IsTransmitting
        {
            get => _isTransmitting;
            set
            {
                if (_isTransmitting != value)
                {
                    _isTransmitting = value;
                    OnPropertyChanged();
                    BroadcastUpdate("isTransmitting", value);
                    _logger.LogInformation("TX Status: {Status}", value ? "TRANSMITTING" : "RECEIVING");
                }
            }
        }

        // Keep existing band tracking (non-reactive for now)
        public string BandA { get; private set; } = "20m";
        public string BandB { get; private set; } = "20m";
        public Dictionary<string, object> Controls { get; } = new();

        public RadioStateService(ILogger<RadioStateService> logger, IHubContext<RadioHub>? hubContext = null)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        // SignalR broadcast helper
        private void BroadcastUpdate(string property, object value)
        {
            _hubContext?.Clients.All.SendAsync("RadioStateUpdate", new { property, value });
        }

        // Keep existing methods
        public void SetBand(string receiver, string band)
        {
            if (receiver == "A")
                BandA = band;
            else if (receiver == "B")
                BandB = band;
        }

        public void SetAntenna(string receiver, string antenna)
        {
            if (receiver == "A")
                AntennaA = antenna;
            else if (receiver == "B")
                AntennaB = antenna;
        }

        public RadioState GetState()
        {
            return new RadioState
            {
                FrequencyA = FrequencyA,
                FrequencyB = FrequencyB,
                ModeA = ModeA,
                ModeB = ModeB,
                BandA = BandA,
                BandB = BandB,
                AntennaA = AntennaA,
                AntennaB = AntennaB,
                Controls = Controls
            };
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}