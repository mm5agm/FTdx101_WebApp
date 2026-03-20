using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FTdx101_WebApp.Models;
using FTdx101_WebApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FTdx101_WebApp.Pages
{
    public class CalibrationModel : PageModel
    {
        private readonly CalibrationService _calibrationService;
        private readonly ILogger<CalibrationModel> _logger;

        [BindProperty]
        public CalibrationSettings Calibration { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public CalibrationModel(CalibrationService calibrationService, ILogger<CalibrationModel> logger)
        {
            _calibrationService = calibrationService;
            _logger = logger;
        }

        private void RemapCalibrationPointIds(string prefix, List<CalibrationPoint> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                var key = $"{prefix}[{i}].Id";
                if (Request.Form.ContainsKey(key))
                {
                    var idStr = Request.Form[key];
                    if (Guid.TryParse(idStr, out var id))
                        points[i].Id = id;
                }
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Calibration = await _calibrationService.GetCalibrationAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            RemapCalibrationPointIds("Calibration.SMeter.Points", Calibration.SMeter.Points);
            RemapCalibrationPointIds("Calibration.SWR.Points", Calibration.SWR.Points);
            RemapCalibrationPointIds("Calibration.Power.Points", Calibration.Power.Points);
            RemapCalibrationPointIds("Calibration.ALC.Points", Calibration.ALC.Points);

            if (Request.Form.ContainsKey("AddSMeterPoint"))
                Calibration.SMeter.Points.Add(new CalibrationPoint { Id = Guid.NewGuid() });
            if (Request.Form.ContainsKey("AddSWRPoint"))
                Calibration.SWR.Points.Add(new CalibrationPoint { Id = Guid.NewGuid() });
            if (Request.Form.ContainsKey("AddPowerPoint"))
                Calibration.Power.Points.Add(new CalibrationPoint { Id = Guid.NewGuid() });
            if (Request.Form.ContainsKey("AddALCPoint"))
                Calibration.ALC.Points.Add(new CalibrationPoint { Id = Guid.NewGuid() });

            foreach (var key in Request.Form.Keys.Where(k => k.StartsWith("RemoveSMeterPoint-")).ToList())
            {
                var idStr = key["RemoveSMeterPoint-".Length..];
                if (Guid.TryParse(idStr, out var id))
                    Calibration.SMeter.Points.RemoveAll(p => p.Id == id);
            }
            foreach (var key in Request.Form.Keys.Where(k => k.StartsWith("RemoveSWRPoint-")).ToList())
            {
                string idStr = key["RemoveSWRPoint-".Length..];
                if (!string.IsNullOrEmpty(idStr) && Guid.TryParse(idStr, out var id))
                    Calibration.SWR.Points.RemoveAll(p => p.Id == id);
            }
            foreach (var key in Request.Form.Keys.Where(k => k.StartsWith("RemovePowerPoint-")).ToList())
            {
                string idStr = key["RemovePowerPoint-".Length..];
                if (!string.IsNullOrEmpty(idStr) && Guid.TryParse(idStr, out var id))
                    Calibration.Power.Points.RemoveAll(p => p.Id == id);
            }
            foreach (var key in Request.Form.Keys.Where(k => k.StartsWith("RemoveALCPoint-")).ToList())
            {
                string idStr = key["RemoveALCPoint-".Length..];
                if (!string.IsNullOrEmpty(idStr) && Guid.TryParse(idStr, out var id))
                    Calibration.ALC.Points.RemoveAll(p => p.Id == id);
            }

            if (!ModelState.IsValid)
            {
                StatusMessage = "Invalid input.";
                return Page();
            }

            if (Request.Form["saveCalibrationBtn"].Count > 0)
            {
                await _calibrationService.SaveCalibrationAsync(Calibration);
                StatusMessage = "Calibration updated.";
            }
            return Page();
        }
    }
}
