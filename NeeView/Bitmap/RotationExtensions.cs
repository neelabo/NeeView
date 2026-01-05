using System.Windows.Media.Imaging;

namespace NeeView
{
    public static class RotationExtensions
    {
        public static double ToAngle(this Rotation rotation)
        {
            return rotation switch
            {
                Rotation.Rotate0 => 0.0,
                Rotation.Rotate90 => 90.0,
                Rotation.Rotate180 => 180.0,
                Rotation.Rotate270 => 270.0,
                _ => 0.0,
            };
        }
    }
}