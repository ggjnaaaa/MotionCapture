using MotionCapture.Core.Models;

namespace MotionCapture.Infrastructure.Grpc.Repositories;

public interface IMotionGrpcClient
{
    Task<MotionResult?> ProcessAsync(IEnumerable<CameraFrame> batch, CancellationToken ct);
    bool ChangeCameraIndex(int? previousIndex, int newIndex);
    bool RemoveCameras();
    bool AddCamera(int newIndex);
}