using AutoMapper;
using Grpc.Core;
using MotionCapture.Core.Interfaces;
using MotionCapture.Grpc.Contracts.Skeleton;

namespace MotionCapture.Infrastructure.GrpcServer.Services;

public class SkeletonGrpcService : SkeletonService.SkeletonServiceBase
{
    private readonly ISkeletonState _state;
    private readonly IMapper _mapper;

    public SkeletonGrpcService(ISkeletonState state, IMapper mapper)
    {
        _state = state;
        _mapper = mapper;
    }

    public override async Task StreamSkeleton(
        SkeletonRequest request,
        IServerStreamWriter<Skeleton> responseStream,
        ServerCallContext context)
    {
        while (!context.CancellationToken.IsCancellationRequested)
        {
            try
            {
                var skeleton = await _state.WaitForNextAsync(context.CancellationToken);

                var proto = _mapper.Map<Skeleton>(skeleton);

                await responseStream.WriteAsync(proto);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}