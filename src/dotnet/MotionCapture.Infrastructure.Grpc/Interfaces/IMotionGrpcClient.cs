using MotionCapture.Core.Models;

namespace MotionCapture.Infrastructure.GrpcClient.Repositories;

public interface IMotionGrpcClient
{
    Task<MotionResult?> ProcessAsync(IEnumerable<CameraFrame> batch, CancellationToken ct);
}