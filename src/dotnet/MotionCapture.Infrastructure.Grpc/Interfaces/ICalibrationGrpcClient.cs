using Grpc.Core;
using MotionCapture.Grpc.Contracts.Calibration;

namespace MotionCapture.Infrastructure.GrpcClient.Repositories;

public interface ICalibrationGrpcClient
{
    AsyncDuplexStreamingCall<CalibrationFrame, CalibrationStatus> StartCalibration();
}