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
    public class WindowPlacement
    {
        public static WindowPlacement None { get; } = new WindowPlacement();

        public WindowPlacement()
        {
        }

        public WindowPlacement(WindowState windowState, int left, int top, int width, int height)
        {
            WindowState = windowState;
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }

        public WindowPlacement(WindowState windowState, int left, int top, int width, int height, bool isFullScreen) : this(windowState, left, top, width, height)
        {
            IsFullScreen = isFullScreen;

            if (isFullScreen)
            {
                WindowState = WindowState.Maximized;
            }
        }


        public WindowState WindowState { get; private set; }
        public int Left { get; private set; }
        public int Top { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public bool IsFullScreen { get; private set; }

        public int Right => Left + Width;
        public int Bottom => Top + Height;


        public bool IsValid()
        {
            return Width > 0 || Height > 0;
        }

        public WindowPlacement WithIsFullScreen(bool isFullScreen)
        {
            return new WindowPlacement(this.WindowState, this.Left, this.Top, this.Width, this.Height, isFullScreen);
        }

        public WindowPlacement WithState(WindowState state)
        {
            var isFullScreen = state == WindowState.Maximized && this.IsFullScreen;
            return new WindowPlacement(state, this.Left, this.Top, this.Width, this.Height, isFullScreen);
        }

        public WindowPlacement WithState(WindowState state, bool isFullScreen)
        {
            isFullScreen = state == WindowState.Maximized && isFullScreen;
            return new WindowPlacement(state, this.Left, this.Top, this.Width, this.Height, isFullScreen);
        }

        public override string ToString()
        {
            var state = IsFullScreen ? "FullScreen" : WindowState.ToString();
            return $"{state},{Left},{Top},{Width},{Height}";
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

            bool isFullScreen;
            WindowState windowState;
            if (tokens[0] == "FullScreen")
            {
                windowState = WindowState.Maximized;
                isFullScreen = true;
            }
            else
            {
                windowState = (WindowState)(Enum.Parse(typeof(WindowState), tokens[0]));
                isFullScreen = false;
            }

            var placement = new WindowPlacement(
                windowState,
                int.Parse(tokens[1], CultureInfo.InvariantCulture),
                int.Parse(tokens[2], CultureInfo.InvariantCulture),
                int.Parse(tokens[3], CultureInfo.InvariantCulture),
                int.Parse(tokens[4], CultureInfo.InvariantCulture),
                isFullScreen);

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
