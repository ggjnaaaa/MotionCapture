using MotionCapture.Core.Models;

namespace MotionCapture.Core.Interfaces;

public interface IMotionTrackingService
{
    /// <summary>
    /// Выдаёт MotionFrame с позами суставов
    /// </summary>
    event Action<MotionResult?>? FrameProcessed;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="frame"></param>
    /// <param name="cameraIndex"></param>
    Task SubmitFrameAsync(List<CameraFrame> cameraFrames, CancellationToken ct = default);
}
