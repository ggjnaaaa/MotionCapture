namespace MotionCapture.Core.Models;

public class JointPosition3D
{
    public string Name { get; set; } = string.Empty;
    public int ParentIndex { get; set; }
    public int PosX { get; set; }
    public int PosY { get; set; }
    public int PosZ { get; set; }
}
