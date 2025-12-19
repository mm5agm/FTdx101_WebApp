using FTdx101MP_WebApp.Models;

namespace FTdx101MP_WebApp.Services
{
    public interface ISettingsService
    {
        Task<ApplicationSettings> GetSettingsAsync();
        Task SaveSettingsAsync(ApplicationSettings settings);
    }
}