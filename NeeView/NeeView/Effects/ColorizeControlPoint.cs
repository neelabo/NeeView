using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace NeeView.Effects
{
    [JsonConverter(typeof(JsonColorizeControlPointConverter))]
    public sealed class ColorizeControlPoint
    {
        public ColorizeControlPoint() : this(Colors.Black, 1.0)
        {
        }

        public ColorizeControlPoint(Color color, double strength)
        {
            Color = color;
            Strength = strength;
        }

        public ColorizeControlPoint(ColorizeControlPoint other)
        {
            Color = other.Color;
            Strength = other.Strength;
        }

        public Color Color { get; set; }
        public double Strength { get; set; }

        public bool ValueEquals(ColorizeControlPoint other)
        {
            return Color == other.Color
                && Strength == other.Strength;
        }
    }


    public sealed class JsonColorizeControlPointConverter : JsonConverter<ColorizeControlPoint>
    {
        public override ColorizeControlPoint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            if (s is null) throw new JsonException("Invalid ColorizeControlPoint format");
            
            var tokens = s.Split(',');
            if (tokens.Length != 2) throw new JsonException("Invalid ColorizeControlPoint format");

            var color = (Color)ColorConverter.ConvertFromString(tokens[0]);
            var strength = double.Parse(tokens[1], CultureInfo.InvariantCulture);

            return new ColorizeControlPoint(color, strength);
        }

        public override void Write(Utf8JsonWriter writer, ColorizeControlPoint value, JsonSerializerOptions options)
        {
            var s = value.Color.ToString(CultureInfo.InvariantCulture) + "," + value.Strength.ToString(CultureInfo.InvariantCulture);
            writer.WriteStringValue(s);
        }
    }
}
