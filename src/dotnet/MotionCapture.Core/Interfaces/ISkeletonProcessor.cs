using MotionCapture.Core.Models;

namespace MotionCapture.Core.Interfaces;

public interface ISkeletonProcessor
{
    void Process(MotionResult motion);
}
