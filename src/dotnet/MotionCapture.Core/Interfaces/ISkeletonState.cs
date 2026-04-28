using MotionCapture.Core.Models;

namespace MotionCapture.Core.Interfaces;

public interface ISkeletonState
{
    Task<Skeleton> WaitForNextAsync(CancellationToken ct);
    void Publish(Skeleton skeleton);
}
