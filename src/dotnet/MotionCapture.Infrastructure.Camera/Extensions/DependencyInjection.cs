using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MotionCapture.Core.Interfaces;
using MotionCapture.Infrastructure.Camera.Options;
using MotionCapture.Infrastructure.Camera.Services;

namespace MotionCapture.Infrastructure.Camera.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddCameraInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ICameraProvider, EmguCameraProvider>();
        services.AddSingleton<ISkeletonDrawingService, EmguSkeletonDrawingService>();

        services.AddTransient<ICameraCaptureService, CameraCaptureService>();
        services.AddTransient<IMotionTrackingService, MotionTrackingService>();

        return services;
    }
}
