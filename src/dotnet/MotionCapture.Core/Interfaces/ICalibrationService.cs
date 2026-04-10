using MotionCapture.Core.Models;

namespace MotionCapture.Core.Interfaces;

public interface ICalibrationService
{
    event Action<float, IEnumerable<FrameLandmarks2D>?, bool?>? CalibrationProgressChanged;
    Task StartCalibrationAsync(CancellationToken token = default);
    Task ProcessAsync(List<CameraFrame> cameraFrame);
    Task StopCalibrationAsync();
}