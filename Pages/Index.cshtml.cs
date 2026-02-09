using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FTdx101_WebApp.Services;

namespace FTdx101_WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly RadioStateService _radioStateService;

        public IndexModel(RadioStateService radioStateService)
        {
            _radioStateService = radioStateService;
        }

        public string SelectedBandA { get; set; } = string.Empty;
        public string SelectedBandB { get; set; } = string.Empty;

        public void OnGet()
        {
            // Ensure bands are updated based on current frequencies
            _radioStateService.UpdateBandFromFrequency();
            SelectedBandA = _radioStateService.BandA ?? "20m";
            SelectedBandB = _radioStateService.BandB ?? "20m";
        }

        public RadioStateService RadioState => _radioStateService;
    }
}
