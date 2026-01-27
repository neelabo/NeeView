using System.Windows;

namespace NeeView
{
    public static class DpiScaleExtensions
    {
        public static DpiScale OneDpiScale { get; } = new DpiScale(1, 1);

        /// <summary>
        /// IsIgnoreImageDpiを 適用した DpiScale に変換
        /// </summary>
        public static DpiScale ToFixedScale(this DpiScale self)
        {
            return Config.Current.System.IsIgnoreImageDpi ? self : OneDpiScale;
        }
    }
}
