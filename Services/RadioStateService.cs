using FTdx101_WebApp.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FTdx101_WebApp.Services
{
    public class RadioStateService : INotifyPropertyChanged, IRadioStateService
    {
        private readonly ILogger<RadioStateService> _logger;
        private readonly IHubContext<RadioHub> _hubContext;
        private readonly RadioStatePersistenceService _statePersistence;

        public bool IsInitialized { get; set; } = false;

        private RadioState _initialState;

        public RadioStateService(
            ILogger<RadioStateService> logger,
            RadioStatePersistenceService statePersistence,
            IHubContext<RadioHub> hubContext)
        {
            _logger = logger;
            _statePersistence = statePersistence;
            _hubContext = hubContext;
            _initialState = _statePersistence.Load();

            _logger.LogInformation("RadioStateService constructed with IHubContext: {HubContextAvailable}", hubContext != null);

            // ADD THIS LOG:
            _logger.LogInformation("RadioStateService constructed with initial state: ModeA={ModeA}, ModeB={ModeB}, Power={Power}, AntennaA={AntennaA}, AntennaB={AntennaB}, MicGain={MicGain}",
                _initialState.ModeA, _initialState.ModeB, _initialState.Power, _initialState.AntennaA, _initialState.AntennaB, _initialState.MicGain);

            // Initialize properties from _initialState
            FrequencyA = _initialState.FrequencyA;
            FrequencyB = _initialState.FrequencyB;
            BandA = _initialState.BandA;
            BandB = _initialState.BandB;
            ModeA = _initialState.ModeA ?? "";
            ModeB = _initialState.ModeB ?? "";
            AntennaA = _initialState.AntennaA ?? "";
            AntennaB = _initialState.AntennaB ?? "";
            RoofingFilterA = _initialState.RoofingFilterA ?? "";
            RoofingFilterB = _initialState.RoofingFilterB ?? "";
            Power = _initialState.Power;
            MicGain = _initialState.MicGain;
        }

        public RadioState InitialState => _initialState;

        private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                _logger.LogInformation("[SetField] Setting {Property} from {OldValue} to {NewValue}", propertyName, field, value);
                field = value;
                OnPropertyChanged(propertyName);
                BroadcastUpdate(propertyName!, value!);

                // Only save after initialization is complete
                if (IsInitialized)
                {
                    _logger.LogInformation("[SetField] Persisting state (IsInitialized=true, {Property}={Value})", propertyName, value);
                    _statePersistence.Save(this.ToRadioState());
                }
                else
                {
                    _logger.LogWarning("[SetField] NOT persisting {Property} (IsInitialized=false)", propertyName);
                }
            }
            else
            {
                // Log when value doesn't change (helpful for debugging band issues)
                if (propertyName == "BandA" || propertyName == "BandB")
                {
                    _logger.LogDebug("[SetField] {Property} unchanged (already {Value})", propertyName, value);
                }
            }
        }

        // Call this after DT0; is received
        public void CompleteInitialization()
        {
            IsInitialized = true;
            ReloadFromPersistence(); // Load the latest persisted state into memory
            // Do NOT call Save() here!
        }

        private void BroadcastUpdate(string property, object value)
        {
            _logger.LogInformation("[BroadcastUpdate] Broadcasting {Property} = {Value}", property, value);
            if (property == "PowerMeter")
            {
                _logger.LogWarning("[DEBUG][PowerMeter] Broadcasting PowerMeter value: {@Value}", value);
            }
            // Special case: PowerMeter should include isTransmitting for frontend sync
            if (property == "PowerMeter")
            {
                _hubContext.Clients.All.SendAsync("RadioStateUpdate", new { property, value = new { value, isTransmitting = this.IsTransmitting } });
            }
            else
            {
                _hubContext.Clients.All.SendAsync("RadioStateUpdate", new { property, value });
            }
        }

        // --- Properties for all CAT commands in GetInitialValues() ---

        private string _id = "";
        public string Id { get => _id; set => SetField(ref _id, value); }

        private int? _agcMain;
        public int? AGCMain { get => _agcMain; set => SetField(ref _agcMain, value); }
        private int? _agcSub;
        public int? AGCSub { get => _agcSub; set => SetField(ref _agcSub, value); }

        private int? _rfMain;
        public int? RFMain { get => _rfMain; set => SetField(ref _rfMain, value); }
        private int? _rfSub;
        public int? RFSub { get => _rfSub; set => SetField(ref _rfSub, value); }

        private long _frequencyA;
        public long FrequencyA
        {
            get => _frequencyA;
            set
            {
                SetField(ref _frequencyA, value);
                UpdateBandFromFrequency();
            }
        }

        private long _frequencyB;
        public long FrequencyB
        {
            get => _frequencyB;
            set
            {
                SetField(ref _frequencyB, value);
                UpdateBandFromFrequency();
            }
        }

        private string? _fr;
        public string? FR { get => _fr; set => SetField(ref _fr, value); }
        private string? _ft;
        public string? FT { get => _ft; set => SetField(ref _ft, value); }

        private string? _ss04;
        public string? SS04 { get => _ss04; set => SetField(ref _ss04, value); }
        private string? _ss14;
        public string? SS14 { get => _ss14; set => SetField(ref _ss14, value); }

        private string? _ao;
        public string? AO { get => _ao; set => SetField(ref _ao, value); }
        private string? _mg;
        public string? MG { get => _mg; set => SetField(ref _mg, value); }
        private string? _pl;
        public string? PL { get => _pl; set => SetField(ref _pl, value); }
        private string? _pr0;
        public string? PR0 { get => _pr0; set => SetField(ref _pr0, value); }
        private string? _pr1;
        public string? PR1 { get => _pr1; set => SetField(ref _pr1, value); }
        private string? _md0;
        public string? MD0 { get => _md0; set => SetField(ref _md0, value); }
        private string? _md1;
        public string? MD1 { get => _md1; set => SetField(ref _md1, value); }
        private string? _vs;
        public string? VS { get => _vs; set => SetField(ref _vs, value); }
        private string? _kp;
        public string? KP { get => _kp; set => SetField(ref _kp, value); }
        private string? _pc;
        public string? PC { get => _pc; set => SetField(ref _pc, value); }
        private string? _rl0;
        public string? RL0 { get => _rl0; set => SetField(ref _rl0, value); }
        private string? _rl1;
        public string? RL1 { get => _rl1; set => SetField(ref _rl1, value); }
        private string? _nr0;
        public string? NR0 { get => _nr0; set => SetField(ref _nr0, value); }
        private string? _nr1;
        public string? NR1 { get => _nr1; set => SetField(ref _nr1, value); }
        private string? _nb0;
        public string? NB0 { get => _nb0; set => SetField(ref _nb0, value); }
        private string? _nb1;
        public string? NB1 { get => _nb1; set => SetField(ref _nb1, value); }
        private string? _nl0;
        public string? NL0 { get => _nl0; set => SetField(ref _nl0, value); }

        private string _bandA = "20m";
        public string BandA { get => _bandA; set => SetField(ref _bandA, value); }

        private string _bandB = "20m";
        public string BandB { get => _bandB; set => SetField(ref _bandB, value); }

        public Dictionary<string, object> Controls { get; } = new();

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

        private int _power;
        public int Power { get => _power; set => SetField(ref _power, value); }

        private string? _modeA = "";
        public string? ModeA { get => _modeA; set => SetField(ref _modeA, value); }

        private string? _modeB = "";
        public string? ModeB { get => _modeB; set => SetField(ref _modeB, value); }

        private string? _antennaA = "";
        public string? AntennaA { get => _antennaA; set => SetField(ref _antennaA, value); }

        private string? _antennaB = "";
        public string? AntennaB { get => _antennaB; set => SetField(ref _antennaB, value); }

        private string _roofingFilterA = "";
        public string RoofingFilterA { get => _roofingFilterA; set => SetField(ref _roofingFilterA, value); }

        private string _roofingFilterB = "";
        public string RoofingFilterB { get => _roofingFilterB; set => SetField(ref _roofingFilterB, value); }


        private int? _sMeterA;
        public int? SMeterA
        {
            get => _sMeterA;
            set
            {
                if (value == null) return;
                int clamped = Math.Clamp(value.Value, 0, 255);
                // Spike filtering removed for full responsiveness
                SetField(ref _sMeterA, clamped);
            }
        }

        private int? _sMeterB;
        public int? SMeterB
        {
            get => _sMeterB;
            set
            {
                if (value == null) return;
                int clamped = Math.Clamp(value.Value, 0, 255);
                // Spike filtering removed for full responsiveness
                SetField(ref _sMeterB, clamped);
            }
        }

        private int? _powerMeter;
        public int? PowerMeter { get => _powerMeter; set => SetField(ref _powerMeter, value); }

        private int? _swrMeter;
        public int? SWRMeter
        {
            get => _swrMeter;
            set
            {
                if (value == null) return;
                int clamped = Math.Clamp(value.Value, 0, 255);
                if (_swrMeter.HasValue && clamped != 0 && Math.Abs(clamped - _swrMeter.Value) > 50)
                {
                    _logger.LogWarning("[SWRMeter] Ignored spike: {Old} -> {New}", _swrMeter, clamped);
                    return;
                }
                SetField(ref _swrMeter, clamped);
            }
        }

        // Removed duplicate int? _power and int? Power definitions. Only int _power and int Power property remain.

        private int? _maxPower;
        public int? MaxPower
        {
            get => _maxPower;
            set => SetField(ref _maxPower, value);
        }

        private bool _isTransmitting;
        public bool IsTransmitting { get => _isTransmitting; set => SetField(ref _isTransmitting, value); }

        private int? _iddMeter;
        public int? IDDMeter { get => _iddMeter; set => SetField(ref _iddMeter, value); }

        private int? _vddMeter;
        public int? VDDMeter { get => _vddMeter; set => SetField(ref _vddMeter, value); }

        private int? _temperature;
        public int? Temperature { get => _temperature; set => SetField(ref _temperature, value); }

        private int _afGainA = 128;
        public int AfGainA { get => _afGainA; set => SetField(ref _afGainA, value); }
        private int _afGainB = 128;
        public int AfGainB { get => _afGainB; set => SetField(ref _afGainB, value); }

        private int _micGain = 50;
        public int MicGain { get => _micGain; set => SetField(ref _micGain, value); }

        private bool _radioPowerOn = true; // Assume on when app starts
        public bool RadioPowerOn { get => _radioPowerOn; set => SetField(ref _radioPowerOn, value); }

        // TX VFO: 0 = VFO A is TX, 1 = VFO B is TX
        private int _txVfo = 0;
        public int TxVfo { get => _txVfo; set => SetField(ref _txVfo, value); }

        public RadioState GetState()
        {
            return new RadioState
            {
                FrequencyA = FrequencyA,
                FrequencyB = FrequencyB,
                BandA = BandA,
                BandB = BandB,
                ModeA = ModeA ?? "",
                ModeB = ModeB ?? "",
                AntennaA = AntennaA ?? "",
                AntennaB = AntennaB ?? "",
                Power = Power,
                AfGainA = AfGainA,
                AfGainB = AfGainB,
                MicGain = MicGain,
                Controls = Controls
            };
        }

        public void UpdateBandFromFrequency()
        {
            var newBandA = GetBandFromFrequency(FrequencyA);
            var newBandB = GetBandFromFrequency(FrequencyB);

            _logger.LogInformation("[UpdateBandFromFrequency] FreqA={FreqA} -> BandA={OldBandA} -> {NewBandA}, FreqB={FreqB} -> BandB={OldBandB} -> {NewBandB}",
                FrequencyA, BandA, newBandA, FrequencyB, BandB, newBandB);

            BandA = newBandA;
            BandB = newBandB;
        }
        public string GetBandFromFrequency(long freq)
        {
            if (freq >= 1800000 && freq < 2000000) return "160m";
            if (freq >= 3500000 && freq < 4000000) return "80m";
            if (freq >= 5351500 && freq <= 5366500) return "60m";
            if (freq >= 7000000 && freq < 7300000) return "40m";
            if (freq >= 10100000 && freq < 10150000) return "30m";
            if (freq >= 14000000 && freq < 14350000) return "20m";
            if (freq >= 18068000 && freq < 18168000) return "17m";
            if (freq >= 21000000 && freq < 21450000) return "15m";
            if (freq >= 24890000 && freq < 24990000) return "12m";
            if (freq >= 28000000 && freq < 29700000) return "10m";
            if (freq >= 50000000 && freq < 54000000) return "6m";
            if (freq >= 70000000 && freq < 70500000) return "4m";
            return "Unknown";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateFrequencyB(long freq)
        {
            _frequencyB = freq;
        }

        public void ReloadFromPersistence()
        {
            var state = _statePersistence.Load();
            FrequencyA = state.FrequencyA;
            FrequencyB = state.FrequencyB;
            BandA = state.BandA ?? string.Empty;
            BandB = state.BandB ?? string.Empty;
            ModeA = state.ModeA ?? string.Empty;
            ModeB = state.ModeB ?? string.Empty;
            AntennaA = state.AntennaA ?? string.Empty;
            AntennaB = state.AntennaB ?? string.Empty;
            RoofingFilterA = state.RoofingFilterA ?? string.Empty;
            RoofingFilterB = state.RoofingFilterB ?? string.Empty;
            Power = state.Power;
            AfGainA = state.AfGainA;
            AfGainB = state.AfGainB;
            MicGain = state.MicGain;
        }

        public RadioState ToRadioState()
        {
            return new RadioState
            {
                FrequencyA = FrequencyA,
                FrequencyB = FrequencyB,
                BandA = BandA,
                BandB = BandB,
                ModeA = ModeA ?? "",
                ModeB = ModeB ?? "",
                AntennaA = AntennaA ?? "",
                AntennaB = AntennaB ?? "",
                RoofingFilterA = RoofingFilterA ?? "",
                RoofingFilterB = RoofingFilterB ?? "",
                Power = Power,
                AfGainA = AfGainA,
                AfGainB = AfGainB,
                MicGain = MicGain
            };
        }
    }
}