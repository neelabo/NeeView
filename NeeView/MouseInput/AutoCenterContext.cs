using System.Windows;

namespace NeeView
{
    /// <summary>
    /// 拡大縮小の中心を自動で求めるための情報
    /// </summary>
    public static class AutoCenterContext
    {
        public static double RateX { get; set; } = 0.5;
        public static double RateY { get; set; } = 0.5;
        public static bool CanAutoCenterX => !Config.Current.Book.IsPanorama || Config.Current.Book.Orientation != PageFrameOrientation.Horizontal;
        public static bool CanAutoCenterY => !Config.Current.Book.IsPanorama || Config.Current.Book.Orientation != PageFrameOrientation.Vertical;

        public static void ResetScaleCenter(HorizontalAlignment horizontalOrigin, VerticalAlignment verticalOrigin)
        {
            RateX = horizontalOrigin switch
            {
                HorizontalAlignment.Right => 0.0,
                HorizontalAlignment.Left => 1.0,
                _ => 0.5,
            };

            RateY = verticalOrigin switch
            {
                VerticalAlignment.Top => 0.0,
                VerticalAlignment.Bottom => 1.0,
                _ => 0.5,
            };
        }
    }

}
