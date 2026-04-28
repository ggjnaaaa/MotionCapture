using Grpc.Core;
using Grpc.Net.ClientFactory;
using MotionCapture.Grpc.Contracts.Calibration;
using MotionCapture.Infrastructure.GrpcClient.Options;

namespace MotionCapture.Infrastructure.GrpcClient.Repositories;

public class CalibrationGrpcClient : ICalibrationGrpcClient
{
    private readonly CalibrationService.CalibrationServiceClient _client;

    public CalibrationGrpcClient(GrpcClientFactory grpcClientFactory)
    {
        _client = grpcClientFactory.CreateClient<CalibrationService.CalibrationServiceClient>(GrpcClientNames.CalibrationService);
        ArgumentNullException.ThrowIfNull(_client);
    }

    public AsyncDuplexStreamingCall<CalibrationFrame, CalibrationStatus> StartCalibration()
    {
        return _client.Calibrate();
    }
}
