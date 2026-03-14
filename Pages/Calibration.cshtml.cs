using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FTdx101_WebApp.Models;
using FTdx101_WebApp.Services;

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

        // Ensures at least one CalibrationPoint exists in the meter
        private void EnsureAtLeastOnePoint(MeterCalibration meter)
        {
            if (meter.Points == null || meter.Points.Count == 0)
            {
                meter.Points = new List<CalibrationPoint> { new CalibrationPoint() };
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Calibration = await _calibrationService.GetCalibrationAsync();
            EnsureAtLeastOnePoint(Calibration.SMeter);
            EnsureAtLeastOnePoint(Calibration.SWR);
            EnsureAtLeastOnePoint(Calibration.Power);
            EnsureAtLeastOnePoint(Calibration.ALC);
            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            // Handle add/remove actions for each meter
            var form = Request.Form;
            // Add actions
            if (form.ContainsKey("AddSMeterPoint"))
                Calibration.SMeter.Points.Add(new CalibrationPoint());
            if (form.ContainsKey("AddSWRPoint"))
                Calibration.SWR.Points.Add(new CalibrationPoint());
            if (form.ContainsKey("AddPowerPoint"))
                Calibration.Power.Points.Add(new CalibrationPoint());
            if (form.ContainsKey("AddALCPoint"))
                Calibration.ALC.Points.Add(new CalibrationPoint());

            // Remove actions (look for RemoveSMeterPoint-#, etc)
            foreach (var key in form.Keys)
            {
                if (key.StartsWith("RemoveSMeterPoint-"))
                {
                    if (int.TryParse(key["RemoveSMeterPoint-".Length..], out int idx) && idx >= 0 && idx < Calibration.SMeter.Points.Count && Calibration.SMeter.Points.Count > 1)
                        Calibration.SMeter.Points.RemoveAt(idx);
                }
                if (key.StartsWith("RemoveSWRPoint-"))
                {
                    if (int.TryParse(key["RemoveSWRPoint-".Length..], out int idx) && idx >= 0 && idx < Calibration.SWR.Points.Count && Calibration.SWR.Points.Count > 1)
                        Calibration.SWR.Points.RemoveAt(idx);
                }
                if (key.StartsWith("RemovePowerPoint-"))
                {
                    if (int.TryParse(key["RemovePowerPoint-".Length..], out int idx) && idx >= 0 && idx < Calibration.Power.Points.Count && Calibration.Power.Points.Count > 1)
                        Calibration.Power.Points.RemoveAt(idx);
                }
                if (key.StartsWith("RemoveALCPoint-"))
                {
                    if (int.TryParse(key["RemoveALCPoint-".Length..], out int idx) && idx >= 0 && idx < Calibration.ALC.Points.Count && Calibration.ALC.Points.Count > 1)
                        Calibration.ALC.Points.RemoveAt(idx);
                }
            }

            // Always ensure at least one point exists for each meter
            EnsureAtLeastOnePoint(Calibration.SMeter);
            EnsureAtLeastOnePoint(Calibration.SWR);
            EnsureAtLeastOnePoint(Calibration.Power);
            EnsureAtLeastOnePoint(Calibration.ALC);

            // Remove actions (look for RemoveSMeterPoint-#, etc)
            foreach (var key in form.Keys)
            {
                if (key.StartsWith("RemoveSMeterPoint-"))
                {
                    if (int.TryParse(key["RemoveSMeterPoint-".Length..], out int idx) && idx >= 0 && idx < Calibration.SMeter.Points.Count)
                        Calibration.SMeter.Points.RemoveAt(idx);
                }
                if (key.StartsWith("RemoveSWRPoint-"))
                {
                    if (int.TryParse(key["RemoveSWRPoint-".Length..], out int idx) && idx >= 0 && idx < Calibration.SWR.Points.Count)
                        Calibration.SWR.Points.RemoveAt(idx);
                }
                if (key.StartsWith("RemovePowerPoint-"))
                {
                    if (int.TryParse(key["RemovePowerPoint-".Length..], out int idx) && idx >= 0 && idx < Calibration.Power.Points.Count)
                        Calibration.Power.Points.RemoveAt(idx);
                }
                if (key.StartsWith("RemoveALCPoint-"))
                {
                    if (int.TryParse(key["RemoveALCPoint-".Length..], out int idx) && idx >= 0 && idx < Calibration.ALC.Points.Count)
                        Calibration.ALC.Points.RemoveAt(idx);
                }
            }

            // If any add/remove, just redisplay page (don't save)
            if (form.Keys.Any(k => k.StartsWith("Add") || k.StartsWith("Remove")))
            {
                return Page();
            }

            if (!ModelState.IsValid)
            {
                StatusMessage = "Invalid input.";
                return Page();
            }
            // Optionally: trim empty trailing points (not required, but can be added)
            // Save as-is
            await _calibrationService.SaveCalibrationAsync(Calibration);
            StatusMessage = "Calibration settings saved.";
            return RedirectToPage();
        }


    }
}
