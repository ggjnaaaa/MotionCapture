using Emgu.CV;
using Grpc.Core;
using MotionCapture.Core.Interfaces;
using MotionCapture.Core.Models;
using System.Diagnostics;

namespace MotionCapture.Infrastructure.Camera.Services;

public class FrameSyncBuffer : IFrameSyncBuffer, IDisposable
{
    private readonly Dictionary<int, Mat> _latestFrames = new();
    private readonly HashSet<int> _activeCameras = new();

    private readonly object _lock = new();

    private int _fps = 30;
    private CancellationTokenSource _cts = new();
    private Task? _loopTask;

    public event Func<List<CameraFrame>, IReadOnlyDictionary<int, Mat>, Task>? FramesReadyAsync;

    public FrameSyncBuffer()
    {
        StartLoop();
    }

    public void SetFps(int fps)
    {
        _fps = Math.Clamp(fps, 1, 120);
    }

    public void RegisterCamera(int cameraIndex)
    {
        lock (_lock)
        {
            _activeCameras.Add(cameraIndex);
        }
    }

    public void UnregisterCamera(int cameraIndex)
    {
        lock (_lock)
        {
            _activeCameras.Remove(cameraIndex);

            if (_latestFrames.TryGetValue(cameraIndex, out var mat))
            {
                mat.Dispose();
                _latestFrames.Remove(cameraIndex);
            }
        }
    }

    public void SubmitFrame(Mat frame, int cameraIndex)
    {
        lock (_lock)
        {
            if (_latestFrames.TryGetValue(cameraIndex, out var old))
                old.Dispose();

            _latestFrames[cameraIndex] = frame.Clone();
        }
    }

    private void StartLoop()
    {
        _loopTask = Task.Run(ProcessLoopAsync);
    }

    private async Task ProcessLoopAsync()
    {
        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                var start = Stopwatch.GetTimestamp();

                List<CameraFrame>? frames = null;
                Dictionary<int, Mat>? snapshot = null;

                lock (_lock)
                {
                    if (_activeCameras.All(c => _latestFrames.ContainsKey(c)))
                    {
                        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                        frames = _latestFrames.Select(kvp => new CameraFrame
                        {
                            CameraIndex = kvp.Key,
                            Timestamp = timestamp,
                            ImageBytes = MatToBytes(kvp.Value)
                        }).ToList();

                        snapshot = _latestFrames.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Clone()
                        );
                    }
                }

                if (frames != null && snapshot != null && FramesReadyAsync != null)
                {
                    var tasks = FramesReadyAsync.GetInvocationList()
                        .Cast<Func<List<CameraFrame>, IReadOnlyDictionary<int, Mat>, Task>>()
                        .Select(handler => handler(frames, snapshot));

                    try
                    {
                        await Task.WhenAll(tasks);
                    }
                    catch (Exception ex)
                    {
                        if (ex is not RpcException)
                        {
                            Debug.WriteLine($"Non-gRPC error: {ex}");
                        }
                    }
                }

                var elapsed = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
                var delay = Math.Max(1, (1000.0 / _fps) - elapsed);

                await Task.Delay((int)delay, _cts.Token);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Frame Sync Loop crashed: {ex}");
        }
    }

    private static byte[] MatToBytes(Mat mat)
    {
        using var buffer = new Emgu.CV.Util.VectorOfByte();
        CvInvoke.Imencode(".jpg", mat, buffer);
        return buffer.ToArray();
    }

    public void Dispose()
    {
        _cts.Cancel();
        _loopTask?.Wait();

        lock (_lock)
        {
            foreach (var mat in _latestFrames.Values)
                mat.Dispose();

            _latestFrames.Clear();
        }
    }
}