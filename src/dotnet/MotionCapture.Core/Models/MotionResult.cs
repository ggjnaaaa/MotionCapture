namespace MotionCapture.Core.Models;

public class MotionResult
{
    public required IEnumerable<JointPosition3D> Joints { get; set; }
    public required IEnumerable<FrameLandmarks2D> FramesToDraw { get; set; }
}
