using Grpc.Core;
using Grpc.Net.Client.Configuration;
using Grpc.Net.ClientFactory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MotionCapture.Core.Interfaces;
using MotionCapture.Infrastructure.Grpc.Interceptors;
using MotionCapture.Infrastructure.Grpc.Mapping;
using MotionCapture.Infrastructure.Grpc.Options;
using MotionCapture.Infrastructure.Grpc.Repositories;
using MotionCapture.Infrastructure.Grpc.Services;
using System.Net.Http;

namespace MotionCapture.Infrastructure.Grpc.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddGrpcInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddGrpcClientsFromConfiguration(configuration);

        services.AddSingleton<IMotionGrpcClient, MotionGrpcClient>();
        services.AddSingleton<ICameraGrpcClient, CameraGrpcClient>();
        services.AddSingleton<ICalibrationGrpcClient, CalibrationGrpcClient>();
        services.AddSingleton<IConnectionStateService, ConnectionStateService>();
        services.AddSingleton<ErrorHandlingInterceptor>();

        services.AddHostedService<HealthCheckBackgroundService>();

        services.AddAutoMapper(ctg => { }, typeof(MotionMappingProfile));

        return services;
    }

    public static IServiceCollection AddGrpcClientsFromConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var clients = configuration.GetSection("GrpcClients").Get<List<GrpcClientConfig>>();
        if (clients == null) return services;

        foreach (var clientConfig in clients)
        {
            Type? clientType = Type.GetType(clientConfig.ClientType);
            if (clientType == null)
            {
                clientType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.FullName == clientConfig.ClientType);
            }
            if (clientType == null)
                throw new InvalidOperationException($"Тип клиента {clientConfig.ClientType} не найден");

            var addGrpcClientMethod = typeof(GrpcClientServiceExtensions)
                .GetMethod(nameof(GrpcClientServiceExtensions.AddGrpcClient), 1, new[] { typeof(IServiceCollection), typeof(string), typeof(Action<GrpcClientFactoryOptions>) })
                ?.MakeGenericMethod(clientType);

            if (addGrpcClientMethod == null)
                continue;

            Action<GrpcClientFactoryOptions> configure = options =>
            {
                if (!string.IsNullOrEmpty(clientConfig.Address))
                    options.Address = new Uri(clientConfig.Address);

                options.ChannelOptionsActions.Add(channelOptions =>
                {
                    channelOptions.HttpHandler = new SocketsHttpHandler
                    {
                        ConnectTimeout = TimeSpan.FromSeconds(clientConfig.TimeoutSec)
                    };

                    var retryableStatuses = clientConfig.RetryableStatuses
                        .Select(s => (StatusCode)Enum.Parse(typeof(StatusCode), s, ignoreCase: true))
                        .ToList();

                    var retryPolicy = new RetryPolicy
                    {
                        MaxAttempts = clientConfig.MaxRetryAttempts,
                        MaxBackoff = TimeSpan.FromSeconds(clientConfig.TimeoutSec),
                        InitialBackoff = TimeSpan.FromSeconds(1),
                        BackoffMultiplier = 1.5
                    };

                    foreach (var status in retryableStatuses)
                        retryPolicy.RetryableStatusCodes.Add(status);

                    var methodConfig = new MethodConfig
                    {
                        Names = { MethodName.Default },
                        RetryPolicy = retryPolicy
                    };

                    channelOptions.ServiceConfig = new ServiceConfig
                    {
                        MethodConfigs = { methodConfig }
                    };
                });
            };

            var httpClientBuilder = addGrpcClientMethod.Invoke(null, new object[] { services, clientConfig.ClientType, configure });
            var addInterceptorMethod = typeof(GrpcHttpClientBuilderExtensions)
                .GetMethods()
                .FirstOrDefault(m => m.Name == nameof(GrpcHttpClientBuilderExtensions.AddInterceptor) &&
                                     m.IsGenericMethod &&
                                     m.GetGenericArguments().Length == 1 &&
                                     m.GetParameters().Length == 1 &&
                                     m.GetParameters()[0].ParameterType == typeof(IHttpClientBuilder));

            if (addInterceptorMethod != null)
            {
                var genericMethod = addInterceptorMethod.MakeGenericMethod(typeof(ErrorHandlingInterceptor));
                genericMethod.Invoke(null, new object[] { httpClientBuilder });
            }
            else
            {
                throw new InvalidOperationException("AddInterceptor method not found");
            }
        }

        return services;
    }
}
