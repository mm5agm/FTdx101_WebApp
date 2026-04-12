// FTdx101 WebApp – SDR REST Controller
// Provides device enumeration for the Settings page dropdown.
// No UI logic, no SignalR, no calibration.
//
// Response shape: { devices: [...], notes: ["..."] }
// 'notes' contains plain-English installation guidance when drivers are missing.

using FTdx101_WebApp.Services.Sdr;
using Microsoft.AspNetCore.Mvc;

namespace FTdx101_WebApp.Controllers
{
    [ApiController]
    [Route("api/sdr")]
    public class SdrController : ControllerBase
    {
        private readonly ILogger<SdrController> _logger;

        public SdrController(ILogger<SdrController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Returns connected SDR devices plus plain-English installation notes.
        /// Always responds 200.
        /// </summary>
        [HttpGet("devices")]
        public IActionResult GetDevices()
        {
            var all   = new List<SdrDeviceInfo>();
            var notes = new List<string>();

            // ── SDRplay (sdrplay_api.dll) ────────────────────────────────────────
            try
            {
                var sdrplay = SdrplayDevice.EnumerateDevices();
                if (sdrplay.Count > 0)
                {
                    all.AddRange(sdrplay);
                    _logger.LogDebug("SDR: Found {Count} SDRplay device(s)", sdrplay.Count);
                }
                else
                {
                    // DLL loaded but enumeration found nothing — likely the API service
                    // is not running or no device is connected.
                    notes.Add("SDRplay API loaded but no devices found. " +
                              "Ensure the SDRplay RSP is plugged in and the " +
                              "SDRplay API Service is running.");
                }
            }
            catch (DllNotFoundException)
            {
                notes.Add("sdrplay_api.dll not found. " +
                          "Download and install the official SDRplay API from " +
                          "www.sdrplay.com/softwaredownloads/ — this also enables " +
                          "SoapySDR support for SDRplay devices via SoapySDRPlay3.");
                _logger.LogDebug("SDR: sdrplay_api.dll not present");
            }
            catch (Exception ex)
            {
                notes.Add($"SDRplay API error: {ex.Message}");
                _logger.LogWarning(ex, "SDR: SDRplay enumeration failed");
            }

            // ── SoapySDR (SoapySDR.dll) ──────────────────────────────────────────
            try
            {
                var soapy = SoapySdrInterop.EnumerateDevices();
                if (soapy.Count > 0)
                {
                    all.AddRange(soapy);
                    _logger.LogDebug("SDR: Found {Count} SoapySDR device(s)", soapy.Count);
                }
                else
                {
                    // DLL found but no devices — the device isn't plugged in or the
                    // device-specific SoapySDR plugin (e.g. SoapySDRPlay3) is missing.
                    notes.Add("SoapySDR loaded but no devices found. " +
                              "Check your SDR is plugged in and the device-specific " +
                              "SoapySDR plugin is installed (e.g. SoapySDRPlay3 for SDRplay).");
                }
            }
            catch (DllNotFoundException)
            {
                notes.Add("SoapySDR.dll not found. " +
                          "SoapySDR enables support for RTL-SDR, SDRplay, Airspy, HackRF, " +
                          "and 20+ other devices. Install it via CubicSDR or SDRangel " +
                          "(both include SoapySDR and device plugins in their Windows installers).");
                _logger.LogDebug("SDR: SoapySDR.dll not present");
            }
            catch (Exception ex)
            {
                notes.Add($"SoapySDR error: {ex.Message}");
                _logger.LogWarning(ex, "SDR: SoapySDR enumeration failed");
            }

            return Ok(new
            {
                devices = all.Select(d => new { d.Key, d.Label, d.Driver }),
                notes
            });
        }
    }
}
