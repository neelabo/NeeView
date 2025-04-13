using System;

namespace NeeView
{
    public enum FontType
    {
        Message,
        Menu,
    }

    public static class FontTypeExtensions
    {
        public static double ToFontSize(this FontType type)
        {
            return type switch
            {
                FontType.Message => SystemVisualParameters.Current.MessageFontSize,
                FontType.Menu => SystemVisualParameters.Current.MenuFontSize,
                _ => throw new ArgumentOutOfRangeException(nameof(type)),
            };
        }
    }
}
