using Emgu.CV;
using MotionCapture.Core.Interfaces;
using MotionCapture.Core.Models;

namespace MotionCapture.Services;

public class ProcessingOrchestrator
{
    private EmguSkeletonDrawingService _drawingService;
    private readonly object _frameLock = new();
    private readonly Dictionary<int, Mat> _lastFrames = new();

    public event Action<Dictionary<int, Mat>?>? FrameReady;
    public event Action<bool>? CalibrationFinished;

    public ProcessingOrchestrator(
        ICalibrationService calibrationService,
        IMotionTrackingService motionTrackingService,
        IFrameSyncBuffer frameSyncBuffer,
        ICameraCaptureService cameraCaptureService,
        ApplicationState state,
        EmguSkeletonDrawingService drawingService
    )
    {
        _drawingService = drawingService;

        cameraCaptureService.FrameArrived += UpdateFrame;

        frameSyncBuffer.FramesReadyAsync += async (frames, framesToDraw) =>
        {
            foreach (var f in framesToDraw) UpdateFrame(f.Key, f.Value);

            if (state.IsCalibrating)
                await calibrationService.ProcessAsync(frames);

            else if (state.IsSkeletonCaptureActive)
                await motionTrackingService.SubmitFrameAsync(frames);
        };

        motionTrackingService.FrameProcessed += OnMotionProcessed;

        calibrationService.CalibrationProgressChanged += OnCalibrationProcessed;

        state.CalibrationStopped += async () =>
        {
            await calibrationService.StopCalibrationAsync();
        };
    }

    private void UpdateFrame(int index, Mat frame)
    {
        lock (_frameLock)
        {
            if (_lastFrames.TryGetValue(index, out var old))
                old?.Dispose();
            _lastFrames[index] = frame.Clone();
        }
    }

    private Mat? GetLastFrame(int cameraIndex)
    {
        lock (_frameLock)
        {
            return _lastFrames.TryGetValue(cameraIndex, out var frame) ? frame.Clone() : null;
        }
    }

    private void OnMotionProcessed(MotionResult? result)
    {
        if (result == null || !result.FramesToDraw.Any())
        {
            FrameReady?.Invoke(null);
            return;
        }

        Dictionary<int, Mat> drawResults = new Dictionary<int, Mat>();

        foreach (var frame in result.FramesToDraw)
        {
            var lastFrame = GetLastFrame(frame.SourceCameraIndex);
            if (lastFrame == null) return;

            drawResults[frame.SourceCameraIndex] = _drawingService.Draw(lastFrame, frame.Joint2D);
        }

        FrameReady?.Invoke(drawResults);
    }

    private void OnCalibrationProcessed(float process, IEnumerable<FrameLandmarks2D>? landmarks2D, bool? isSuccess)
    {
        var text = $"Калибровка {process}%";
        Dictionary<int, Mat> drawResults = new();

        if (landmarks2D == null || !landmarks2D.Any())
        {
            int[] cameraIndices;
            lock (_frameLock)
            {
                cameraIndices = _lastFrames.Keys.ToArray();
            }

            foreach (int camIdx in cameraIndices)
            {
                var frame = GetLastFrame(camIdx);
                if (frame == null) continue;
                drawResults[camIdx] = _drawingService.DrawOverlay(frame, text);
            }
        }
        else
        {
            foreach (var landmarks in landmarks2D)
            {
                var lastFrame = GetLastFrame(landmarks.SourceCameraIndex);
                if (lastFrame == null) return;

                lastFrame = _drawingService.DrawPoints(lastFrame, landmarks.Joint2D);
                drawResults[landmarks.SourceCameraIndex] = _drawingService.DrawOverlay(lastFrame, text);
            }
        }

        FrameReady?.Invoke(drawResults);
        if (isSuccess != null)
        {
            CalibrationFinished?.Invoke((bool)isSuccess);
        }
    }
}
