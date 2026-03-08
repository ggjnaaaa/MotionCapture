using Emgu.CV;
using Emgu.CV.CvEnum;
using MotionCapture.Core.Interfaces;
using System.Text.RegularExpressions;

namespace MotionCapture.Infrastructure.Camera.Services;

public class CameraCaptureService : ICameraCaptureService
{
    public event Action<Mat> FrameArrived;
    private VideoCapture? _capture;
    private CancellationTokenSource? _cts;

    public void Start(int index)
    {
        Stop();

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

            FrameArrived?.Invoke(frame);
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
