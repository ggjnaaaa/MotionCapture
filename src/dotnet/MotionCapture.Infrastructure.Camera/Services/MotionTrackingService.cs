using MotionCapture.Core.Interfaces;
using MotionCapture.Core.Models;
using MotionCapture.Infrastructure.GrpcClient.Repositories;
using System.Diagnostics;

namespace MotionCapture.Infrastructure.Camera.Services;

public class MotionTrackingService : IMotionTrackingService
{
    private readonly IMotionGrpcClient _grpcClient;

    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

    public event Action<MotionResult?>? FrameProcessed;

    public MotionTrackingService(IMotionGrpcClient grpcClient)
    {
        _grpcClient = grpcClient;
    }

    public async Task SubmitFrameAsync(List<CameraFrame> cameraFrames, CancellationToken ct = default)
    {
        if (cameraFrames == null || cameraFrames.Count == 0) return;

        await _processingSemaphore.WaitAsync(ct);
        try
        {
            var result = await _grpcClient.ProcessAsync(cameraFrames, ct);
            FrameProcessed?.Invoke(result);
        }
        finally
        {
            _processingSemaphore.Release();
        }

        return;
    }
}
