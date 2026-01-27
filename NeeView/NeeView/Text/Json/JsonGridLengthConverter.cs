using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace NeeView.Text.Json
{
    /// <summary>
    /// GridLengthを文字列に変換する
    /// </summary>
    public sealed class JsonGridLengthConverter : JsonConverter<GridLength>
    {
        public override GridLength Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            if (s == null) return new GridLength();

            var instance = new GridLengthConverter().ConvertFromInvariantString(s) as GridLength?;
            if (instance == null) throw new InvalidCastException();

            return instance.Value;
        }

        public override void Write(Utf8JsonWriter writer, GridLength value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
