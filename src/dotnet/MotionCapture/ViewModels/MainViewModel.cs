using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using MotionCapture.Controls;
using MotionCapture.Core.Enums;
using MotionCapture.Core.Interfaces;
using MotionCapture.Infrastructure.Grpc.Repositories;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MotionCapture.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public List<int> CameraCounts { get; } = new() { 1, 2 };

    [ObservableProperty]
    private int _selectedCameraCount;
    [ObservableProperty]
    private bool _isConnected;

    public ObservableCollection<CameraCaptureControl> CameraControls { get; } = new();

    private readonly IServiceProvider _provider;
    private readonly ICameraGrpcClient _cameraGrpcClient;
    private readonly IConnectionStateService _connectionStateService;

    public MainViewModel(IServiceProvider provider, ICameraGrpcClient cameraGrpcClient, IConnectionStateService connectionStateService)
    {
        _provider = provider;
        _cameraGrpcClient = cameraGrpcClient;
        _connectionStateService = connectionStateService;
        _connectionStateService.StateChanged += ConnectionStateChanged;
        IsConnected = _connectionStateService.IsConnected;
    }

    private async void ConnectionStateChanged(ConnectionState state, string message)
    {
        try
        {
            IsConnected = _connectionStateService.IsConnected;

            if (IsConnected)
            {
                await _cameraGrpcClient.RemoveCamerasAsync();
                foreach (var camera in CameraControls)
                {
                    await ((CameraViewModel)camera.DataContext).ReconnectCamera();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ConnectionStateChanged error: {ex}");
        }
    }

    public async Task InitializeAsync()
    {
        await _cameraGrpcClient.RemoveCamerasAsync();
    }

    partial void OnSelectedCameraCountChanged(int value)
    {
        _ = UpdateCamerasAsync();
    }

    private async Task UpdateCamerasAsync()
    {
        try
        {
            foreach (var camera in CameraControls)
            {
                ((CameraViewModel)camera.DataContext).Stop();
            }
            CameraControls.Clear();

            if (!await _cameraGrpcClient.RemoveCamerasAsync())
                return;

            for (int i = 0; i < SelectedCameraCount; i++)
            {
                var vm = _provider.GetRequiredService<CameraViewModel>();
                var control = new CameraCaptureControl(vm);
                CameraControls.Add(control);
            }
        }
        catch (Exception) { }
    }
}
