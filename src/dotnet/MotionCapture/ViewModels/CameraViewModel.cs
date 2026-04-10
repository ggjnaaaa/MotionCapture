using CommunityToolkit.Mvvm.ComponentModel;
using Emgu.CV;
using MotionCapture.Core.Enums;
using MotionCapture.Core.Interfaces;
using MotionCapture.Core.Models;
using MotionCapture.Grpc.Contracts.Calibration;
using MotionCapture.Infrastructure.Grpc.Repositories;
using MotionCapture.Infrastructure.Grpc.Services;
using MotionCapture.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace MotionCapture.ViewModels;

public partial class CameraViewModel : ObservableObject
{
    private readonly ICameraProvider _cameraProvider;
    private readonly ICameraCaptureService _captureService;
    private readonly ICameraGrpcClient _cameraGrpcClient;
    private readonly IFrameSyncBuffer _frameSync;
    private readonly IConnectionStateService _connectionStateService;

    public ObservableCollection<CameraInfo> AvailableCameras { get; }
    private Mat? _lastFrame;
    private int? _currentIndex;

    [ObservableProperty]
    private CameraInfo? _selectedCamera;

    [ObservableProperty]
    private ImageSource? _rawImage;

    [ObservableProperty]
    private ImageSource? _skeletonImage;

    public CameraViewModel(
        ICameraProvider cameraProvider,
        ICameraCaptureService captureService,
        ICameraGrpcClient cameraGrpcClient,
        IFrameSyncBuffer frameSync,
        IConnectionStateService connectionStateService,
        ProcessingOrchestrator processingOrchestrator)
    {
        _cameraProvider = cameraProvider;
        _captureService = captureService;
        _cameraGrpcClient = cameraGrpcClient;
        _frameSync = frameSync;
        _connectionStateService = connectionStateService;
        _connectionStateService.StateChanged += ConnectionStateChanged;

        AvailableCameras = new ObservableCollection<CameraInfo>(_cameraProvider.GetAvailableCameras());

        processingOrchestrator.FrameReady += OnResultFrameReady;
        captureService.FrameArrived += OnFrameArrived;
    }

    private void ConnectionStateChanged(ConnectionState state, string message)
    {
        if (!_connectionStateService.IsConnected)
        {
            SkeletonImage = null;
        }
    }

    partial void OnSelectedCameraChanged(CameraInfo? value)
    {
        _ = OnCameraSelected();
    }

    private async void OnFrameArrived(int _,Mat frame)
    {
        _lastFrame = frame;
        RawImage = MatConverter.ToBitmapSource(frame);
    }

    private void OnResultFrameReady(Dictionary<int, Mat>? results)
    {
        if (results == null || !_currentIndex.HasValue || _lastFrame == null)
        {
            if (_lastFrame != null)
                SkeletonImage = MatConverter.ToBitmapSource(_lastFrame);
            else
                SkeletonImage = null;
            return;
        }
        else if (!_connectionStateService.IsConnected)
            SkeletonImage = null;

        SkeletonImage = MatConverter.ToBitmapSource(results.SingleOrDefault(f => f.Key == _currentIndex).Value);
    }

    private async Task OnCameraSelected()
    {
        if (SelectedCamera == null)
            return;

        if (SelectedCamera.IsBusy)
            return;

        try
        {
            if (_currentIndex.HasValue)
            {
                _cameraProvider.MarkFree(_currentIndex.Value);
                _frameSync.UnregisterCamera(_currentIndex.Value);
                await _cameraGrpcClient.ChangeCameraIndexAsync(_currentIndex.Value, SelectedCamera.Index);
            }
            else
            {
                await _cameraGrpcClient.AddCameraAsync(SelectedCamera.Index);
            }
        }
        catch (Exception) { }

        _currentIndex = SelectedCamera.Index;
        _cameraProvider.MarkBusy(_currentIndex.Value);
        _frameSync.RegisterCamera(_currentIndex.Value);

        _captureService.Start(SelectedCamera.Index);
    }

    public void Stop()
    {
        _captureService.Stop();

        if (_currentIndex.HasValue)
        {
            _cameraProvider.MarkFree(_currentIndex.Value);
            _frameSync.UnregisterCamera(_currentIndex.Value);
        }
        _currentIndex = null;
    }

    public async Task ReconnectCamera()
    {
        if (_currentIndex.HasValue)
            _ = _cameraGrpcClient.AddCameraAsync(_currentIndex.Value);
        else
        {
            RawImage = null;
            SkeletonImage = null;
        }
    }
}
