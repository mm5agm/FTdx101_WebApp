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

        public RadioStateService RadioState => _radioStateService;
    }


}