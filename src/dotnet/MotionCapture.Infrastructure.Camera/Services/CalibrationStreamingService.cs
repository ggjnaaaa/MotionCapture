using AutoMapper;
using Grpc.Core;
using MotionCapture.Core.Interfaces;
using MotionCapture.Core.Models;
using MotionCapture.Grpc.Contracts.Calibration;
using MotionCapture.Infrastructure.GrpcClient.Repositories;

namespace MotionCapture.Infrastructure.Camera.Services;

public class CalibrationStreamingService : ICalibrationService
{
    private readonly ICalibrationGrpcClient _grpcClient;
    private readonly IMapper _mapper;
    private AsyncDuplexStreamingCall<CalibrationFrame, CalibrationStatus>? _call;
    private volatile bool _isRunning;
    private readonly SemaphoreSlim _readyToSend = new SemaphoreSlim(1, 1);
    private Task? _responseTask;
    private CancellationTokenSource? _cts;

    public event Action<float, IEnumerable<FrameLandmarks2D>?, bool?>? CalibrationProgressChanged;

    public CalibrationStreamingService(ICalibrationGrpcClient grpcClient, IMapper mapper)
    {
        _grpcClient = grpcClient;
        _mapper = mapper;
    }

    public async Task StartCalibrationAsync(CancellationToken token = default)
    {
        if (_isRunning) return;
        _isRunning = true;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

        _call = _grpcClient.StartCalibration();

        _responseTask = Task.Run(() => ReadResponsesAsync(_cts.Token));
    }

    private async Task ReadResponsesAsync(CancellationToken token)
    {
        try
        {
            await foreach (var status in _call.ResponseStream.ReadAllAsync(token))
            {
                CalibrationProgressChanged?.Invoke(
                    status.Progress * 100,
                    _mapper.Map<IEnumerable<FrameLandmarks2D>>(status.Landmarks),
                    status.IsDone ? status.Success : null);

                _readyToSend.Release();

                if (status.IsDone)
                {
                    _isRunning = false;
                    await StopCalibrationAsync();
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Calibration read error: {ex.Message}");
            _isRunning = false;
            CalibrationProgressChanged?.Invoke(0, null, false);
            _readyToSend.Release();
        }
    }

    public async Task ProcessAsync(List<CameraFrame> cameraFrames)
    {
        if (!_isRunning || _call == null) return;
        if (cameraFrames == null || cameraFrames.Count == 0) return;

        await _readyToSend.WaitAsync(_cts?.Token ?? CancellationToken.None);

        var request = new CalibrationFrame();
        foreach (var frameToSend in cameraFrames)
        {
            request.Frames.Add(new MotionCapture.Grpc.Contracts.Motion.CameraFrame
            {
                CameraIndex = frameToSend.CameraIndex,
                TimestampMs = frameToSend.Timestamp,
                Image = Google.Protobuf.ByteString.CopyFrom(frameToSend.ImageBytes)
            });
        }

        await _call.RequestStream.WriteAsync(request);
    }

    public async Task StopCalibrationAsync()
    {
        if (!_isRunning || _call == null) return;

        _isRunning = false;
        _cts?.Cancel();

        _readyToSend.Release();

        await _call.RequestStream.CompleteAsync();

        if (_responseTask != null)
            await _responseTask;

        _cts?.Dispose();
    }
}

