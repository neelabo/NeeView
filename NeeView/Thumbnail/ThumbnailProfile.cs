using NeeLaboratory.ComponentModel;
using PhotoSauce.MagicScaler;
using System;
using System.Windows;

namespace NeeView
{
    public class ThumbnailProfile : BindableBase
    {
        static ThumbnailProfile() => Current = new ThumbnailProfile();
        public static ThumbnailProfile Current { get; }

        private ThumbnailProfile()
        {
        }

        /// <summary>
        /// BitmapFactoryでの画像生成モード
        /// </summary>
        public BitmapCreateMode CreateMode { get; } = BitmapCreateMode.HighQuality;


        /// <summary>
        /// サムネイル画像サイズ取得
        /// </summary>
        /// <param name="size">元画像サイズ</param>
        /// <returns></returns>
        public static Size GetThumbnailSize(Size size)
        {
            var resolution = Config.Current.Thumbnail.ImageWidth;

            if (size.IsEmpty) return new Size(resolution, resolution);

            var pixels = resolution * resolution;

            var scale = Math.Sqrt(pixels / (size.Width * size.Height));

            var max = resolution * 2;
            if (size.Width * scale > max) scale = max / size.Width;
            if (size.Height * scale > max) scale = max / size.Height;
            if (scale > 1.0) scale = 1.0;

            var thumbnailSize = new Size(size.Width * scale, size.Height * scale);

            return thumbnailSize;
        }

        //
        public BitmapCreateSetting CreateBitmapCreateSetting(bool isOrientationEnabled)
        {
            var setting = new BitmapCreateSetting();
            setting.Mode = this.CreateMode;
            setting.ProcessImageSettings = new ProcessImageSettings()
            {
                HybridMode = HybridScaleMode.Turbo,
                MatteColor = System.Drawing.Color.White,
                OrientationMode = isOrientationEnabled ? OrientationMode.Normalize : OrientationMode.Ignore,
            };
            return setting;
        }

    }
}
