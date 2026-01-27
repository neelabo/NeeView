using System;
using System.Windows;

namespace NeeView.Windows
{
    public static class VectorExtensions
    {
        // from http://stackoverflow.com/questions/22818531/how-to-rotate-2d-vector

        private const double DegToRad = Math.PI / 180;


        public static Vector Zero { get; } = new Vector();


        public static Vector Rotate(this Vector v, double degrees)
        {
            return v.RotateRadians(degrees * DegToRad);
        }

        public static Vector RotateRadians(this Vector v, double radians)
        {
            var ca = Math.Cos(radians);
            var sa = Math.Sin(radians);
            return new Vector(ca * v.X - sa * v.Y, sa * v.X + ca * v.Y);
        }


        public static Vector Lerp(Vector v0, Vector v1, double rate)
        {
            return v0 + (v1 - v0) * rate;
        }

        public static Point Lerp(Point v0, Point v1, double rate)
        {
            return v0 + (v1 - v0) * rate;
        }

        public static bool IsZero(this Vector v)
        {
            return v.X == 0.0 && v.Y == 0.0;
        }
    }

}
