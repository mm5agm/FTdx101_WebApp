namespace FTdx101_WebApp.Services
{
    /// <summary>
    /// FT-dx101MP CAT Command Reference
    /// Complete list of CAT commands for the Yaesu FT-dx101MP transceiver
    /// </summary>
    public static class CatCommands
    {
        // FREQUENCY COMMANDS
        public const string FrequencyVfoA = "FA";
        public const string FrequencyVfoB = "FB";

        // MODE COMMANDS
        public const string ModeMain = "MD0";
        public const string ModeSub = "MD1";

        // S-METER COMMANDS
        public const string SMeterMain = "SM0";
        public const string SMeterSub = "SM1";

        // METER READING COMMANDS (RM)
        public const string MeterPower = "RM1";    // Power output meter (0-255)
        public const string MeterSWR = "RM2";      // SWR meter (0-255)
        public const string MeterALC = "RM3";      // ALC meter (0-255)
        public const string MeterComp = "RM4";     // Compression meter (0-255)

        // TRANSMIT STATUS
        public const string TransmitStatus = "TX";

        // POWER COMMANDS
        public const string TxPower = "PC";

        // AGC COMMANDS
        public const string Agc = "GT0";

        // FILTER COMMANDS
        public const string FilterHigh = "SH0";
        public const string FilterLow = "SL0";

        // CLARIFIER COMMANDS
        public const string ClarifierClear = "RC";
        public const string ClarifierDown = "RD";
        public const string ClarifierUp = "RU";

        // SPLIT OPERATION
        public const string Split = "FT";

        // LOCK COMMANDS
        public const string Lock = "LK";

        // MENU COMMANDS  
        public const string ExtendedMenu = "EX";

        // INFORMATION COMMANDS
        public const string Information = "IF";
        public const string RadioId = "ID";

        // VFO COMMANDS
        public const string VfoSelect = "VS";
        public const string VfoAEqualsB = "AB";
        public const string VfoBEqualsA = "BA";
        public const string VfoSwap = "SV";

        // MEMORY COMMANDS
        public const string MemoryChannel = "MC";
        public const string MemoryRead = "MR";
        public const string MemoryWrite = "MW";

        // BAND COMMANDS
        public const string BandSelect = "BS";

        // CLARIFIER/RIT/XIT
        public const string RitOnOff = "RT";
        public const string XitOnOff = "XT";

        // KEYER COMMANDS
        public const string Keyer = "KY";
        public const string KeyerSpeed = "KS";

        // NOISE BLANKER
        public const string NoiseBlanker = "NB0";

        // NOISE REDUCTION
        public const string NoiseReduction = "NR0";

        // NOTCH FILTER
        public const string AutoNotch = "BC0";

        // CONTOUR
        public const string Contour = "CO00";

        // DNR (Digital Noise Reduction)
        public const string DnrLevel = "RL0";

        // HELPER METHODS
        public static string FormatFrequencyA(long frequencyHz)
            => $"FA{frequencyHz:D9};";

        public static string FormatFrequencyB(long frequencyHz)
            => $"FB{frequencyHz:D9};";

        public static long ParseFrequency(string response)
        {
            // Remove trailing semicolon if present
            var trimmed = response.TrimEnd(';');
            // Ensure the response is at least 11 characters (e.g., "FA007100000")
            if (trimmed.Length >= 11 && (trimmed.StartsWith("FA") || trimmed.StartsWith("FB")))
            {
                var freqStr = trimmed.Substring(2, 9);
                if (long.TryParse(freqStr, out var freq))
                    return freq;
            }
            return 0;
        }

        public static string FormatMode(string mode, bool isSubVfo = false)
        {
            var modeCode = mode.ToUpper() switch
            {
                "LSB"      => "1",
                "USB"      => "2",
                "CW-U"     => "3",
                "FM"       => "4",
                "AM"       => "5",
                "RTTY-L"   => "6",
                "CW-L"     => "7",
                "DATA-L"   => "8",
                "RTTY-U"   => "9",
                "DATA-FM"  => "A",
                "FM-N"     => "B",
                "DATA-U"   => "C",
                "AM-N"     => "D",
                "PSK"      => "E",
                "DATA-FM-N" => "F",
                _ => "2" // Default to USB
            };
            return $"MD{(isSubVfo ? "1" : "0")}{modeCode};";
        }

        public static string ParseMode(string response)
        {
            if (response.Length >= 4 && response.StartsWith("MD"))
            {
                var modeCode = response.Substring(3, 1);
                return modeCode switch
                {
                    "1" => "LSB",
                    "2" => "USB",
                    "3" => "CW-U",
                    "4" => "FM",
                    "5" => "AM",
                    "6" => "RTTY-L",
                    "7" => "CW-L",
                    "8" => "DATA-L",
                    "9" => "RTTY-U",
                    "A" => "DATA-FM",
                    "B" => "FM-N",
                    "C" => "DATA-U",
                    "D" => "AM-N",
                    "E" => "PSK",
                    "F" => "DATA-FM-N",
                    _ => "UNKNOWN"
                };
            }
            return "UNKNOWN";
        }

        public static int ParseSMeter(string response)
        {
            if (string.IsNullOrEmpty(response) || !response.StartsWith("SM"))
                return 0;

            int semicolonIndex = response.IndexOf(';');
            if (semicolonIndex > 0)
            {
                response = response.Substring(0, semicolonIndex);
            }
            if (response.Length >= 5)
            {
                string valueStr = response.Substring(3);
                if (int.TryParse(valueStr, out int value))
                {
                    return value;
                }
            }
            return 0;
        }

        public static int ParseMeterReading(string response)
        {
            // Parse RM1, RM2, RM3, RM4 responses
            // Format: RMx0000; where x is meter type and 0000 is the value (0-255)
            if (string.IsNullOrEmpty(response) || !response.StartsWith("RM"))
                return 0;

            int semicolonIndex = response.IndexOf(';');
            if (semicolonIndex > 0)
            {
                response = response.Substring(0, semicolonIndex);
            }
            if (response.Length >= 6)
            {
                string valueStr = response.Substring(2); // Skip "RM"
                // Remove the meter type digit (1-4)
                if (valueStr.Length > 1)
                {
                    valueStr = valueStr.Substring(1); // Skip meter type
                    if (int.TryParse(valueStr, out int value))
                    {
                        return value;
                    }
                }
            }
            return 0;
        }

        // Initialization commands for the transceiver
        public static readonly string[] InitializationCommands = new[]
        {
            "ID;", "AG0;", "AG1;", "RG0;", "RG1;", "FA;", "FB;", "FR;", "FT;", "SS04;", "SS14;", "AO;", "MG;", "PL;", "PR0;", "PR1;", "MD0;", "MD1;", "VS;", "KP;", "PC;", "RL0;", "RL1;", "NR0;", "NR1;", "NB0;", "NB1;", "NL0;", "CO00;", "CO10;", "CO01;", "CO11;", "CO02;", "CO12;", "CO03;", "CO13;", "CN00;", "CN10;", "CT0;", "CT1;", "EX030203;", "EX030202;", "EX030102;", "EX030103;", "EX040105;", "EX030201;", "EX010111;", "EX010112;", "EX030405;", "EX010111;", "EX010211;", "EX010310;", "EX010413;", "EX010112;", "EX010213;", "EX010312;", "EX010414;", "EX0403021;", "SH0;", "SH1;", "IS0;", "SS06;", "IS1;", "AC;", "KP;", "FT;", "IF;", "BP00;", "BP01;", "BP10;", "BP11;", "GT0;", "GT1;", "AN0;", "AN1;", "PA0;", "PA1;", "RF0;", "RF1;", "ID;", "CS;", "ML0;", "ML1;", "BI;", "MS;", "KS;", "SS05;", "SS15;", "SS06;", "SS16;", "VT0;", "VX;", "VG;", "AV;", "CF000;", "CF100;", "CF001;", "CF101;", "BC0;", "BC1;", "KR;", "RA0;", "RA1;", "SY;", "VD;", "DT0;"
};
    }

    public static class IFCommandParser
    {
        public static (long frequency, string mode) ParseIFResponse(string response)
        {
            if (string.IsNullOrEmpty(response) || response.Length < 20 || !response.StartsWith("IF"))
            {
                Console.WriteLine($"IFCommandParser: Invalid response: [{response}]");
                return (0, "UNKNOWN");
            }

            try
            {
                string freqStr = response.Substring(5, 9);
                long frequency = long.Parse(freqStr);
                Console.WriteLine($"IFCommandParser: Parsed freq from [{response}] -> [{freqStr}] = {frequency} Hz");
                string mode = "USB";
                return (frequency, mode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IFCommandParser: Exception parsing [{response}]: {ex.Message}");
                return (0, "UNKNOWN");
            }
        }
    }
}