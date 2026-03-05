using Emgu.CV;
using MotionCapture.Core.Models;

namespace MotionCapture.Core.Interfaces;

public interface ISkeletonDrawingService
{
    Mat Draw(Mat frame, IEnumerable<Joint2D> motionFrame);
}
