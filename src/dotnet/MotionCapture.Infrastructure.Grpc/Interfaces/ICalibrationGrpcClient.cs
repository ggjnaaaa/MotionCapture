using Grpc.Core;
using MotionCapture.Grpc.Contracts.Calibration;

namespace MotionCapture.Infrastructure.Grpc.Repositories;

public interface ICalibrationGrpcClient
{
    AsyncDuplexStreamingCall<CalibrationFrame, CalibrationStatus> StartCalibration();
}