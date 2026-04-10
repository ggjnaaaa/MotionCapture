using Grpc.Core;
using MotionCapture.Core.Interfaces;

namespace MotionCapture.Infrastructure.Grpc.Interceptors;

public class ErrorHandlingStreamWriter<T> : IClientStreamWriter<T>
{
    private readonly IClientStreamWriter<T> _inner;
    private readonly IConnectionStateService _connectionStateService;

    public ErrorHandlingStreamWriter(IClientStreamWriter<T> inner, IConnectionStateService connectionStateService)
    {
        _inner = inner;
        _connectionStateService = connectionStateService;
    }

    public WriteOptions WriteOptions
    {
        get => _inner.WriteOptions;
        set => _inner.WriteOptions = value;
    }

    public async Task WriteAsync(T message)
    {
        try
        {
            await _inner.WriteAsync(message);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
        {
            _connectionStateService.SetServerUnavailable(ex.Message);
            throw;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
        {
            _connectionStateService.SetTimeout(ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _connectionStateService.SetGenericError(ex.Message);
            throw;
        }
    }

    public async Task CompleteAsync()
    {
        try
        {
            await _inner.CompleteAsync();
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
        {
            _connectionStateService.SetServerUnavailable(ex.Message);
            throw;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
        {
            _connectionStateService.SetTimeout(ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _connectionStateService.SetGenericError(ex.Message);
            throw;
        }
    }
}