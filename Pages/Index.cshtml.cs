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
            SelectedBandA = _radioStateService.BandA ?? "20m";
            SelectedBandB = _radioStateService.BandB ?? "20m";
        }

        public RadioStateService RadioState => _radioStateService;

        public IActionResult OnPostSetBand([FromBody] BandChangeRequest request)
        {
            _radioStateService.SetBand(request.receiver, request.band);
            var selectedBandA = _radioStateService.BandA ?? "20m";
            var selectedBandB = _radioStateService.BandB ?? "20m";
            var selectedBand = request.receiver == "A" ? selectedBandA : selectedBandB;
            return Partial("_BandButtonsPartial", Tuple.Create("unused", request.receiver, selectedBand));
        }

        public class BandChangeRequest
        {
            public string receiver { get; set; } = string.Empty;
            public string band { get; set; } = string.Empty;
        }
    }
}