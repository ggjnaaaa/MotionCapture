using Microsoft.Extensions.DependencyInjection;
using MotionCapture.Core.Interfaces;
using MotionCapture.Processing.Services;

namespace MotionCapture.Processing.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddProcessing(this IServiceCollection services)
    {
        services.AddTransient<ISkeletonProcessor, SkeletonProcessor>();
        services.AddSingleton<ISkeletonState, SkeletonState>();

        return services;
    }
}
