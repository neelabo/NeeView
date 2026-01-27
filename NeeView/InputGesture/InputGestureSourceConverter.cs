using System;
using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// 文字列を InputGestureSource に変換する
    /// </summary>
    public static class InputGestureSourceConverter
    {
        private static readonly KeyGestureSourceConverter _keyGestureConverter = new();
        private static readonly MouseGestureSourceConverter _mouseGestureConverter = new();


        /// <summary>
        /// 文字列を InputGestureSource に変換する
        /// </summary>
        /// <param name="source">ショートカット定義文字列</param>
        /// <returns>InputGestureSource。変換出来なかった場合は null</returns>
        public static InputGestureSource? ConvertFromString(string source)
        {
            if (source.Contains("Wheel", StringComparison.Ordinal) || source.Contains("Click", StringComparison.Ordinal))
            {
                return ConvertFromMouseGestureString(source);
            }
            else
            {
                return ConvertFromKeyGestureString(source);
            }
        }

        public static InputGestureSource? ConvertFromKeyGestureString(string source)
        {
            try
            {
                var gesture = _keyGestureConverter.ConvertFromString(source) as InputGestureSource;
                if (gesture is not null) return gesture;
            }
            catch (Exception e)
            {
                Debug.WriteLine("(Ignore this exception): " + e.Message);
            }

            return null;
        }

        public static InputGestureSource? ConvertFromMouseGestureString(string source)
        {
            try
            {
                var gesture = _mouseGestureConverter.ConvertFromString(source) as InputGestureSource;
                if (gesture is not null) return gesture;
            }
            catch (Exception e)
            {
                Debug.WriteLine("(Ignore this exception): " + e.Message);
            }

            return null;
        }

        /// <summary>
        /// InputGestureを文字列にする
        /// </summary>
        public static string? ConvertToString(InputGestureSource? gesture)
        {
            return gesture switch
            {
                KeyGestureSource e => _keyGestureConverter.ConvertToString(e),
                MouseGestureSource e => _mouseGestureConverter.ConvertToString(e),
                _ => throw new NotSupportedException($"Not supported gesture type: {gesture?.GetType()}"),
            };
        }

        // TODO: InputDisplayString クラスで定義されれるべき？
        public static string GetDisplayString(InputGestureSource? gesture)
        {
            return gesture switch
            {
                KeyGestureSource e => e.GetDisplayString(),
                MouseGestureSource e => e.GetDisplayString(),
                _ => throw new NotSupportedException($"Not supported gesture type: {gesture?.GetType()}"),
            };
        }
    }
}
