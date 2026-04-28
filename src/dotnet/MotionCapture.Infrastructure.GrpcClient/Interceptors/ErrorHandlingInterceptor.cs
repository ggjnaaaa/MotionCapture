using Grpc.Core;
using Grpc.Core.Interceptors;
using MotionCapture.Core.Interfaces;

namespace MotionCapture.Infrastructure.GrpcClient.Interceptors;

public class ErrorHandlingInterceptor : Interceptor
{
    private readonly IConnectionStateService _connectionStateService;

    public ErrorHandlingInterceptor(IConnectionStateService connectionStateService)
    {
        _connectionStateService = connectionStateService;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var call = continuation(request, context);
        return new AsyncUnaryCall<TResponse>(
            HandleResponse(call.ResponseAsync),
            call.ResponseHeadersAsync,
            call.GetStatus,
            call.GetTrailers,
            call.Dispose);
    }

    public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        var call = continuation(context);
        return new AsyncClientStreamingCall<TRequest, TResponse>(
            call.RequestStream,
            HandleResponse(call.ResponseAsync),
            call.ResponseHeadersAsync,
            call.GetStatus,
            call.GetTrailers,
            call.Dispose);
    }

    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        var call = continuation(request, context);
        var wrappedResponseStream = new ErrorHandlingStreamReader<TResponse>(call.ResponseStream, _connectionStateService);
        return new AsyncServerStreamingCall<TResponse>(
            wrappedResponseStream,
            call.ResponseHeadersAsync,
            call.GetStatus,
            call.GetTrailers,
            call.Dispose);
    }

    public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        var call = continuation(context);
        var wrappedResponseStream = new ErrorHandlingStreamReader<TResponse>(call.ResponseStream, _connectionStateService);
        var wrappedRequestStream = new ErrorHandlingStreamWriter<TRequest>(call.RequestStream, _connectionStateService);
        return new AsyncDuplexStreamingCall<TRequest, TResponse>(
            wrappedRequestStream,
            wrappedResponseStream,
            call.ResponseHeadersAsync,
            call.GetStatus,
            call.GetTrailers,
            call.Dispose);
    }

    private async Task<TResponse> HandleResponse<TResponse>(Task<TResponse> responseTask)
    {
        try
        {
            var response = await responseTask;
            _connectionStateService.SetConnected();
            return response;
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
