using System;

namespace NeeLaboratory
{
    public static class MathUtility
    {
        // from http://stackoverflow.com/questions/2683442/where-can-i-find-the-clamp-function-in-net
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        public static bool WithinRange<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return false;
            else if (val.CompareTo(max) > 0) return false;
            else return true;
        }

        public static T Max<T>(this T v0, T v1) where T : IComparable<T>
        {
            return v0.CompareTo(v1) > 0 ? v0 : v1;
        }

        public static T Min<T>(this T v0, T v1) where T : IComparable<T>
        {
            return v0.CompareTo(v1) < 0 ? v0 : v1;
        }

        public static double Lerp(double v0, double v1, double rate)
        {
            return v0 + (v1 - v0) * rate;
        }


        public static int CycleLoopRange(int val, int min, int max)
        {
            if (min > max) throw new ArgumentException("need min <= max");

            if (val >= max)
            {
                return (val - min) / (max - min + 1);
            }
            else if (val < min)
            {
                return (val - min + 1) / (max - min + 1) - 1;
            }
            else
            {
                return 0;
            }
        }

        public static int NormalizeLoopRange(int val, int min, int max)
        {
            if (min > max) throw new ArgumentException("need min <= max");

            if (val >= max)
            {
                return min + (val - min) % (max - min + 1);
            }
            else if (val < min)
            {
                return max - (min - val - 1) % (max - min + 1);
            }
            else
            {
                return val;
            }
        }

        public static double NormalizeLoopRange(double val, double min, double max)
        {
            if (min >= max) throw new ArgumentException("need min < max");

            if (val >= max)
            {
                return min + (val - min) % (max - min);
            }
            else if (val < min)
            {
                return max - (min - val) % (max - min);
            }
            else
            {
                return val;
            }
        }

        public static double SnapValue(double value, double target, double margin)
        {
            return (Math.Abs(target - value) < margin) ? target : value;
        }

        public static double Snap(double val, double tick)
        {
            return Math.Floor((val + tick * 0.5) / tick) * tick;
        }

        public static double Snap(double value, double oldValue, double snap, double margin)
        {
            var newLength = Math.Abs(value - snap);
            var oldLength = Math.Abs(oldValue - snap);
            return (oldLength < newLength && newLength < margin) ? snap : value;
        }

        public static AngleDirection DegreeToDirection(double degree)
        {
            var value = (int)(NormalizeLoopRange(degree, 0.0, 360.0) / 45.0);
            return value switch
            {
                0 or 7 => AngleDirection.Forward,
                1 or 2 => AngleDirection.Right,
                3 or 4 => AngleDirection.Back,
                5 or 6 => AngleDirection.Left,
                _ => AngleDirection.Forward
            };
        }
    }
}

