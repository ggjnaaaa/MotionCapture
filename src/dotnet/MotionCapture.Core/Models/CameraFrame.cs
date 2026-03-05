namespace MotionCapture.Core.Models;

public class CameraFrame
{
    public int CameraIndex { get; set; }
    public long Timestamp { get; set; }
    public required byte[] ImageBytes { get; set; }
}
