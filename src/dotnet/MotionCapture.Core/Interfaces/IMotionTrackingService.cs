using Emgu.CV;
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
    /// <param name="fps"></param>
    void SetTargetFps(int fps);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cameraIndex"></param>
    void RegisterCamera(int cameraIndex);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cameraIndex"></param>
    void UnregisterCamera(int cameraIndex);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="frame"></param>
    /// <param name="cameraIndex"></param>
    void SubmitFrame(Mat frame, int cameraIndex);
}
