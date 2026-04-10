using Emgu.CV;
using Emgu.CV.CvEnum;
using MotionCapture.Core.Interfaces;
using System.Text.RegularExpressions;

namespace MotionCapture.Infrastructure.Camera.Services;

public class CameraCaptureService : ICameraCaptureService
{
    private IFrameSyncBuffer _frameSyncBuffer;

    public event Action<int, Mat>? FrameArrived;
    private VideoCapture? _capture;
    private CancellationTokenSource? _cts;
    private int _index;

    public CameraCaptureService(IFrameSyncBuffer frameSyncBuffer)
    {
        _frameSyncBuffer = frameSyncBuffer ?? throw new ArgumentNullException(nameof(frameSyncBuffer));
    }

    public void Start(int index)
    {
        Stop();

        _index = index;
        _capture = new VideoCapture(index, VideoCapture.API.DShow);
        _capture.Set(CapProp.FrameWidth, 640);
        _capture.Set(CapProp.FrameHeight, 480);

        _capture.ImageGrabbed += OnFrameGrabbed;

        Task.Run(() =>
        {
            try
            {
                _capture?.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка запуска VideoCapture: " + ex);
            }
        });
    }

    private void OnFrameGrabbed(object? sender, EventArgs e)
    {
        if (_capture == null) return;

        try
        {
            var frame = new Mat();
            bool success = _capture.Retrieve(frame);

            if (!success || frame.IsEmpty)
                return;

            FrameArrived?.Invoke(_index, frame);
            _frameSyncBuffer.SubmitFrame(frame, _index);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка обработки кадра: " + ex);
        }
    }

    public void Stop()
    {
        if (_capture != null)
        {
            _capture.ImageGrabbed -= OnFrameGrabbed;
            _capture.Stop();
            _capture.Dispose();
            _capture = null;
        }

        _cts?.Cancel();
        _cts = null;
    }
}
