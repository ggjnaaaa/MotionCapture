using MotionCapture.Core.Interfaces;
using MotionCapture.Core.Models;

namespace MotionCapture.Processing.Services;

public class SkeletonState : ISkeletonState
{
    private readonly object _lock = new();

    private TaskCompletionSource<Skeleton> _tcs =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task<Skeleton> WaitForNextAsync(CancellationToken ct)
    {
        lock (_lock)
        {
            return _tcs.Task.WaitAsync(ct);
        }
    }

    public void Publish(Skeleton skeleton)
    {
        TaskCompletionSource<Skeleton> toRelease;

        lock (_lock)
        {
            toRelease = _tcs;

            _tcs = new TaskCompletionSource<Skeleton>(
                TaskCreationOptions.RunContinuationsAsynchronously);
        }

        toRelease.TrySetResult(skeleton);
    }
}
