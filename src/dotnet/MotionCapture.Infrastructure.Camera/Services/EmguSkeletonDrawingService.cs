using Emgu.CV;
using Emgu.CV.Structure;
using MotionCapture.Core.Interfaces;
using MotionCapture.Core.Models;

namespace MotionCapture.Infrastructure.Camera.Services
{
    public class EmguSkeletonDrawingService : ISkeletonDrawingService
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
                        new MCvScalar(255, 0, 0), // синий (BGR!)
                        3);
                }

                CvInvoke.Circle(
                    result,
                    childPoint,
                    8,
                    new MCvScalar(0, 255, 0), // зелёный
                    -1);
            }

            return result;
        }
    }
}
