namespace MotionCapture.Infrastructure.Grpc.Repositories;

public interface ICameraGrpcClient
{
    Task<bool> ChangeCameraIndexAsync(int? previousIndex, int newIndex);
    Task<bool> RemoveCamerasAsync();
    Task<bool> AddCameraAsync(int newIndex);
}