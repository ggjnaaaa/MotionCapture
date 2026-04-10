using Emgu.CV;
using Emgu.CV.Structure;
using MotionCapture.Core.Models;
using System.Drawing;

namespace MotionCapture.Services;

public class EmguSkeletonDrawingService
{
    public Mat Draw(Mat frame, IEnumerable<Joint2D> joints)
    {
        var result = frame.Clone();

        var jointList = joints.ToList();

        for (int i = 0; i < jointList.Count; i++)
        {
            var joint = jointList[i];

            var childPoint = new System.Drawing.Point(joint.X, joint.Y);

            if (joint.ParentIndex >= 0 &&
                joint.ParentIndex < jointList.Count)
            {
                var parent = jointList[joint.ParentIndex];
                var parentPoint = new System.Drawing.Point(parent.X, parent.Y);

                CvInvoke.Line(
                    result,
                    childPoint,
                    parentPoint,
                    new MCvScalar(255, 0, 0), // синий
                    3);
            }

            CvInvoke.Circle(
                result,
                childPoint,
                8,
                getColorByJointType(joint),
                -1);
        }

        return result;
    }

    private MCvScalar getColorByJointType(Joint2D joint2D)
    {
        if (!joint2D.IsVisible) return new MCvScalar(0, 0, 255);  // на точках с низкой видимостью красный
        if (joint2D.Name.Contains("LEFT")) return new MCvScalar(253, 76, 192);  // на левую фиолетовый
        if (joint2D.Name.Contains("RIGHT")) return new MCvScalar(0, 197, 253);  // на правую желтую
        return new MCvScalar(36, 151, 80);  // на остальные (например, нос - зеленые)
    }

    public Mat DrawPoints(Mat frame, IEnumerable<Joint2D> joints)
    {
        var result = frame.Clone();

        var jointList = joints.ToList();

        for (int i = 0; i < jointList.Count; i++)
        {
            var joint = jointList[i];
            var point = new System.Drawing.Point(joint.X, joint.Y);

            CvInvoke.Circle(
                result,
                point,
                3,
                new MCvScalar(4, 75, 242),
                - 1);
        }

        return result;
    }

    public Mat DrawOverlay(Mat frame, string text)
    {
        var result = frame.Clone();

        // затемнение (накладываем полупрозрачный черный слой)
        using var overlay = new Mat(result.Size, Emgu.CV.CvEnum.DepthType.Cv8U, 3);
        overlay.SetTo(new MCvScalar(0, 0, 0));

        double alpha = 0.3;
        CvInvoke.AddWeighted(overlay, alpha, result, 1 - alpha, 0, result);

        var font = Emgu.CV.CvEnum.FontFace.HersheySimplex;
        double fontScale = 1.5;
        int thickness = 3;

        int baseline = -1;
        var textSize = CvInvoke.GetTextSize(text, font, fontScale, thickness, ref baseline);

        var centerX = (result.Width - textSize.Width) / 2;
        var centerY = (result.Height + textSize.Height) / 2;

        result = DrawCyrillicTextCentered(result, text, fontSizePoints: 28);

        return result;
    }

    public Mat DrawCyrillicTextCentered(Mat frame, string text, float fontSizePoints = 28, Color color = default)
    {
        if (color == default) color = Color.White;

        using (var bitmap = frame.ToBitmap())
        using (var g = Graphics.FromImage(bitmap))
        {
            using (var font = new Font("Arial", fontSizePoints, FontStyle.Bold))
            using (var brush = new SolidBrush(color))
            {
                SizeF textSize = g.MeasureString(text, font);

                float x = (bitmap.Width - textSize.Width) / 2;
                float y = bitmap.Height - textSize.Height - 10;

                g.DrawString(text, font, brush, x, y);
            }
            return bitmap.ToMat();
        }
    }
}
