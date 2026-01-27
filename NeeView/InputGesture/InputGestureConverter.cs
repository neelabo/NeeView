using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// 文字列をInputGestureに変換する
    /// </summary>
    public static class InputGestureConverter
    {
        private static readonly KeyGestureConverter _keyGestureConverter = new();
        private static readonly KeyExGestureConverter _keyExGestureConverter = new();
        private static readonly MouseGestureConverter _mouseGestureConverter = new();
        private static readonly MouseExGestureConverter _mouseExGestureConverter = new();
        private static readonly MouseWheelGestureConverter _mouseWheelGestureConverter = new();
        private static readonly MouseHorizontalWheelGestureConverter _mouseHorizontalWheelGestureConverter = new();


        private enum ConverterType
        {
            Key,
            Mouse,
            MouseWheel,
            MouseHorizontalWheel
        }


        private static readonly Dictionary<ConverterType, Func<string, InputGesture?>> _converter = new()
        {
            [ConverterType.Key] = ConvertFromKeyGestureString,
            [ConverterType.Mouse] = ConvertFromMouseGestureString,
            [ConverterType.MouseWheel] = ConvertFromMouseWheelGestureString,
            [ConverterType.MouseHorizontalWheel] = ConvertFromMouseHorizontalWheelGestureString,
        };


        private static InputGesture? ConvertFromStringByOrder(string source, ConverterType type)
        {
            List<ConverterType> order = type switch
            {
                ConverterType.MouseWheel or ConverterType.MouseHorizontalWheel
                    => new List<ConverterType> { ConverterType.MouseWheel, ConverterType.MouseHorizontalWheel, ConverterType.Mouse, ConverterType.Key },
                ConverterType.Mouse
                    => new List<ConverterType> { ConverterType.Mouse, ConverterType.MouseWheel, ConverterType.MouseHorizontalWheel, ConverterType.Key },
                _
                    => new List<ConverterType> { ConverterType.Key, ConverterType.Mouse, ConverterType.MouseWheel, ConverterType.MouseHorizontalWheel },
            };
            foreach (var t in order)
            {
                var gesture = _converter[t](source);
                if (gesture != null) return gesture;
            }

            Debug.WriteLine($"'The combination of {source} key and modifier key is not supported.");
            return null;
        }


        /// <summary>
        /// 文字列をInputGestureに変換する
        /// </summary>
        /// <param name="source">ショートカット定義文字列</param>
        /// <returns>InputGesture。変換出来なかった場合は null</returns>
        public static InputGesture? ConvertFromString(string source)
        {
            // なるべく例外が発生しないようにコンバート順を考慮する
            if (source.Contains("Wheel", StringComparison.Ordinal))
            {
                return ConvertFromStringByOrder(source, ConverterType.MouseWheel);
            }
            else if (source.Contains("Click", StringComparison.Ordinal))
            {
                return ConvertFromStringByOrder(source, ConverterType.Mouse);
            }
            else
            {
                return ConvertFromStringByOrder(source, ConverterType.Key);
            }
        }

        /// <summary>
        /// 文字列をInputGestureに変換する。KeyExGestureのみ。
        /// </summary>
        /// <param name="source">ショートカット定義文字列</param>
        /// <returns>InputGesture。変換出来なかった場合は null</returns>
        public static InputGesture? ConvertFromKeyGestureString(string source)
        {
            // NOTE: KeyGesture は使用しない。 KeyExGesture のみ。

            try
            {
                var converter = new KeyExGestureConverter();
                var gesture = converter.ConvertFromString(source) as InputGesture;
                if (gesture is not null) return gesture;
            }
            catch (Exception e)
            {
                Debug.WriteLine("(Ignore this exception): " + e.Message);
            }

            return null;
        }

        /// <summary>
        /// 文字列をInputGestureに変換する。MouseExGestureのみ。
        /// </summary>
        /// <param name="source">ショートカット定義文字列</param>
        /// <returns>InputGesture。変換出来なかった場合は null</returns>
        public static InputGesture? ConvertFromMouseGestureString(string source)
        {
            // NOTE: MouseGesture は使用しない。 MouseExGesture のみ。

            try
            {
                var converter = new MouseExGestureConverter();
                var gesture = converter.ConvertFromString(source) as InputGesture;
                if (gesture is not null) return gesture;

            }
            catch (Exception e)
            {
                Debug.WriteLine("(Ignore this exception): " + e.Message);
            }

            return null;
        }

        /// <summary>
        /// 文字列をInputGestureに変換する。MouseWheelGestureのみ。
        /// </summary>
        /// <param name="source">ショートカット定義文字列</param>
        /// <returns>InputGesture。変換出来なかった場合は null</returns>
        public static InputGesture? ConvertFromMouseWheelGestureString(string source)
        {
            try
            {
                var converter = new MouseWheelGestureConverter();
                var gesture = converter.ConvertFromString(source) as InputGesture;
                if (gesture is not null) return gesture;
            }
            catch (Exception e)
            {
                Debug.WriteLine("(Ignore this exception): " + e.Message);
            }

            return null;
        }

        /// <summary>
        /// 文字列をInputGestureに変換する。MouseHorizontalWheelGestureのみ。
        /// </summary>
        /// <param name="source">ショートカット定義文字列</param>
        /// <returns>InputGesture。変換出来なかった場合は null</returns>
        public static InputGesture? ConvertFromMouseHorizontalWheelGestureString(string source)
        {
            try
            {
                var converter = new MouseHorizontalWheelGestureConverter();
                var gesture = converter.ConvertFromString(source) as InputGesture;
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
        public static string? ConvertToString(InputGesture? gesture)
        {
            return gesture switch
            {
                KeyGesture e => _keyGestureConverter.ConvertToString(e),
                KeyExGesture e => _keyExGestureConverter.ConvertToString(e),
                MouseGesture e => _mouseGestureConverter.ConvertToString(e),
                MouseExGesture e => _mouseExGestureConverter.ConvertToString(e),
                MouseWheelGesture e => _mouseWheelGestureConverter.ConvertToString(e),
                MouseHorizontalWheelGesture e => _mouseHorizontalWheelGestureConverter.ConvertToString(e),
                _ => throw new NotSupportedException($"Not supported gesture type: {gesture?.GetType()}"),
            };
        }

        // TODO: InputDisplayString クラスで定義されれるべき？
        public static string GetDisplayString(InputGesture? gesture)
        {
            return gesture switch
            {
                KeyGesture e => e.GetDisplayString(),
                KeyExGesture e => e.GetDisplayString(),
                MouseGesture e => e.GetDisplayString(),
                MouseExGesture e => e.GetDisplayString(),
                MouseWheelGesture e => e.GetDisplayString(),
                MouseHorizontalWheelGesture e => e.GetDisplayString(),
                _ => throw new NotSupportedException($"Not supported gesture type: {gesture?.GetType()}"),
            };
        }
    }
}
