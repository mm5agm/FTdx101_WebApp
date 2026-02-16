using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FTdx101_WebApp.Services;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace FTdx101_WebApp.Pages
{
    // Add this at the top or in a suitable namespace
    public class RadioStateViewModel
    {
        public VfoViewModel vfoA { get; set; } = new VfoViewModel();
        public VfoViewModel vfoB { get; set; } = new VfoViewModel();
    }

    public class VfoViewModel
    {
        public long frequency { get; set; }
        public string band { get; set; } = string.Empty;
        public int sMeter { get; set; }
        public int power { get; set; } // <-- KEEP this property for VFO A usage
        public string mode { get; set; } = string.Empty;
        public string antenna { get; set; } = string.Empty;
    }

    public class IndexModel : PageModel
    {
        private readonly RadioStateService _radioStateService;

        public IndexModel(RadioStateService radioStateService)
        {
            _radioStateService = radioStateService;
        }

        public string SelectedBandA { get; set; } = string.Empty;
        public string SelectedBandB { get; set; } = string.Empty;

       

        public RadioStateService RadioState => _radioStateService;

        public RadioStateViewModel State { get; set; } = new RadioStateViewModel();

        public async Task<IActionResult> OnGetAsync()
        {
            if (FTdx101_WebApp.Services.AppStatus.InitializationStatus == "error")
            {
                return RedirectToPage("/Settings");
            }

            // VFO A (keep as is)
            State.vfoA.frequency = _radioStateService.FrequencyA;
            State.vfoA.band = _radioStateService.BandA;
            State.vfoA.sMeter = _radioStateService.SMeterA ?? 0;
            State.vfoA.power = _radioStateService.PowerA;
            State.vfoA.mode = _radioStateService.ModeA ?? "";
            State.vfoA.antenna = _radioStateService.AntennaA ?? "";

            // VFO B (remove power assignment)
            State.vfoB.frequency = _radioStateService.FrequencyB;
            State.vfoB.band = _radioStateService.BandB;
            State.vfoB.sMeter = _radioStateService.SMeterB ?? 0;
            State.vfoB.mode = _radioStateService.ModeB ?? "";
            State.vfoB.antenna = _radioStateService.AntennaB ?? "";

            return Page(); // <-- Add this line
        }
    }
}