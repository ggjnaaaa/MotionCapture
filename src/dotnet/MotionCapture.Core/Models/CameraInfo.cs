namespace MotionCapture.Core.Models;

public class CameraInfo
{
    public int Index { get; init; }
    public string Name { get; init; } = String.Empty;
    public bool IsBusy { get; set; }
}
