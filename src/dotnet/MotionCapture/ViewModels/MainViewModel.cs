using Microsoft.Extensions.DependencyInjection;
using MotionCapture.Controls;
using System.Collections.ObjectModel;

namespace MotionCapture.ViewModels;

public class MainViewModel : ViewModelBase
{
    public List<int> CameraCounts { get; } = new() { 1, 2 };

    private int _selectedCameraCount;
    public int SelectedCameraCount
    {
        get => _selectedCameraCount;
        set
        {
            SetProperty(ref _selectedCameraCount, value);
            UpdateCameras();
        }
    }
    public ObservableCollection<CameraCaptureControl> CameraControls { get; } = new();

    private readonly IServiceProvider _provider;

    public MainViewModel(IServiceProvider provider)
    {
        _provider = provider;
    }

    private void UpdateCameras()
    {
        foreach (var camera in CameraControls)
        {
            ((CameraViewModel)camera.DataContext).Stop();
        }
        CameraControls.Clear();

        for (int i = 0; i < SelectedCameraCount; i++)
        {
            var vm = _provider.GetRequiredService<CameraViewModel>();
            var control = new CameraCaptureControl(vm);
            CameraControls.Add(control);
        }
    }
}
