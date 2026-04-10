namespace MotionCapture.Infrastructure.Grpc.Options;

public static class GrpcClientNames
{
    public const string MotionService = "MotionCapture.Grpc.Contracts.Motion.MotionService+MotionServiceClient";
    public const string CalibrationService = "MotionCapture.Grpc.Contracts.Calibration.CalibrationService+CalibrationServiceClient";
    public const string CameraService = "MotionCapture.Grpc.Contracts.Camera.CameraService+CameraServiceClient";
    public const string HealthCheck = "MotionCapture.Grpc.Contracts.Health.HealthCheckService+HealthCheckServiceClient";
}
