using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR;
using FTdx101_WebApp.Hubs;

namespace FTdx101_WebApp.Services
{
    public class RadioStateService : INotifyPropertyChanged, IRadioStateService
    {
        private readonly ILogger<RadioStateService> _logger;
        private readonly IHubContext<RadioHub>? _hubContext;

        public RadioStateService(ILogger<RadioStateService> logger, IHubContext<RadioHub>? hubContext = null)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        // Helper for property change and SignalR broadcast
        private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
                BroadcastUpdate(propertyName!, value!);
            }
        }

        private void BroadcastUpdate(string property, object value)
        {
            _hubContext?.Clients.All.SendAsync("RadioStateUpdate", new { property, value });
        }

        // --- Properties for all CAT commands in GetInitialValues() ---

        // ID
        private string _id = "";
        public string Id { get => _id; set => SetField(ref _id, value); }

        // AGC
        private int? _agcMain;
        public int? AGCMain { get => _agcMain; set => SetField(ref _agcMain, value); }
        private int? _agcSub;
        public int? AGCSub { get => _agcSub; set => SetField(ref _agcSub, value); }

        // RF Gain
        private int? _rfMain;
        public int? RFMain { get => _rfMain; set => SetField(ref _rfMain, value); }
        private int? _rfSub;
        public int? RFSub { get => _rfSub; set => SetField(ref _rfSub, value); }

        // Frequencies
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

        // VFO/Receiver
        private string? _fr;
        public string? FR { get => _fr; set => SetField(ref _fr, value); }
        private string? _ft;
        public string? FT { get => _ft; set => SetField(ref _ft, value); }

        // SSB Meter
        private string? _ss04;
        public string? SS04 { get => _ss04; set => SetField(ref _ss04, value); }
        private string? _ss14;
        public string? SS14 { get => _ss14; set => SetField(ref _ss14, value); }

        // AO, MG, PL, PR0, PR1, VS, KP, PC, RL0, RL1, NR0, NR1, NB0, NB1, NL0
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

        // Contour/Notch/CTCSS/Other
        public string? CO00 { get; set; }
        public string? CO10 { get; set; }
        public string? CO01 { get; set; }
        public string? CO11 { get; set; }
        public string? CO02 { get; set; }
        public string? CO12 { get; set; }
        public string? CO03 { get; set; }
        public string? CO13 { get; set; }
        public string? CN00 { get; set; }
        public string? CN10 { get; set; }
        public string? CT0 { get; set; }
        public string? CT1 { get; set; }

        // EX, SH, IS, AC, BP, GT, AN, PA, RF, CS, ML, BI, MS, KS, SS05, SS15, SS06, SS16, VT0, VX, VG, AV, CF, BC, KR, RA, SY, VD, DT0
        public string? EX030203 { get; set; }
        public string? EX030202 { get; set; }
        public string? EX030102 { get; set; }
        public string? EX030103 { get; set; }
        public string? EX040105 { get; set; }
        public string? EX030201 { get; set; }
        public string? EX010111 { get; set; }
        public string? EX010112 { get; set; }
        public string? EX030405 { get; set; }
        public string? EX010211 { get; set; }
        public string? EX010310 { get; set; }
        public string? EX010413 { get; set; }
        public string? EX010213 { get; set; }
        public string? EX010312 { get; set; }
        public string? EX010414 { get; set; }
        public string? EX0403021 { get; set; }
        public string? SH0 { get; set; }
        public string? SH1 { get; set; }
        public string? IS0 { get; set; }
        public string? IS1 { get; set; }
        public string? AC { get; set; }
        public string? BP00 { get; set; }
        public string? BP01 { get; set; }
        public string? BP10 { get; set; }
        public string? BP11 { get; set; }
        public string? GT0 { get; set; }
        public string? GT1 { get; set; }
        public string? AN0 { get; set; }
        public string? AN1 { get; set; }
        public string? PA0 { get; set; }
        public string? PA1 { get; set; }
        public string? RF0 { get; set; }
        public string? RF1 { get; set; }
        public string? CS { get; set; }
        public string? ML0 { get; set; }
        public string? ML1 { get; set; }
        public string? BI { get; set; }
        public string? MS { get; set; }
        public string? KS { get; set; }
        public string? SS05 { get; set; }
        public string? SS15 { get; set; }
        public string? SS06 { get; set; }
        public string? SS16 { get; set; }
        public string? VT0 { get; set; }
        public string? VX { get; set; }
        public string? VG { get; set; }
        public string? AV { get; set; }
        public string? CF000 { get; set; }
        public string? CF100 { get; set; }
        public string? CF001 { get; set; }
        public string? CF101 { get; set; }
        public string? BC0 { get; set; }
        public string? BC1 { get; set; }
        public string? KR { get; set; }
        public string? RA0 { get; set; }
        public string? RA1 { get; set; }
        public string? SY { get; set; }
        public string? VD { get; set; }
        public string? DT0 { get; set; }

        // Band tracking (non-reactive for now)
        public string BandA { get; private set; } = "20m";
        public string BandB { get; private set; } = "20m";
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
                AN0 = antenna;
            else if (receiver == "B")
                AN1 = antenna;
        }

        // ModeA and ModeB (string? or your enum type)
        private string? _modeA;
        public string? ModeA { get => _modeA; set => SetField(ref _modeA, value); }

        private string? _modeB;
        public string? ModeB { get => _modeB; set => SetField(ref _modeB, value); }

        // SMeterA and SMeterB (string? or int? depending on your design)
        private int? _sMeterA;
        public int? SMeterA { get; set; }

        private int? _sMeterB;
        public int? SMeterB { get; set; }

        // AntennaA and AntennaB (string? or int? depending on your design)
        private string? _antennaA;
        public string? AntennaA { get => _antennaA; set => SetField(ref _antennaA, value); }

        private string? _antennaB;
        public string? AntennaB { get => _antennaB; set => SetField(ref _antennaB, value); }

        // Power (int? or double? depending on your design)
        private int? _power;
        public int? Power { get => _power; set => SetField(ref _power, value); }

        // IsTransmitting (bool? or bool)
        private bool _isTransmitting;
        public bool IsTransmitting { get => _isTransmitting; set => SetField(ref _isTransmitting, value); }

        public RadioState GetState()
        {
            return new RadioState
            {
                FrequencyA = FrequencyA,
                FrequencyB = FrequencyB,
                BandA = BandA,
                BandB = BandB,
                Controls = Controls
            };
        }

        // Band tracking update
        public void UpdateBandFromFrequency()
        {
            BandA = GetBandFromFrequency(FrequencyA);
            BandB = GetBandFromFrequency(FrequencyB);
        }
        public string GetBandFromFrequency(long freq)
        {
            // Frequency in Hz
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
            if (freq >= 70000000 && freq < 70500000) return "4m"; // <-- Add this line for 70 MHz (4m band)
            return "Unknown";
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateFrequencyB(long freq)
        {
            _frequencyB = freq;
            // Add any notification or persistence logic if needed
        }

        
    }
}