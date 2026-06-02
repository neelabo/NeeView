using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace NeeView.Windows
{
    [ImmutableObject(true)]
    [JsonConverter(typeof(JsonWindowPlaceConverter))]
    public record WindowPlacement
    {
        public static WindowPlacement None { get; } = new WindowPlacement();

        public WindowPlacement()
        {
        }

        public WindowPlacement(WindowState windowState, int left, int top, int width, int height)
            : this(windowState.ToWindowStateEx(), left, top, width, height)
        {
        }

        public WindowPlacement(WindowStateEx windowStateEx, int left, int top, int width, int height) 
        {
            WindowStateEx = windowStateEx == WindowStateEx.None ? WindowStateEx.Normal : windowStateEx;
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }


        public WindowStateEx WindowStateEx { get; private set; }
        public WindowState WindowState => WindowStateEx.ToWindowState();

        public int Left { get; private set; }
        public int Top { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Right => Left + Width;
        public int Bottom => Top + Height;


        public bool IsValid()
        {
            return Width > 0 || Height > 0;
        }

        public WindowPlacement WithState(WindowStateEx state)
        {
            return new WindowPlacement(state, this.Left, this.Top, this.Width, this.Height);
        }

        public override string ToString()
        {
            return $"{WindowStateEx},{Left},{Top},{Width},{Height}";
        }

        public static WindowPlacement Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return WindowPlacement.None;

            var tokens = s.Split(',');
            if (tokens.Length != 5)
            {
                Debug.WriteLine($"WindowPlacement.Parse(): InvalidCast: {s}");
                return WindowPlacement.None;
            }

            var windowStateEx = Enum.Parse<WindowStateEx>(tokens[0]);

            var placement = new WindowPlacement(
                windowStateEx,
                int.Parse(tokens[1], CultureInfo.InvariantCulture),
                int.Parse(tokens[2], CultureInfo.InvariantCulture),
                int.Parse(tokens[3], CultureInfo.InvariantCulture),
                int.Parse(tokens[4], CultureInfo.InvariantCulture));

            return placement;
        }


        public sealed class JsonWindowPlaceConverter : JsonConverter<WindowPlacement>
        {
            public override WindowPlacement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var s = reader.GetString();
                if (s is null) return WindowPlacement.None;

                return WindowPlacement.Parse(s);
            }

            public override void Write(Utf8JsonWriter writer, WindowPlacement value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.IsValid() ? value.ToString() : "");
            }
        }
    }

}
