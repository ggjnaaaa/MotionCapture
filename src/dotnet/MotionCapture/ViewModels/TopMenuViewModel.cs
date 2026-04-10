using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MotionCapture.Core.Enums;
using MotionCapture.Core.Interfaces;
using MotionCapture.Services;
using System.Windows.Input;

namespace MotionCapture.ViewModels;

public partial class TopMenuViewModel : ObservableObject
{
    private readonly ApplicationState _appState;
    private readonly IConnectionStateService _connectionStateService;

    public ICommand ToggleCalibrationCommand { get; }
    public ICommand ToggleSkeletonCaptureCommand { get; }
    public ICommand TryConnectCommand { get; }

    public string SkeletonCaptureButtonText =>
        _appState.IsSkeletonCaptureActive
            ? "Выкл захват скелета"
            : "Вкл захват скелета";

    public string CalibrationButtonText =>
        _appState.IsCalibrating
            ? $"Остановить калибровку"
            : "Начать калибровку";

    [ObservableProperty]
    private bool _isConnected;

    public TopMenuViewModel(ApplicationState appState, IConnectionStateService connectionStateService)
    {
        _appState = appState;
        _connectionStateService = connectionStateService;
        _connectionStateService.StateChanged += ConnectionStateChanged;
        IsConnected = _connectionStateService.IsConnected;

        ToggleCalibrationCommand = new RelayCommand(ToggleCalibration);
        ToggleSkeletonCaptureCommand = new RelayCommand(ToggleSkeletonCapture);
        TryConnectCommand = new RelayCommand(async () => await _connectionStateService.ForceCheckAsync());

        _appState.PropertyChanged += (_, __) => SyncFromState();
        SyncFromState();
    }

    private void ConnectionStateChanged(ConnectionState state, string message)
    {
        IsConnected = _connectionStateService.IsConnected;
    }

    private void ToggleCalibration()
    {
        _appState.ToggleCalibration();
    }

    private void ToggleSkeletonCapture()
    {
        _appState.ToggleSkeletonCapture();
    }

    private void SyncFromState()
    {
        OnPropertyChanged(nameof(SkeletonCaptureButtonText));
        OnPropertyChanged(nameof(CalibrationButtonText));
    }
}
