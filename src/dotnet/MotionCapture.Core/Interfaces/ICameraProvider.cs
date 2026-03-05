using MotionCapture.Core.Models;

namespace MotionCapture.Core.Interfaces;

public interface ICameraProvider
{
    public void MarkBusy(int index);
    public void MarkFree(int index);
    IReadOnlyList<CameraInfo> GetAvailableCameras();
}
