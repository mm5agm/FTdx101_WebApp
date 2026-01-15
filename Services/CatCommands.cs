namespace FTdx101_WebApp.Services
{
    /// <summary>
    /// FT-dx101MP CAT Command Reference
    /// Complete list of CAT commands for the Yaesu FT-dx101MP transceiver
    /// </summary>
    public static class CatCommands
    {
        // FREQUENCY COMMANDS
        /// <summary>FA - Frequency Main Band (VFO-A). Format: FA000030000-075000000; (Hz)</summary>
        public const string FrequencyVfoA = "FA";

        /// <summary>FB - Frequency Sub Band (VFO-B). Format: FB000030000-075000000; (Hz)</summary>
        public const string FrequencyVfoB = "FB";

        // MODE COMMANDS
        /// <summary>MD - Operating Mode Main. Format: MD0[1-E]; where 1=LSB, 2=USB, 3=CW, 4=FM, 5=AM, 6=RTTY-LSB, 7=CW-R, 8=DATA-LSB, 9=RTTY-USB, A=DATA-FM, B=FM-N, C=DATA-USB, D=AM-N, E=C4FM</summary>
        public const string ModeMain = "MD0";

        /// <summary>MD - Operating Mode Sub. Format: MD1[1-E];</summary>
        public const string ModeSub = "MD1";

        // S-METER COMMANDS
        /// <summary>SM - S-Meter Main. Format: SM0; Returns: SM0000-SM0255</summary>
        public const string SMeterMain = "SM0";

        /// <summary>SM - S-Meter Sub. Format: SM1; Returns: SM1000-SM1255</summary>
        public const string SMeterSub = "SM1";

        // TRANSMIT STATUS
        /// <summary>TX - Transmit Status. Returns: TX0; (RX) or TX1; (TX)</summary>
        public const string TransmitStatus = "TX";

        // POWER COMMANDS
        /// <summary>PC - TX Power. Format: PC000-PC200; (0-200 watts)</summary>
        public const string TxPower = "PC";

        // AGC COMMANDS
        /// <summary>GT - AGC. Format: GT0[0-4]; where 0=OFF, 1=FAST, 2=MID, 3=SLOW, 4=AUTO</summary>
        public const string Agc = "GT0";

        // FILTER COMMANDS
        /// <summary>SH - Filter High. Format: SH0[0-21];</summary>
        public const string FilterHigh = "SH0";

        /// <summary>SL - Filter Low. Format: SL0[0-21];</summary>
        public const string FilterLow = "SL0";

        // CLARIFIER COMMANDS
        /// <summary>RC - Clarifier (RIT/XIT) Clear</summary>
        public const string ClarifierClear = "RC";

        /// <summary>RD - Clarifier Down. Format: RD0000-RD9999; (Hz)</summary>
        public const string ClarifierDown = "RD";

        /// <summary>RU - Clarifier Up. Format: RU0000-RU9999; (Hz)</summary>
        public const string ClarifierUp = "RU";

        // SPLIT OPERATION
        /// <summary>FT - Split Operation. Format: FT0; (OFF) or FT1; (ON)</summary>
        public const string Split = "FT";

        // LOCK COMMANDS
        /// <summary>LK - Lock. Format: LK0; (OFF) or LK1-LK9; (various lock modes)</summary>
        public const string Lock = "LK";

        // MENU COMMANDS  
        /// <summary>EX - Extended Menu. Format: EX[menu][value];</summary>
        public const string ExtendedMenu = "EX";

        // INFORMATION COMMANDS
        /// <summary>IF - Information. Returns comprehensive radio status</summary>
        public const string Information = "IF";

        /// <summary>ID - Radio ID. Returns: ID020; for FT-dx101MP</summary>
        public const string RadioId = "ID";

        // VFO COMMANDS
        /// <summary>VS - VFO Select. Format: VS0; (VFO-A) or VS1; (VFO-B)</summary>
        public const string VfoSelect = "VS";

        /// <summary>AB - VFO A=B</summary>
        public const string VfoAEqualsB = "AB";

        /// <summary>BA - VFO B=A</summary>
        public const string VfoBEqualsA = "BA";

        /// <summary>SV - VFO A/B Swap</summary>
        public const string VfoSwap = "SV";

        // MEMORY COMMANDS
        /// <summary>MC - Memory Channel. Format: MC[001-118];</summary>
        public const string MemoryChannel = "MC";

        /// <summary>MR - Memory Read. Format: MR[001-118];</summary>
        public const string MemoryRead = "MR";

        /// <summary>MW - Memory Write. Format: MW[channel][data];</summary>
        public const string MemoryWrite = "MW";

        // BAND COMMANDS
        /// <summary>BS - Band Select. Format: BS[00-17]; (00=160m, 01=80m, 02=60m, 03=40m, 04=30m, 05=20m, 06=17m, 07=15m, 08=12m, 09=10m, 10=6m, 11=GEN, 12=MW, 13=AIR, 14=2m, 15=70cm, 16=1.2GHz, 17=PSK)</summary>
        public const string BandSelect = "BS";

        // CLARIFIER/RIT/XIT
        /// <summary>RT - RIT On/Off. Format: RT0; (OFF) or RT1; (ON)</summary>
        public const string RitOnOff = "RT";

        /// <summary>XT - XIT On/Off. Format: XT0; (OFF) or XT1; (ON)</summary>
        public const string XitOnOff = "XT";

        // KEYER COMMANDS
        /// <summary>KY - Keyer. Format: KY [text]; (send CW)</summary>
        public const string Keyer = "KY";

        /// <summary>KS - Keyer Speed. Format: KS000-KS060; (WPM)</summary>
        public const string KeyerSpeed = "KS";

        // NOISE BLANKER
        /// <summary>NB - Noise Blanker. Format: NB00; (OFF), NB01; (NB1), NB02; (NB2), NB03; (NB1+NB2)</summary>
        public const string NoiseBlanker = "NB0";

        // NOISE REDUCTION
        /// <summary>NR - Noise Reduction. Format: NR00-NR15;</summary>
        public const string NoiseReduction = "NR0";

        // NOTCH FILTER
        /// <summary>BC - Auto Notch. Format: BC00; (OFF), BC01; (ON)</summary>
        public const string AutoNotch = "BC0";

        // CONTOUR
        /// <summary>CO - Contour. Format: CO00[0-31][0-31]; (Level, Width)</summary>
        public const string Contour = "CO00";

        // DNR (Digital Noise Reduction)
        /// <summary>RL - DNR Level. Format: RL0[00-15];</summary>
        public const string DnrLevel = "RL0";

        // HELPER METHODS
        /// <summary>Format frequency command for VFO-A</summary>
        public static string FormatFrequencyA(long frequencyHz)
        {
            return $"FA{frequencyHz:D9};";
        }

        /// <summary>Format frequency command for VFO-B</summary>
        public static string FormatFrequencyB(long frequencyHz)
        {
            return $"FB{frequencyHz:D9};";
        }

        /// <summary>Parse frequency response (FA or FB)</summary>
        public static long ParseFrequency(string response)
        {
            if (response.Length >= 11 && (response.StartsWith("FA") || response.StartsWith("FB")))
            {
                if (long.TryParse(response.Substring(2, 9), out long freq))
                {
                    return freq;
                }
            }
            return 0;
        }

        /// <summary>Format mode command</summary>
        public static string FormatMode(string mode, bool isSubVfo = false)
        {
            var modeCode = mode.ToUpper() switch
            {
                "LSB" => "1",
                "USB" => "2",
                "CW" => "3",
                "FM" => "4",
                "AM" => "5",
                "RTTY-LSB" => "6",
                "CW-R" => "7",
                "DATA-LSB" => "8",
                "RTTY-USB" => "9",
                "DATA-FM" => "A",
                "FM-N" => "B",
                "DATA-USB" => "C",
                "AM-N" => "D",
                _ => "2" // Default to USB
            };
            return $"MD{(isSubVfo ? "1" : "0")}{modeCode};";
        }

        /// <summary>Parse mode response</summary>
        public static string ParseMode(string response)
        {
            if (response.Length >= 4 && response.StartsWith("MD"))
            {
                var modeCode = response.Substring(3, 1);
                return modeCode switch
                {
                    "1" => "LSB",
                    "2" => "USB",
                    "3" => "CW",
                    "4" => "FM",
                    "5" => "AM",
                    "6" => "RTTY-LSB",
                    "7" => "CW-R",
                    "8" => "DATA-LSB",
                    "9" => "RTTY-USB",
                    "A" => "DATA-FM",
                    "B" => "FM-N",
                    "C" => "DATA-USB",
                    "D" => "AM-N",
                    _ => "UNKNOWN"
                };
            }
            return "UNKNOWN";
        }

        /// <summary>Parse S-Meter response (0-255 scale)</summary>
        public static int ParseSMeter(string response)
        {
            if (response.Length >= 6 && response.StartsWith("SM"))
            {
                if (int.TryParse(response.Substring(3), out int value))
                {
                    return value;
                }
            }
            return 0;
        }
    }
}