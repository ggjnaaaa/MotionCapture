using AutoMapper;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using MotionCapture.Core.Models;
using MotionCapture.Grpc.Contracts;
using MotionCapture.Infrastructure.Grpc.Options;

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
}
