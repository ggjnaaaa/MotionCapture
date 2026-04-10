using Emgu.CV;
using MotionCapture.Core.Models;

namespace MotionCapture.Core.Interfaces;

public interface IFrameSyncBuffer
{
    event Func<List<CameraFrame>, IReadOnlyDictionary<int, Mat>, Task>? FramesReadyAsync;
    void SetFps(int fps);
    void RegisterCamera(int cameraIndex);
    void UnregisterCamera(int cameraIndex);
    void SubmitFrame(Mat frame, int cameraIndex);
}