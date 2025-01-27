using System;
using System.Text.Json.Serialization;
using System.IO;
using System.Text.Json;

namespace NeeView
{
    [ObjectMergeReferenceCopy]
    [JsonConverter(typeof(JsonThemeSourceConverter))]
    public class ThemeSource
    {
        public ThemeSource(ThemeType themeType)
        {
            if (themeType == ThemeType.Custom) throw new ArgumentException($"{nameof(themeType)} must not be {nameof(ThemeType.Custom)}.");

            Type = themeType;
            FileName = null;
        }

        public ThemeSource(ThemeType themeType, string? fileName)
        {
            if (themeType == ThemeType.Custom && string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException($"{ThemeType.Custom} requires {nameof(fileName)}.");

            if (themeType != ThemeType.Custom && fileName is not null)
                throw new ArgumentException($"{nameof(fileName)} cannot be set except for {ThemeType.Custom}.");

            Type = themeType;
            FileName = fileName;
        }

        public ThemeType Type { get; private set; }

        public string? FileName { get; private set; }

        public string CustomThemeFilePath
        {
            get
            {
                if (Type != ThemeType.Custom) throw new InvalidOperationException($"{nameof(CustomThemeFilePath)} is required ChstomTmeme");
                if (this.FileName is null) throw new InvalidOperationException("CustomTheme.FileName must not be null");
                return Path.Combine(Config.Current.Theme.CustomThemeFolder, this.FileName);
            }
        }



        public override string ToString()
        {
            return Type.ToString() + (FileName != null ? ("." + FileName) : "");
        }

        public static ThemeSource Parse(string? s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return new ThemeSource(ThemeType.Dark);
            }

            var tokens = s.Split(new char[] { '.' }, 2);
            var themeType = (ThemeType)Enum.Parse(typeof(ThemeType), tokens[0]);
            var fileName = tokens.Length >= 2 ? tokens[1] : null;

            if (themeType == ThemeType.Custom && fileName == null)
            {
                themeType = ThemeType.Dark;
            }

            var themeSource = new ThemeSource(themeType, fileName);
            return themeSource;
        }
    }



    public sealed class JsonThemeSourceConverter : JsonConverter<ThemeSource>
    {
        public override ThemeSource? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return ThemeSource.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, ThemeSource value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }


}
