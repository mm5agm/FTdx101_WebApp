using Microsoft.AspNetCore.Mvc;
using FTdx101_WebApp.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FTdx101_WebApp.Controllers
{
    [ApiController]
    [Route("api/calibration")]
    public class CalibrationController : ControllerBase
    {
        private readonly CalibrationService _calibrationService;

        public CalibrationController(CalibrationService calibrationService)
        {
            _calibrationService = calibrationService;
        }

        // GET: /api/calibration/smeter
        [HttpGet("smeter")]
        public async Task<IActionResult> GetSMeterCalibration()
        {
            var settings = await _calibrationService.GetCalibrationAsync();
            var points = settings.SMeter?.Points
                ?.Where(p => !string.IsNullOrWhiteSpace(p.SPoint) && !string.IsNullOrWhiteSpace(p.RawValue))
                .Select(p => new { label = p.SPoint, value = double.TryParse(p.RawValue, out var v) ? v : 0 })
                .OrderBy(p => p.value)
                .ToList();
            if (points == null)
                return Ok(new List<object>());
            return Ok(points);
        }

        // GET: /api/calibration/powerout
        [HttpGet("powerout")]
        public async Task<IActionResult> GetPowerOutCalibration()
        {
            var settings = await _calibrationService.GetCalibrationAsync();
            var points = settings.Power?.Points
                ?.Where(p => !string.IsNullOrWhiteSpace(p.Power) && !string.IsNullOrWhiteSpace(p.RawValue))
                .Select(p => new { label = p.Power, value = double.TryParse(p.RawValue, out var v) ? v : 0 })
                .OrderBy(p => p.value)
                .ToList();
            if (points == null)
                return Ok(new List<object>());
            return Ok(points);
        }
    }
}
