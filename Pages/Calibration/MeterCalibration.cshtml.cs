using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FTdx101_WebApp.Models.Calibration;
using System.Collections.Generic;

namespace FTdx101_WebApp.Pages.Calibration
{
    public class MeterCalibrationModel : PageModel
    {
        [BindProperty]
        public FTdx101_WebApp.Models.Calibration.MeterCalibration Calibration { get; set; } = new FTdx101_WebApp.Models.Calibration.MeterCalibration
        {
            Name = "S-Meter",
            Type = "s_meter",
            IsGauge = true,
            Units = "S-units",
            Points = new List<CalibrationPoint>()
        };

        [BindProperty]
        public double CurrentRawValue { get; set; } = 0;

        public void OnGet()
        {
            // TODO: Load existing calibration data if available
        }

        public IActionResult OnPost(string add, string delete)
        {
            if (!string.IsNullOrEmpty(add))
            {
                Calibration.Points.Add(new CalibrationPoint { Label = "", Raw = 0 });
            }
            if (!string.IsNullOrEmpty(delete) && int.TryParse(delete, out int idx) && idx >= 0 && idx < Calibration.Points.Count)
            {
                Calibration.Points.RemoveAt(idx);
            }
            // TODO: Save logic would go here
            return Page();
        }
    }
}
