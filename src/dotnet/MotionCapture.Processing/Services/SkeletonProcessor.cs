using MotionCapture.Core.Interfaces;
using MotionCapture.Core.Models;
using System.Numerics;

namespace MotionCapture.Processing.Services;

public class SkeletonProcessor : ISkeletonProcessor
{
    private readonly ISkeletonState _state;

    public SkeletonProcessor(ISkeletonState state)
    {
        _state = state;
    }

    public void Process(MotionResult motion)
    {
        List<SkeletonJoint> joints = new List<SkeletonJoint>();

        foreach (var joint in motion.Joints)
        {
            joints.Add(new SkeletonJoint()
            {
                Name = joint.Name,
                ParentIndex = joint.ParentIndex,
                Position = new Vector3(joint.PosX, joint.PosY, joint.PosZ)
            });
        }

        var skeleton = new Skeleton()
        {
            TimestampMs = motion.FramesToDraw.First().Timestamp,
            Joints = joints
        };

        _state.Publish(skeleton);
    }
}