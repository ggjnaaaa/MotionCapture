using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic.ApplicationServices;
using MotionCapture.Core.Enums;
using MotionCapture.Core.Interfaces;

namespace MotionCapture.ViewModels;

public partial class OverlayViewModel : ObservableObject
{
    private readonly IConnectionStateService _connectionStateService;

    [ObservableProperty] private bool _isVisible = false;
    [ObservableProperty] private string _message = string.Empty;

    public OverlayViewModel(IConnectionStateService connectionStateService)
    {
        _connectionStateService = connectionStateService;
        _connectionStateService.StateChanged += OnStateChanged;
    }

    private void OnStateChanged(ConnectionState state, string message)
    {
        switch (state)
        {
            case ConnectionState.Connected:
                IsVisible = false;
                break;
            case ConnectionState.Unavailable:
            case ConnectionState.Timeout:
            case ConnectionState.GenericError:
                IsVisible = true;
                Message = message;
                break;
        }
    }

    [RelayCommand()]
    private void OkClicked()
    {
        IsVisible = false;
        Message = string.Empty;
    }
}
