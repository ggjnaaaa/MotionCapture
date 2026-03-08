using Emgu.CV;
using MotionCapture.Core.Interfaces;
using MotionCapture.Core.Models;

namespace MotionCapture.Infrastructure.Camera.Options;

public class EmguCameraProvider : ICameraProvider
{
    private readonly HashSet<int> _busyCameras = new();

    public void MarkBusy(int index)
    {
        _busyCameras.Add(index);
    }

    public void MarkFree(int index)
    {
        _busyCameras.Remove(index);
    }

    public IReadOnlyList<CameraInfo> GetAvailableCameras()
    {
        var result = new List<CameraInfo>();

        for (int i = 0; i < 10; i++)
        {
            using var cap = new VideoCapture(i, VideoCapture.API.DShow);

            if (cap.IsOpened)
            {
                result.Add(new CameraInfo
                {
                    Index = i,
                    Name = $"Camera {i}",
                    IsBusy = _busyCameras.Contains(i)
                });
            }
        }

        return result;
    }
}
