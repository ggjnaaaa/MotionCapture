namespace MotionCapture.Core.Models;

public class Joint3D
{
    public string Name { get; set; } = string.Empty;
    public int ParentIndex { get; set; }
    public int PosX { get; set; }
    public int PosY { get; set; }
    public int PosZ { get; set; }
    public int RotX { get; set; }
    public int RotY { get; set; }
    public int RotZ { get; set; }
    public int RotW { get; set; }
}
