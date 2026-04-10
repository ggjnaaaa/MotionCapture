using CommunityToolkit.Mvvm.ComponentModel;
using MotionCapture.Core.Interfaces;

namespace MotionCapture.Services;

public partial class ApplicationState : ObservableObject
{
    private readonly ICalibrationService calibrationService;

    [ObservableProperty]
    private bool _isCalibrating;

    [ObservableProperty]
    private bool _isSkeletonCaptureActive;

    public event Action? CalibrationStopped;

    public ApplicationState(ICalibrationService calibrationService, IConnectionStateService connectionStateService)
    {
        this.calibrationService = calibrationService;
        connectionStateService.StateChanged += ((_, __) => {
            if (!connectionStateService.IsConnected) StopAll();
        });
    }

    public void ToggleCalibration()
    {
        IsCalibrating = !IsCalibrating;

        if (IsCalibrating)
            IsSkeletonCaptureActive = false;
    }

    public void ToggleSkeletonCapture()
    {
        IsSkeletonCaptureActive = !IsSkeletonCaptureActive;

        if (IsSkeletonCaptureActive)
            IsCalibrating = false;
    }

    public void StopAll()
    {
        IsSkeletonCaptureActive = false;
        IsCalibrating = false;
    }

    partial void OnIsCalibratingChanged(bool value)
    {
        if (!value)
        {
            calibrationService.StopCalibrationAsync();
            CalibrationStopped?.Invoke();
        }

        if (value)
        {
            calibrationService.StartCalibrationAsync();
        }
    }
}
