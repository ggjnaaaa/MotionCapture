using Microsoft.Extensions.DependencyInjection;
using MotionCapture.Infrastructure.GrpcServer.Mapping;

namespace MotionCapture.Infrastructure.GrpcServer.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddGrpcServer(this IServiceCollection services)
    {
        services.AddAutoMapper(ctg => { }, typeof(SkeletonMappingProfile));

        return services;
    }
}
