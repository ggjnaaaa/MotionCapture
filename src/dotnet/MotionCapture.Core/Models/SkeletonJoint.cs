using System.Numerics;

namespace MotionCapture.Core.Models;

public class SkeletonJoint
{
    public string Name { get; set; } = default!;
    public int ParentIndex { get; set; }

    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
}
