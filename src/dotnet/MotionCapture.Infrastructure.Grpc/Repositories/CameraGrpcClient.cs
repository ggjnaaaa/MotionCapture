using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.ClientFactory;
using MotionCapture.Core.Interfaces;
using MotionCapture.Core.Models;
using MotionCapture.Grpc.Contracts;
using MotionCapture.Grpc.Contracts.Camera;
using MotionCapture.Infrastructure.GrpcClient.Options;

namespace MotionCapture.Infrastructure.GrpcClient.Repositories;

public class CameraGrpcClient : ICameraGrpcClient
{
    private readonly CameraService.CameraServiceClient _client;
    private readonly IConnectionStateService _connectionStateService;
    private readonly IMapper _mapper;

    public CameraGrpcClient(GrpcClientFactory grpcClientFactory, IMapper mapper, IConnectionStateService connectionStateService)
    {
        _client = grpcClientFactory.CreateClient<CameraService.CameraServiceClient>(GrpcClientNames.CameraService);
        ArgumentNullException.ThrowIfNull(_client);
        ArgumentNullException.ThrowIfNull(connectionStateService);
        ArgumentNullException.ThrowIfNull(mapper);
        _connectionStateService = connectionStateService;
        _mapper = mapper;
    }

    public async Task<bool> ChangeCameraIndexAsync(int? previousIndex, int newIndex)
    {
        try
        {
            Empty responce;
            if (previousIndex == null)
            {
                responce = await _client.AddCameraIndexAsync(new() { CameraIndex = newIndex });
            }
            else
            {
                responce = await _client.ChangeCameraIndexAsync(new()
                {
                    PreviousCameraIndex = (int)previousIndex,
                    NewCameraIndex = newIndex
                });
            }
            return responce != null;
        }
        catch (Exception e)
        {
            _connectionStateService.SetGenericError($"Error while trying to ChangeCameraIndexAsync:\n{e.Message}");
            return false;
        }
    }

    public async Task<bool> RemoveCamerasAsync()
    {
        try
        {
            var responce = await _client.RemoveCamerasAsync(new Empty());
            return responce != null;
        }
        catch (Exception e)
        {
            _connectionStateService.SetGenericError($"Error while trying to RemoveCamerasAsync:\n{e.Message}");
            return false;
        }
    }

    public async Task<bool> AddCameraAsync(int newIndex)
    {
        try
        {
            var responce = await _client.AddCameraIndexAsync(new() { CameraIndex = newIndex });
            return responce != null;
        }
        catch (Exception e)
        {
            _connectionStateService.SetGenericError($"Error while trying to AddCameraAsync:\n{e.Message}");
            return false;
        }
    }
}
