using System.Numerics;

namespace MotionCapture.Core.Models;

public class JointPose
{
    public string Name { get; init; } = String.Empty;
    public Vector3 Position { get; init; }
    public Quaternion Rotation { get; init; }
}
