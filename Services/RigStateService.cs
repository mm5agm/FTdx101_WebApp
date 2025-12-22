using FTdx101_WebApp.Models;

namespace FTdx101_WebApp.Services
{
    public class RigStateService : IRigStateService
    {
        private RigState _currentState = new();
        private readonly object _lock = new();

        public RigState CurrentState
        {
            get
            {
                lock (_lock)
                {
                    return new RigState
                    {
                        SMeterLevel = _currentState.SMeterLevel,
                        Frequency = _currentState.Frequency,
                        Mode = _currentState.Mode,
                        IsTransmitting = _currentState.IsTransmitting,
                        LastUpdated = _currentState.LastUpdated
                    };
                }
            }
        }

        public event EventHandler<RigState>? StateChanged;

        public void UpdateState(RigState newState)
        {
            lock (_lock)
            {
                _currentState = newState;
                _currentState.LastUpdated = DateTime.UtcNow;
            }

            StateChanged?.Invoke(this, CurrentState);
        }
    }
}