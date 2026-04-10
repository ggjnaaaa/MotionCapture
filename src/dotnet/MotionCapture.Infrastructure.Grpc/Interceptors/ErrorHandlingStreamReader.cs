using Grpc.Core;
using MotionCapture.Core.Interfaces;

namespace MotionCapture.Infrastructure.Grpc.Interceptors;

public class ErrorHandlingStreamReader<T> : IAsyncStreamReader<T>
{
    private readonly IAsyncStreamReader<T> _inner;
    private readonly IConnectionStateService _connectionStateService;

    public ErrorHandlingStreamReader(IAsyncStreamReader<T> inner, IConnectionStateService connectionStateService)
    {
        _inner = inner;
        _connectionStateService = connectionStateService;
    }

    public T Current => _inner.Current;

    public async Task<bool> MoveNext(CancellationToken cancellationToken)
    {
        try
        {
            var moved = await _inner.MoveNext(cancellationToken);
            if (!moved)
                _connectionStateService.SetConnected();
            return moved;
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