namespace MotionCapture.Core.Models;

public class Skeleton
{
    public List<SkeletonJoint> Joints { get; set; } = new();
    public long TimestampMs { get; set; }
}
