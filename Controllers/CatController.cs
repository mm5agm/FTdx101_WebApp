using Microsoft.AspNetCore.Mvc;
using FTdx101_WebApp.Services;
using System.Text.Json;
using System.Threading;

namespace FTdx101_WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatController : ControllerBase
    {
        private readonly ICatClient _catClient;
        private readonly ISettingsService _settingsService;
        private readonly ILogger<CatController> _logger;
        private readonly RadioStateService _radioStateService;
        private readonly RadioStatePersistenceService _statePersistence;
        private readonly RadioInitializationService _radioInitService;
        private static readonly SemaphoreSlim _requestSemaphore = new(1, 1);

        [HttpPost("afgain/a")]
        public async Task<IActionResult> SetAfGainA([FromBody] int value)
        {
            _logger.LogInformation("[API] SetAfGainA called: value={Value}", value);
            if (!_catClient.IsConnected)
                await EnsureConnectedAsync();
            await _catClient.SetAfGainAAsync(value);
            _radioStateService.AfGainA = value;
            _logger.LogInformation("Set Receiver A AF Gain to {Value}", value);
            _logger.LogInformation("[API] SetAfGainA completed: value={Value}", value);
            return Ok(new { message = $"AF Gain {value} set for Receiver A" });
        }

        [HttpPost("afgain/b")]
        public async Task<IActionResult> SetAfGainB([FromBody] int value)
        {
            _logger.LogInformation("[API] SetAfGainB called: value={Value}", value);
            if (!_catClient.IsConnected)
                await EnsureConnectedAsync();
            await _catClient.SetAfGainBAsync(value);
            _radioStateService.AfGainB = value;
            _logger.LogInformation("Set Receiver B AF Gain to {Value}", value);
            _logger.LogInformation("[API] SetAfGainB completed: value={Value}", value);
            return Ok(new { message = $"AF Gain {value} set for Receiver B" });
        }

        [HttpPost("micgain")]
        public async Task<IActionResult> SetMicGain([FromBody] MicGainRequest request)
        {
            _logger.LogInformation("[API] SetMicGain called: value={Value}", request.Value);

            if (!await _requestSemaphore.WaitAsync(2000))
                return StatusCode(503, new { error = "Radio busy" });

            try
            {
                await EnsureConnectedAsync();
                if (request.Value < 0 || request.Value > 100)
                    return BadRequest(new { error = "MIC Gain value out of range (0-100)" });

                string command = $"MG{request.Value:D3};";
                await _catClient.SendCommandAsync(command, "WebUI", CancellationToken.None);

                // Persist MIC Gain value
                _logger.LogWarning("[MicGain API] Setting _radioStateService.MicGain to {Value}", request.Value);
                _radioStateService.MicGain = request.Value;

                _logger.LogInformation("Set MIC Gain to {Value}", request.Value);
                return Ok(new { message = $"MIC Gain set to {request.Value}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting MIC Gain");
                return StatusCode(500, new { error = "Failed to set MIC Gain" });
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }

        [HttpPost("radiopower")]
        public async Task<IActionResult> SetRadioPower([FromBody] RadioPowerRequest request)
        {
            _logger.LogInformation("[API] SetRadioPower called: powerOn={PowerOn}", request.PowerOn);
            try
            {
                if (request.PowerOn)
                {
                    _logger.LogInformation("Turning radio ON...");
                    await _catClient.SendCommandAsync("PS1;", "WebUI", CancellationToken.None);
                    await Task.Delay(1500);
                    await _catClient.SendCommandAsync("PS1;", "WebUI", CancellationToken.None);
                    _radioStateService.RadioPowerOn = true;
                    _logger.LogInformation("Radio power ON command sent");
                    await Task.Delay(3000);
                    _logger.LogInformation("Re-initializing radio after power on...");
                    await _radioInitService.InitializeRadioAsync();
                    _logger.LogInformation("[API] SetRadioPower completed: powerOn=true");
                    return Ok(new { message = "Radio powered ON and initialized", powerOn = true });
                }
                else
                {
                    _logger.LogInformation("Turning radio OFF...");
                    await _catClient.SendCommandAsync("PS0;", "WebUI", CancellationToken.None);
                    _radioStateService.RadioPowerOn = false;
                    AppStatus.InitializationStatus = "radio_off";
                    _logger.LogInformation("Radio power OFF command sent");
                    _logger.LogInformation("[API] SetRadioPower completed: powerOn=false");
                    return Ok(new { message = "Radio powered OFF", powerOn = false });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting radio power");
                return StatusCode(500, new { error = "Failed to set radio power" });
            }
        }

        [HttpGet("radiopower")]
        public IActionResult GetRadioPowerStatus()
        {
            return Ok(new { powerOn = _radioStateService.RadioPowerOn });
        }

        [HttpPost("tx")]
        public async Task<IActionResult> ToggleTransmit([FromBody] TxRequest request)
        {
            _logger.LogInformation("[API] ToggleTransmit called: transmit={Transmit}", request.Transmit);
            try
            {
                if (request.Transmit)
                {
                    _logger.LogInformation("Turning TX ON...");
                    await _catClient.SendCommandAsync("TX1;", "WebUI", CancellationToken.None);
                    _radioStateService.IsTransmitting = true;
                    _logger.LogInformation("[API] ToggleTransmit completed: transmitting=true");
                    return Ok(new { message = "TX ON", transmitting = true });
                }
                else
                {
                    _logger.LogInformation("Turning TX OFF...");
                    await _catClient.SendCommandAsync("TX0;", "WebUI", CancellationToken.None);
                    _radioStateService.IsTransmitting = false;
                    _logger.LogInformation("[API] ToggleTransmit completed: transmitting=false");
                    return Ok(new { message = "TX OFF", transmitting = false });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling TX");
                return StatusCode(500, new { error = "Failed to toggle TX" });
            }
        }

        [HttpGet("tx")]
        public IActionResult GetTxStatus()
        {
            return Ok(new { 
                transmitting = _radioStateService.IsTransmitting,
                txVfo = _radioStateService.TxVfo
            });
        }

        // Static band frequency mapping (apply this at the top of your class)
        private static readonly Dictionary<string, long> BandFreqs = new(StringComparer.OrdinalIgnoreCase)
        {
            { "160m", 1840000 }, { "80m", 3700000 }, { "60m", 5357000 },
            { "40m", 7100000 }, { "30m", 10136000 }, { "20m", 14074000 },
            { "17m", 18110000 }, { "15m", 21074000 }, { "12m", 24915000 },
            { "10m", 28074000 }, { "6m", 50313000 }, { "4m", 70100000 }
        };

        private static readonly Dictionary<string, string> CatCodeToMode = new()
        {
            { "1", "LSB" },
            { "2", "USB" },
            { "3", "CW-U" },
            { "4", "FM" },
            { "5", "AM" },
            { "6", "RTTY-L" },
            { "7", "CW-L" },
            { "8", "DATA-L" },
            { "9", "RTTY-U" },
            { "A", "DATA-FM" },
            { "B", "FM-N" },
            { "C", "DATA-U" },
            { "D", "AM-N" },
            { "E", "PSK" },
            { "F", "DATA-FM-N" }
        };

        public CatController(
            ICatClient catClient,
            ISettingsService settingsService,
            ILogger<CatController> logger,
            RadioStateService radioStateService,
            RadioStatePersistenceService statePersistence,
            RadioInitializationService radioInitService)
        {
            _catClient = catClient;
            _settingsService = settingsService;
            _logger = logger;
            _radioStateService = radioStateService;
            _statePersistence = statePersistence;
            _radioInitService = radioInitService;
        }

        private async Task EnsureConnectedAsync()
        {
            // RadioInitializationService handles connection and state restoration on startup.
            // This method only needs to verify the connection is still active.
            if (!_catClient.IsConnected)
            {
                var settings = await _settingsService.GetSettingsAsync();
                await _catClient.ConnectAsync(settings.SerialPort, settings.BaudRate);
            }
            // No redundant restoration needed - RadioInitializationService already did it
        }

        private async Task<string> GetMainVfoAsync()
        {
            var response = await _catClient.SendCommandAsync("IF;", "WebUI", CancellationToken.None);
            if (!string.IsNullOrEmpty(response) && response.Length > 5)
                return response[5] == '1' ? "B" : "A";
            return "A";
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            if (_radioStateService.FrequencyA < 100 || _radioStateService.FrequencyB < 100)
            {
                await EnsureConnectedAsync();
            }

            // Log what we're returning for debugging
            _logger.LogInformation("[API] GetStatus called");
            _logger.LogInformation("[API Status] Returning: FreqA={FreqA}, BandA={BandA}, FreqB={FreqB}, BandB={BandB}",
                _radioStateService.FrequencyA, _radioStateService.BandA,
                _radioStateService.FrequencyB, _radioStateService.BandB);

            var settings = await _settingsService.GetSettingsAsync();
            return Ok(new
            {
                radioModel = settings.RadioModel,
                vfoA = new
                {
                    frequency = _radioStateService.FrequencyA,
                    band = _radioStateService.BandA,
                    sMeter = _radioStateService.SMeterA ?? 0,
                    power = _radioStateService.PowerMeter ?? 0,
                    mode = _radioStateService.ModeA ?? "",
                    antenna = _radioStateService.AntennaA ?? "",
                    afGain = _radioStateService.AfGainA,
                    roofingFilter = _radioStateService.RoofingFilterA ?? ""
                },
                vfoB = new
                {
                    frequency = _radioStateService.FrequencyB,
                    band = _radioStateService.BandB,
                    sMeter = _radioStateService.SMeterB ?? 0,
                    mode = _radioStateService.ModeB ?? "",
                    antenna = _radioStateService.AntennaB ?? "",
                    afGain = _radioStateService.AfGainB,
                    roofingFilter = _radioStateService.RoofingFilterB ?? ""
                },
                micGain = _radioStateService.MicGain,
                powerMeter = _radioStateService.PowerMeter ?? 0,
                swrMeter = _radioStateService.SWRMeter ?? 0,
                iddMeter = _radioStateService.IDDMeter ?? 0,
                vddMeter = _radioStateService.VDDMeter ?? 0,
                temperature = _radioStateService.Temperature ?? 0
            });
        }

        [HttpGet("status/init")]
        public IActionResult GetInitStatus()
        {
            return Ok(new { status = AppStatus.InitializationStatus });
        }

        [HttpPost("frequency/a")]
        public async Task<IActionResult> SetFrequencyA([FromBody] FrequencyRequest request)
        {
            if (!await _requestSemaphore.WaitAsync(2000))
                return StatusCode(503, new { error = "Radio busy" });

            _logger.LogInformation("[API] SetFrequencyA called: freq={Freq}", request.FrequencyHz);
            try
            {
                await EnsureConnectedAsync();
                var freq = request.FrequencyHz;
                if (freq < 30000 || freq > 75000000)
                    return BadRequest(new { error = "Frequency out of range" });

                var command = $"FA{freq:D9};";
                await _catClient.SendCommandAsync(command, "WebUI", CancellationToken.None);

                _radioStateService.FrequencyA = freq;

                _logger.LogInformation("Set Receiver A frequency to {Freq}", freq);
                _logger.LogInformation("[API] SetFrequencyA completed: freq={Freq}", freq);
                return Ok(new { message = $"Frequency {freq} Hz set for Receiver A" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Receiver A frequency");
                return StatusCode(500, new { error = "Failed to set frequency" });
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }

        [HttpPost("frequency/b")]
        public async Task<IActionResult> SetFrequencyB([FromBody] FrequencyRequest request)
        {
            if (!await _requestSemaphore.WaitAsync(2000))
                return StatusCode(503, new { error = "Radio busy" });

            _logger.LogInformation("[API] SetFrequencyB called: freq={Freq}", request.FrequencyHz);
            try
            {
                await EnsureConnectedAsync();
                var freq = request.FrequencyHz;
                if (freq < 30000 || freq > 75000000)
                    return BadRequest(new { error = "Frequency out of range" });

                var command = $"FB{freq:D9};";
                await _catClient.SendCommandAsync(command, "WebUI", CancellationToken.None);

                _radioStateService.FrequencyB = freq;

                _logger.LogInformation("Set Receiver B frequency to {Freq}", freq);
                _logger.LogInformation("[API] SetFrequencyB completed: freq={Freq}", freq);
                return Ok(new { message = $"Frequency {freq} Hz set for Receiver B" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Receiver B frequency");
                return StatusCode(500, new { error = "Failed to set frequency" });
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }

        [HttpPost("band/a")]
        public async Task<IActionResult> SetBandA([FromBody] BandRequest request)
        {
            if (!await _requestSemaphore.WaitAsync(2000))
                return StatusCode(503, new { error = "Radio busy" });

            _logger.LogInformation("[API] SetBandA called: band={Band}", request.Band);
            try
            {
                await EnsureConnectedAsync();

                if (!BandFreqs.TryGetValue(request.Band, out var freq))
                    return BadRequest(new { error = "Invalid band" });

                var command = $"FA{freq:D9};";
                await _catClient.SendCommandAsync(command, "WebUI", CancellationToken.None);

                var actualFreq = await _catClient.QueryFrequencyAAsync("WebUI", CancellationToken.None);

                _radioStateService.SetBand("A", request.Band);
                _radioStateService.FrequencyA = actualFreq;

                _logger.LogInformation("Set Receiver A band to {Band} (freq {Freq})", request.Band, actualFreq);
                _logger.LogInformation("[API] SetBandA completed: band={Band}, freq={Freq}", request.Band, actualFreq);
                return Ok(new { message = $"Band {request.Band} selected", frequency = actualFreq });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Receiver A band");
                return StatusCode(500, new { error = "Failed to set band" });
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }

        [HttpPost("band/b")]
        public async Task<IActionResult> SetBandB([FromBody] BandRequest request)
        {
            if (!await _requestSemaphore.WaitAsync(2000))
                return StatusCode(503, new { error = "Radio busy" });

            _logger.LogInformation("[API] SetBandB called: band={Band}", request.Band);
            try
            {
                await EnsureConnectedAsync();

                if (!BandFreqs.TryGetValue(request.Band, out var freq))
                    return BadRequest(new { error = "Invalid band" });

                var command = $"FB{freq:D9};";
                await _catClient.SendCommandAsync(command, "WebUI", CancellationToken.None);

                var actualFreq = await _catClient.QueryFrequencyBAsync("WebUI", CancellationToken.None);

                _radioStateService.SetBand("B", request.Band);
                _radioStateService.FrequencyB = actualFreq;

                _logger.LogInformation("Set Receiver B band to {Band} (freq {Freq})", request.Band, actualFreq);
                _logger.LogInformation("[API] SetBandB completed: band={Band}, freq={Freq}", request.Band, actualFreq);
                return Ok(new { message = $"Band {request.Band} selected", frequency = actualFreq });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Receiver B band");
                return StatusCode(500, new { error = "Failed to set band" });
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }

        [HttpPost("antenna/a")]
        public async Task<IActionResult> SetAntennaA([FromBody] AntennaRequest request)
        {
            if (!await _requestSemaphore.WaitAsync(2000))
            {
                return StatusCode(503, new { error = "Radio busy" });
            }

            try
            {
                await EnsureConnectedAsync();
                var command = $"AN0{request.Antenna};";
                await _catClient.SendCommandAsync(command, "WebUI", CancellationToken.None);

                _radioStateService.AntennaA = request.Antenna;

                _logger.LogInformation("Set Main antenna to {Antenna}", request.Antenna);
                return Ok(new { message = $"Antenna {request.Antenna} selected" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Main antenna");
                return StatusCode(500, new { error = "Failed to set antenna" });
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }

        [HttpPost("antenna/b")]
        public async Task<IActionResult> SetAntennaB([FromBody] AntennaRequest request)
        {
            if (!await _requestSemaphore.WaitAsync(2000))
            {
                return StatusCode(503, new { error = "Radio busy" });
            }

            try
            {
                await EnsureConnectedAsync();
                var command = $"AN1{request.Antenna};";
                await _catClient.SendCommandAsync(command, "WebUI", CancellationToken.None);

                _radioStateService.AntennaB = request.Antenna;

                _logger.LogInformation("Set Sub antenna to {Antenna}", request.Antenna);
                return Ok(new { message = $"Antenna {request.Antenna} selected" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Sub antenna");
                return StatusCode(500, new { error = "Failed to set antenna" });
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }

        // Roofing filter display names (response code -> display name)
        private static readonly Dictionary<string, string> RoofingFilterNames = new()
        {
            { "6", "12 kHz" },
            { "7", "3 kHz" },
            { "8", "1.2 kHz" },
            { "9", "600 Hz" },
            { "A", "300 Hz" }
        };

        // Roofing filter set codes (response code -> set code)
        private static readonly Dictionary<string, string> RoofingFilterSetCodes = new()
        {
            { "6", "1" },  // 12 kHz
            { "7", "2" },  // 3 kHz
            { "8", "3" },  // 1.2 kHz (option)
            { "9", "4" },  // 600 Hz
            { "A", "5" }   // 300 Hz (option)
        };

        [HttpPost("roofingfilter/a")]
        public async Task<IActionResult> SetRoofingFilterA([FromBody] RoofingFilterRequest request)
        {
            if (!await _requestSemaphore.WaitAsync(2000))
            {
                return StatusCode(503, new { error = "Radio busy" });
            }

            try
            {
                await EnsureConnectedAsync();

                // Convert response code (6-A) to set code (1-5)
                if (!RoofingFilterSetCodes.TryGetValue(request.Filter, out var setCode))
                {
                    return BadRequest(new { error = $"Invalid filter code: {request.Filter}" });
                }

                var command = $"RF0{setCode};";
                _logger.LogInformation("Sending roofing filter command: {Command}", command);
                await _catClient.SendCommandAsync(command, "WebUI", CancellationToken.None);

                // Small delay to let the radio process the command
                await Task.Delay(100);

                // Check if the filter was actually set by reading back
                var readCommand = "RF0;";
                var readResponse = await _catClient.SendCommandAsync(readCommand, "WebUI", CancellationToken.None);
                _logger.LogInformation("Read back roofing filter response: {Response}", readResponse);

                // Parse response - format: RF0X; where X is filter code
                if (!string.IsNullOrEmpty(readResponse) && readResponse.Length >= 4)
                {
                    var actualFilter = readResponse[3].ToString();
                    _radioStateService.RoofingFilterA = actualFilter; // Always update to actual value

                    if (actualFilter != request.Filter)
                    {
                        // Filter doesn't exist or wasn't set
                        var requestedName = RoofingFilterNames.GetValueOrDefault(request.Filter, request.Filter);
                        var actualName = RoofingFilterNames.GetValueOrDefault(actualFilter, actualFilter);
                        _logger.LogWarning("Roofing filter {Requested} not available, radio returned {Actual}", 
                            requestedName, actualName);
                        return Ok(new { 
                            message = $"Filter {requestedName} not installed. Using {actualName}.",
                            warning = true,
                            filter = actualFilter,
                            filterName = actualName
                        });
                    }

                    var filterName = RoofingFilterNames.GetValueOrDefault(actualFilter, actualFilter);
                    _logger.LogInformation("Set Main roofing filter to {Filter}", filterName);
                    return Ok(new { message = $"Roofing filter {filterName} selected", filter = actualFilter, filterName });
                }

                // Fallback if read failed
                _radioStateService.RoofingFilterA = request.Filter;
                var fallbackName = RoofingFilterNames.GetValueOrDefault(request.Filter, request.Filter);
                return Ok(new { message = $"Roofing filter {fallbackName} selected", filter = request.Filter, filterName = fallbackName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Main roofing filter");
                return StatusCode(500, new { error = "Failed to set roofing filter" });
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }

        [HttpPost("roofingfilter/b")]
        public async Task<IActionResult> SetRoofingFilterB([FromBody] RoofingFilterRequest request)
        {
            if (!await _requestSemaphore.WaitAsync(2000))
            {
                return StatusCode(503, new { error = "Radio busy" });
            }

            try
            {
                await EnsureConnectedAsync();

                // Convert response code (6-A) to set code (1-5)
                if (!RoofingFilterSetCodes.TryGetValue(request.Filter, out var setCode))
                {
                    return BadRequest(new { error = $"Invalid filter code: {request.Filter}" });
                }

                var command = $"RF1{setCode};";
                _logger.LogInformation("Sending roofing filter command: {Command}", command);
                await _catClient.SendCommandAsync(command, "WebUI", CancellationToken.None);

                // Small delay to let the radio process the command
                await Task.Delay(100);

                // Check if the filter was actually set by reading back
                var readCommand = "RF1;";
                var readResponse = await _catClient.SendCommandAsync(readCommand, "WebUI", CancellationToken.None);
                _logger.LogInformation("Read back roofing filter response: {Response}", readResponse);

                // Parse response - format: RF1X; where X is filter code
                if (!string.IsNullOrEmpty(readResponse) && readResponse.Length >= 4)
                {
                    var actualFilter = readResponse[3].ToString();
                    _radioStateService.RoofingFilterB = actualFilter; // Always update to actual value

                    if (actualFilter != request.Filter)
                    {
                        // Filter doesn't exist or wasn't set
                        var requestedName = RoofingFilterNames.GetValueOrDefault(request.Filter, request.Filter);
                        var actualName = RoofingFilterNames.GetValueOrDefault(actualFilter, actualFilter);
                        _logger.LogWarning("Roofing filter {Requested} not available, radio returned {Actual}", 
                            requestedName, actualName);
                        return Ok(new { 
                            message = $"Filter {requestedName} not installed. Using {actualName}.",
                            warning = true,
                            filter = actualFilter,
                            filterName = actualName
                        });
                    }

                    var filterName = RoofingFilterNames.GetValueOrDefault(actualFilter, actualFilter);
                    _logger.LogInformation("Set Sub roofing filter to {Filter}", filterName);
                    return Ok(new { message = $"Roofing filter {filterName} selected", filter = actualFilter, filterName });
                }

                // Fallback if read failed
                _radioStateService.RoofingFilterB = request.Filter;
                var fallbackName = RoofingFilterNames.GetValueOrDefault(request.Filter, request.Filter);
                return Ok(new { message = $"Roofing filter {fallbackName} selected", filter = request.Filter, filterName = fallbackName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Sub roofing filter");
                return StatusCode(500, new { error = "Failed to set roofing filter" });
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }

        [HttpPost("mode/{receiver}")]
        public async Task<IActionResult> SetMode(string receiver, [FromBody] ModeRequest request)
        {
            if (!await _requestSemaphore.WaitAsync(2000))
                return StatusCode(503, new { error = "Radio busy" });

            try
            {
                await EnsureConnectedAsync();
                string displayMode = CatCodeToMode.TryGetValue(request.Mode, out var modeName) ? modeName : request.Mode;

                if (receiver.ToUpper() == "A")
                {
                    await _catClient.SendCommandAsync($"MD0{request.Mode};", "User");
                    _radioStateService.ModeA = displayMode;
                }
                else if (receiver.ToUpper() == "B")
                {
                    await _catClient.SendCommandAsync($"MD1{request.Mode};", "User");
                    _radioStateService.ModeB = displayMode;
                }
                else
                {
                    return BadRequest(new { error = "Invalid receiver specified" });
                }

                _logger.LogInformation("Sending CAT command: MD{Vfo}{Mode}; for Receiver {Receiver}", receiver.ToUpper() == "A" ? "0" : "1", request.Mode, receiver);
                return Ok(new { message = $"Mode {displayMode} selected for Receiver {receiver}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Receiver {Receiver} mode", receiver);
                return StatusCode(500, new { error = "Failed to set mode" });
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }

        [HttpPost("power/{receiver}")]
        public async Task<IActionResult> SetPower(string receiver, [FromBody] PowerRequest request)
        {
            _logger.LogInformation("[Slider][CAT] SetPower endpoint called: receiver={Receiver}, watts={Watts}", receiver, request.Watts);
            if (!await _requestSemaphore.WaitAsync(2000))
                return StatusCode(503, new { error = "Radio busy" });

            try
            {
                await EnsureConnectedAsync();

                var settings = await _settingsService.GetSettingsAsync();
                int maxPower = settings.RadioModel == "FTdx101MP" ? 200 : 100;

                _logger.LogInformation("[API] Received SetPower request: receiver={Receiver}, Watts={Watts}, Model={Model}", receiver, request.Watts, settings.RadioModel);
                _logger.LogInformation("[API] DEBUG: Received slider value = {Watts}", request.Watts);

                if (request.Watts < 5 || request.Watts > maxPower)
                    return BadRequest(new { error = $"Power out of range (5-{maxPower}W for {settings.RadioModel})" });

                var command = $"PC{request.Watts:D3};";
                _logger.LogInformation("[API] Sending CAT command: {Command}", command);
                await _catClient.SendCommandAsync(command, "WebUI", CancellationToken.None);
                // Immediately send PC; to read back power
                var readResponse = await _catClient.SendCommandAsync("PC;", "WebUI", CancellationToken.None);
                int actualPower = ParsePower(readResponse ?? "");
                _logger.LogInformation("[Slider][CAT] Sent PC command: watts={Watts}, readback={Readback}, actualPower={ActualPower}", request.Watts, readResponse, actualPower);

                _logger.LogInformation("[API] Setting Power to {ActualPower}", actualPower);
                _radioStateService.Power = actualPower;

                _logger.LogInformation("[API] Power set to {Power}W on {RadioModel}", actualPower, settings.RadioModel);
                return Ok(new { message = $"Power set to {actualPower}W", maxPower = maxPower });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting power");
                return StatusCode(500, new { error = "Failed to set power" });
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }

        // Add this helper method (put it near other helper methods like ParseSMeter)
        private int ParsePower(string response)
        {
            // Response format: PC123; (3 digits for watts)
            if (response.Length >= 5 && response.StartsWith("PC"))
            {
                if (int.TryParse(response.Substring(2, 3), out int watts))
                {
                    return watts;
                }
            }
            return 100; // Default to 100W if can't parse
        }

        [HttpPost("afgain")]
        public async Task<IActionResult> SetAfGain([FromBody] AfGainRequest request)
        {
            if (!await _requestSemaphore.WaitAsync(2000))
                return StatusCode(503, new { error = "Radio busy" });

            try
            {
                await EnsureConnectedAsync();
                if (request == null || (request.Band != "0" && request.Band != "1"))
                    return BadRequest(new { error = "Invalid band (must be '0' or '1')" });
                if (!int.TryParse(request.Value, out int val) || val < 0 || val > 255)
                    return BadRequest(new { error = "AF Gain value out of range (0-255)" });

                string command = $"AG{request.Band}{val:D3};";
                await _catClient.SendCommandAsync(command, "WebUI", CancellationToken.None);
                // Persist AF Gain value
                if (request.Band == "0")
                    _radioStateService.AfGainA = val;
                else if (request.Band == "1")
                    _radioStateService.AfGainB = val;
                _logger.LogInformation("Set AF Gain band {Band} to {Value}", request.Band, val);
                return Ok(new { message = $"AF Gain set to {val} for band {request.Band}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting AF Gain");
                return StatusCode(500, new { error = "Failed to set AF Gain" });
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }

        public class BandRequest { public string Band { get; set; } = string.Empty; }
        public class AntennaRequest { public string Antenna { get; set; } = string.Empty; }
        public class ModeRequest { public string Mode { get; set; } = string.Empty; }
        public class FrequencyRequest { public long FrequencyHz { get; set; } }
        public class PowerRequest
        {
            public int Watts { get; set; }
        }

        public class MicGainRequest
        {
            public int Value { get; set; }
        }

        public class AfGainRequest
        {
            public string Band { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
        }

        public class RadioPowerRequest
        {
            public bool PowerOn { get; set; }
        }

        public class TxRequest
        {
            public bool Transmit { get; set; }
        }

        public class RoofingFilterRequest 
        { 
            public string Filter { get; set; } = string.Empty; 
        }

        [HttpPost("reinitialize")]
        public async Task<IActionResult> Reinitialize()
        {
            try
            {
                _logger.LogInformation("Manual re-initialization requested from Settings page");
                await _radioInitService.InitializeRadioAsync();

                // Update status to complete so Index page polling sees it
                AppStatus.InitializationStatus = "complete";

                return Ok(new { success = true, message = "Radio connected successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manual re-initialization failed");
                AppStatus.InitializationStatus = "error";
                return Ok(new { success = false, message = ex.Message });
            }
        }
    }
}
