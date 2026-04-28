using Grpc.Core;
using Grpc.Net.ClientFactory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MotionCapture.Core.Interfaces;
using MotionCapture.Grpc.Contracts.Health;
using MotionCapture.Infrastructure.GrpcClient.Options;

namespace MotionCapture.Infrastructure.GrpcClient.Services;

public class HealthCheckBackgroundService : BackgroundService
{
    private readonly HealthCheckService.HealthCheckServiceClient _healthClient;
    private readonly IConnectionStateService _connectionStateService;
    private readonly ILogger<HealthCheckBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(5);
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(3);

    public HealthCheckBackgroundService(
        GrpcClientFactory grpcClientFactory,
        IConnectionStateService connectionStateService,
        ILogger<HealthCheckBackgroundService> logger)
    {
        _healthClient = grpcClientFactory.CreateClient<HealthCheckService.HealthCheckServiceClient>(GrpcClientNames.HealthCheck);
        ArgumentNullException.ThrowIfNull(_healthClient);
        _connectionStateService = connectionStateService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                cts.CancelAfter(_timeout);
                await _healthClient.CheckHealthAsync(new Google.Protobuf.WellKnownTypes.Empty(), cancellationToken: cts.Token);

                if (!_connectionStateService.IsConnected)
                {
                    _logger.LogInformation("Health check passed: server is available.");
                    _connectionStateService.SetConnected();
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable || ex.StatusCode == StatusCode.DeadlineExceeded)
            {
                if (_connectionStateService.IsConnected)
                {
                    _logger.LogWarning($"Health check failed: {ex.StatusCode}. Server unavailable.");
                    _connectionStateService.SetServerUnavailable(ex.Message);
                }
            }
            catch (OperationCanceledException)
            {
                if (_connectionStateService.IsConnected)
                {
                    _logger.LogWarning("Health check timeout.");
                    _connectionStateService.SetTimeout("Сервер не отвечает на проверку здоровья.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check error.");
                if (_connectionStateService.IsConnected)
                    _connectionStateService.SetGenericError(ex.Message);
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
}
