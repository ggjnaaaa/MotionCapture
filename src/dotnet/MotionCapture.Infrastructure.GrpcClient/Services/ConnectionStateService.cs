using Google.Protobuf.WellKnownTypes;
using Grpc.Net.ClientFactory;
using MotionCapture.Core.Enums;
using MotionCapture.Core.Interfaces;
using MotionCapture.Grpc.Contracts.Health;
using MotionCapture.Grpc.Contracts.Motion;
using MotionCapture.Infrastructure.GrpcClient.Options;
using MotionCapture.Infrastructure.GrpcClient.Repositories;

namespace MotionCapture.Infrastructure.GrpcClient.Services;

public class ConnectionStateService : IConnectionStateService
{
    private readonly Lazy<HealthCheckService.HealthCheckServiceClient> _healthClientLazy;
    private HealthCheckService.HealthCheckServiceClient? _healthClient;
    private HealthCheckService.HealthCheckServiceClient HealthClient
    {
        get
        {
            if (_healthClient == null)
                _healthClient = _healthClientLazy.Value;
            return _healthClient;
        }
    }

    private ConnectionState _state = ConnectionState.GenericError;
    private string _message = string.Empty;

    public event Action<ConnectionState, string>? StateChanged;
    public bool IsConnected { get => _state == ConnectionState.Connected; }

    public ConnectionStateService(GrpcClientFactory grpcClientFactory)
    {
        ArgumentNullException.ThrowIfNull(grpcClientFactory);
        _healthClientLazy = new Lazy<HealthCheckService.HealthCheckServiceClient>(
            () => grpcClientFactory.CreateClient<HealthCheckService.HealthCheckServiceClient>(GrpcClientNames.HealthCheck));
        ArgumentNullException.ThrowIfNull(_healthClientLazy);
    }

    public void SetConnected()
    {
        if (_state == ConnectionState.Connected) return;
        _state = ConnectionState.Connected;
        _message = string.Empty;
        StateChanged?.Invoke(_state, _message);
    }

    public void SetServerUnavailable(string message)
    {
        _state = ConnectionState.Unavailable;
        _message = message;
        StateChanged?.Invoke(_state, _message);
    }

    public void SetTimeout(string message)
    {
        _state = ConnectionState.Timeout;
        _message = message;
        StateChanged?.Invoke(_state, _message);
    }

    public void SetGenericError(string message)
    {
        _state = ConnectionState.GenericError;
        _message = message;
        StateChanged?.Invoke(_state, _message);
    }

    public async Task<bool> ForceCheckAsync()
    {
        try
        {
            var isAvailable = await HealthClient.CheckHealthAsync(new Empty());
            SetConnected();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
