using Emgu.CV;
using Emgu.CV.Util;
using MotionCapture.Core.Interfaces;
using MotionCapture.Core.Models;
using MotionCapture.Infrastructure.Grpc.Repositories;
using System.Diagnostics;

namespace MotionCapture.Infrastructure.Camera.Services;

public class MotionTrackingService : IMotionTrackingService
{
    private readonly IMotionGrpcClient _grpcClient;

    private readonly Dictionary<int, Mat> _latestFrames = new();
    private readonly HashSet<int> _activeCameras = new();
    private CancellationTokenSource _cts = new();

    private bool _isRunning;
    private int _targetFps = 3;

    public event Action<MotionResult?>? FrameProcessed;

    public MotionTrackingService(IMotionGrpcClient grpcClient)
    {
        _grpcClient = grpcClient;
    }

    public void SetTargetFps(int fps)
    {
        _targetFps = fps;
    }

    public void RegisterCamera(int cameraIndex)
    {
        _activeCameras.Add(cameraIndex);
    }

    public void UnregisterCamera(int cameraIndex)
    {
        _activeCameras.Remove(cameraIndex);
        _latestFrames.Remove(cameraIndex);
    }

    public void SubmitFrame(Mat frame, int cameraIndex)
    {
        lock (_latestFrames)
        {
            if (_latestFrames.ContainsKey(cameraIndex))
            {
                _latestFrames[cameraIndex].Dispose();
            }

            _latestFrames[cameraIndex] = frame.Clone();
        }

        if (!_isRunning)
            _ = ProcessLoop();
    }

    private async Task ProcessLoop()
    {
        _isRunning = true;

        var delay = 1000 / _targetFps;
        var stopwatch = Stopwatch.StartNew();

        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                List<CameraFrame> framesToSend = new();

                lock (_latestFrames)
                {
                    if (!_activeCameras.All(c => _latestFrames.ContainsKey(c)))
                    {
                        continue;
                    }

                    foreach (var kvp in _latestFrames)
                    {
                        framesToSend.Add(new CameraFrame
                        {
                            ImageBytes = MatToBytes(kvp.Value),
                            CameraIndex = kvp.Key,
                            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                        });
                    }
                }

                var result = await _grpcClient.ProcessAsync(framesToSend, _cts.Token);

                FrameProcessed?.Invoke(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MotionTracking error: {ex.Message}");
            }
        }

        stopwatch.Stop();

        var remaining = delay - stopwatch.ElapsedMilliseconds;
        if (remaining > 0)
            await Task.Delay((int)remaining);

        _isRunning = false;
    }
    public static byte[] MatToBytes(Mat mat)
    {
        using var buffer = new VectorOfByte();
        CvInvoke.Imencode(".jpg", mat, buffer);
        return buffer.ToArray();
    }
}
