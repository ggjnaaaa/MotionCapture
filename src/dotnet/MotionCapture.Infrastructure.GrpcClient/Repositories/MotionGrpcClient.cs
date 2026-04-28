using AutoMapper;
using Grpc.Net.ClientFactory;
using MotionCapture.Core.Interfaces;
using MotionCapture.Core.Models;
using MotionCapture.Grpc.Contracts.Motion;
using MotionCapture.Infrastructure.GrpcClient.Options;

namespace MotionCapture.Infrastructure.GrpcClient.Repositories;

public class MotionGrpcClient : IMotionGrpcClient
{
    private readonly MotionService.MotionServiceClient _client;
    private readonly IConnectionStateService _connectionStateService;
    private readonly IMapper _mapper;

    public MotionGrpcClient(GrpcClientFactory grpcClientFactory, IMapper mapper, IConnectionStateService connectionStateService)
    {
        _client = grpcClientFactory.CreateClient<MotionService.MotionServiceClient>(GrpcClientNames.MotionService);
        ArgumentNullException.ThrowIfNull(_client);
        ArgumentNullException.ThrowIfNull(connectionStateService);
        ArgumentNullException.ThrowIfNull(mapper);
        _connectionStateService = connectionStateService;
        _mapper = mapper;
    }

    public async Task<MotionResult?> ProcessAsync(IEnumerable<Core.Models.CameraFrame> batch, CancellationToken ct)
    {
        var request = _mapper.Map<MotionRequest>(batch);

        var response = await _client.ProcessMotionAsync(request, cancellationToken: ct);

        if (response == null)
            return null;

        return _mapper.Map<MotionResult>(response);
    }
}
