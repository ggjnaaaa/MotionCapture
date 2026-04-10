using Emgu.CV;

namespace MotionCapture.Core.Interfaces;

public interface ICameraCaptureService
{
    event Action<int, Mat> FrameArrived;

    void Start(int index);
    void Stop();
}
