using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MotionCapture.Infrastructure.Grpc.Mapping;
using MotionCapture.Infrastructure.Grpc.Options;
using MotionCapture.Infrastructure.Grpc.Repositories;

namespace MotionCapture.Infrastructure.Grpc.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddGrpcInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<GrpcOptions>(
            configuration.GetSection("Grpc"));

        services.AddSingleton<IMotionGrpcClient, MotionGrpcClient>();

        services.AddAutoMapper(ctg => { }, typeof(MotionMappingProfile));

        return services;
    }
}
