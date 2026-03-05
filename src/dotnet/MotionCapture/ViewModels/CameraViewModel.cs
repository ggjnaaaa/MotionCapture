using Emgu.CV;
using MotionCapture.Core.Interfaces;
using MotionCapture.Core.Models;
using MotionCapture.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace MotionCapture.ViewModels;

public class CameraViewModel : ViewModelBase
{
    private readonly ICameraProvider _cameraProvider;
    private readonly ICameraCaptureService _captureService;
    private readonly IMotionTrackingService _motionService;
    private readonly ISkeletonDrawingService _drawingService;

    public ObservableCollection<CameraInfo> AvailableCameras { get => new ObservableCollection<CameraInfo>(_cameraProvider.GetAvailableCameras()); }
    private Mat? _lastFrame;
    private int? _currentIndex;

    private CameraInfo? _selectedCamera;
    public CameraInfo? SelectedCamera
    {
        get => _selectedCamera;
        set
        {
            if (SetProperty(ref _selectedCamera, value))
            {
                OnCameraSelected();
            }
        }
    }

    private ImageSource? _rawImage;
    public ImageSource? RawImage
    {
        get => _rawImage;
        set => SetProperty(ref _rawImage, value);
    }

    private ImageSource? _skeletonImage;
    public ImageSource? SkeletonImage
    {
        get => _skeletonImage;
        set => SetProperty(ref _skeletonImage, value);
    }

    public CameraViewModel(
        ICameraProvider cameraProvider,
        ICameraCaptureService captureService,
        IMotionTrackingService motionService,
        ISkeletonDrawingService drawingService)
    {
        _cameraProvider = cameraProvider;
        _captureService = captureService;
        _motionService = motionService;
        _drawingService = drawingService;

        _captureService.FrameArrived += frame =>
        {
            _lastFrame = frame;
            RawImage = MatConverter.ToBitmapSource(frame);
            _motionService.SubmitFrame(frame, SelectedCamera.Index);
        };
        _motionService.FrameProcessed += OnMotionFrameArrived;
    }

    private void OnMotionFrameArrived(MotionResult? res)
    {
        if (_lastFrame == null)
            return;

        if (res == null)
        {
            SkeletonImage = MatConverter.ToBitmapSource(_lastFrame);
            return;
        }

        foreach (var toDraw in res.FramesToDraw)
        {
            var drawn = _drawingService.Draw(_lastFrame, toDraw.Joint2D);

            SkeletonImage = MatConverter.ToBitmapSource(drawn);
        }
    }

    private void OnCameraSelected()
    {
        if (SelectedCamera == null)
            return;

        if (SelectedCamera.IsBusy)
            return;

        if (_currentIndex.HasValue)
        {
            _cameraProvider.MarkFree(_currentIndex.Value);
            _motionService.UnregisterCamera(_currentIndex.Value);
        }

        _currentIndex = SelectedCamera.Index;
        _cameraProvider.MarkBusy(_currentIndex.Value);
        _motionService.RegisterCamera(_currentIndex.Value);

        _captureService.Start(SelectedCamera.Index);
    }

    public void Stop()
    {
        _captureService.Stop();

        if (_currentIndex.HasValue)
        {
            _cameraProvider.MarkFree(_currentIndex.Value);
            _motionService.UnregisterCamera(_currentIndex.Value);
        }
        _currentIndex = null;
    }
}
