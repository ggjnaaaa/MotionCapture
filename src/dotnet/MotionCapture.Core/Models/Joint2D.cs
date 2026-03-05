namespace MotionCapture.Core.Models;

public class Joint2D
{
    public string Name { get; set; } = string.Empty;
    public int ParentIndex { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
}
