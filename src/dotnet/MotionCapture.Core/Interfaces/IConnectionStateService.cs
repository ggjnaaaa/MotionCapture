using MotionCapture.Core.Enums;

namespace MotionCapture.Core.Interfaces;

public interface IConnectionStateService
{
    event Action<ConnectionState, string>? StateChanged;
    bool IsConnected { get; }
    void SetConnected();
    void SetServerUnavailable(string message);
    void SetTimeout(string message);
    void SetGenericError(string message);
    Task<bool> ForceCheckAsync();
}