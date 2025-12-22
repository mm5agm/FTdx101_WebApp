using FTdx101_WebApp.Models;

namespace FTdx101_WebApp.Services
{
    public interface IRigStateService
    {
        RigState CurrentState { get; }
        event EventHandler<RigState>? StateChanged;
        void UpdateState(RigState newState);
    }
}