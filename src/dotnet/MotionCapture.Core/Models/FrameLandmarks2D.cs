namespace MotionCapture.Core.Models;

public class FrameLandmarks2D
{
    public required IEnumerable<Joint2D> Joint2D { get; set; }
    public int SourceCameraIndex { get; set; }
    public long Timestamp { get; set; }
}
