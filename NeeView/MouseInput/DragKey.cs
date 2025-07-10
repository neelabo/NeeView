using System;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using NeeView.Properties;

namespace NeeView
{
    [JsonConverter(typeof(JsonDragKeyConverter))]
    public record class DragKey
    {
        private const char _modifierDelimiter = '+';

        public static DragKey Empty { get; } = new DragKey();


        public DragKey()
        {
        }


        public DragKey(MouseButtonBits bits, ModifierKeys modifiers)
        {
            MouseButtonBits = bits;
            ModifierKeys = modifiers;
        }

        public DragKey(string? gesture)
        {
            if (string.IsNullOrWhiteSpace(gesture)) return;

            try
            {
                var key = DragKeyConverter.ConvertFromString(gesture);
                MouseButtonBits = key.MouseButtonBits;
                ModifierKeys = key.ModifierKeys;
            }
            catch (Exception)
            { }
        }

        public MouseButtonBits MouseButtonBits { get; init; }
        public ModifierKeys ModifierKeys { get; init; }
        public bool IsValid => MouseButtonBits != MouseButtonBits.None;

        public override string ToString()
        {
            return DragKeyConverter.ConvertToString(this);
        }

        public string GetDisplayString()
        {
            if (!IsValid) return "";

            string text = "";

            foreach (ModifierKeys key in Enum.GetValues(typeof(ModifierKeys)))
            {
                if ((ModifierKeys & key) != ModifierKeys.None)
                {
                    text += _modifierDelimiter + key.GetDisplayString();
                }
            }

            text += _modifierDelimiter + MouseButtonBits.GetDisplayString();

            return text.TrimStart(_modifierDelimiter);
        }
    }

    /// <summary>
    /// DragKey to DisplayString converter
    /// </summary>
    public class DragKeyToDisplayStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DragKey dragKey)
            {
                return dragKey.GetDisplayString();
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// マウスドラッグ コンバータ
    /// </summary>
    public class DragKeyConverter
    {
        private const char _modifierDelimiter = '+';

        /// <summary>
        ///  文字列からマウスドラッグアクションに変換する
        /// </summary>
        /// <param name="source">ジェスチャ文字列</param>
        /// <returns>DragKey。変換に失敗したときは NotSupportedException 例外が発生</returns>
        public static DragKey ConvertFromString(string source)
        {
            // ex. LeftButton
            // ex. Ctrl+XButton1+LeftButton

            if (string.IsNullOrEmpty(source)) return DragKey.Empty;

            // １操作のみサポート
            source = source.Split(',').First();

            // ～Drag → ～Button
            source = source.Replace("Drag", "Button", StringComparison.Ordinal);

            var keys = source.Split('+');

            ModifierKeys modifierKeys = ModifierKeys.None;
            MouseButtonBits mouseButtonBits = MouseButtonBits.None;

            foreach (var key in keys)
            {
                switch (key)
                {
                    case "Ctrl":
                        modifierKeys |= ModifierKeys.Control;
                        continue;
                }

                if (Enum.TryParse<ModifierKeys>(key, out ModifierKeys modifierKeysOne))
                {
                    modifierKeys |= modifierKeysOne;
                    continue;
                }

                if (Enum.TryParse<MouseButtonBits>(key, out MouseButtonBits bit))
                {
                    mouseButtonBits |= bit;
                    continue;
                }

                throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, TextResources.GetString("NotSupportedKeyException.Message"), source, "DragKey"));
            }

            //
            if (mouseButtonBits == MouseButtonBits.None)
            {
                throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, TextResources.GetString("NotSupportedKeyException.Message"), source, "DragKey"));
            }

            return new DragKey(mouseButtonBits, modifierKeys);
        }

        /// <summary>
        ///  マウスドラッグアクションから文字列に変換する
        /// </summary>
        public static string ConvertToString(DragKey gesture)
        {
            if (!gesture.IsValid) return "";

            string text = "";

            foreach (ModifierKeys key in Enum.GetValues(typeof(ModifierKeys)))
            {
                if ((gesture.ModifierKeys & key) != ModifierKeys.None)
                {
                    text += _modifierDelimiter + ((key == ModifierKeys.Control) ? "Ctrl" : key.ToString());
                }
            }

            text += _modifierDelimiter + string.Join(_modifierDelimiter, gesture.MouseButtonBits.ToString().Split(',').Select(e => e.Trim()));

            return text.TrimStart('+');
        }
    }


    public sealed class JsonDragKeyConverter : JsonConverter<DragKey>
    {
        public override DragKey? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            if (s is null) return DragKey.Empty;
            return DragKeyConverter.ConvertFromString(s);
        }

        public override void Write(Utf8JsonWriter writer, DragKey value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
