using AutoMapper;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using MotionCapture.Core.Models;
using MotionCapture.Grpc.Contracts;
using MotionCapture.Infrastructure.Grpc.Options;
using Google.Protobuf.WellKnownTypes;

namespace MotionCapture.Infrastructure.Grpc.Repositories;

public class MotionGrpcClient : IMotionGrpcClient
{
    private readonly MotionService.MotionServiceClient _client;
    private readonly IMapper _mapper;

    public MotionGrpcClient(IOptions<GrpcOptions> options, IMapper mapper)
    {
        var channel = GrpcChannel.ForAddress(options.Value.Address);
        _client = new MotionService.MotionServiceClient(channel);
        _mapper = mapper;
    }

    public async Task<MotionResult?> ProcessAsync(IEnumerable<Core.Models.CameraFrame> batch, CancellationToken ct)
    {
        var request = new MotionRequest();

        foreach (var frame in batch)
        {
            request.Frames.Add(new MotionCapture.Grpc.Contracts.CameraFrame
            {
                CameraIndex = frame.CameraIndex,
                TimestampMs = frame.Timestamp,
                Image = Google.Protobuf.ByteString.CopyFrom(frame.ImageBytes)
            });
        }

        var response = await _client.ProcessMotionAsync(request, cancellationToken: ct);

        if (response == null)
            return null;

        return _mapper.Map<MotionResult>(response);
    }

    public bool ChangeCameraIndex(int? previousIndex, int newIndex)
    {
        Empty responce;
        if (previousIndex == null)
        {
            responce = _client.AddCameraIndex(new() { CameraIndex = newIndex });
        }
        else
        {
            responce = _client.ChangeCameraIndex(new()
            {
                PreviousCameraIndex = (int)previousIndex,
                NewCameraIndex = newIndex
            });
        }
        return responce != null;
    }

    public bool RemoveCameras()
    {
        var responce = _client.RemoveCameras(new Empty());
        return responce != null;
    }

    public bool AddCamera(int newIndex)
    {
        var responce = _client.AddCameraIndex(new() { CameraIndex = newIndex });
        return responce != null;
    }
}
