using FTdx101_WebApp.Models;

namespace FTdx101_WebApp.Services
{
    public interface ISettingsService
    {
        Task<ApplicationSettings> GetSettingsAsync();
        Task SaveSettingsAsync(ApplicationSettings settings);
    }
}