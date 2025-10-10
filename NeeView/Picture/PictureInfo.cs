using NeeView.Media.Imaging.Metadata;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// 画像情報
    /// </summary>
    public class PictureInfo
    {
        private bool _isPixelInfoInitialized;
        private Size _aspectSize = Size.Empty;


        public PictureInfo()
        {
        }

        public PictureInfo(Size size)
        {
            Size = size;
            OriginalSize = size;
        }


        /// <summary>
        /// Bitmap画像のRaw情報
        /// </summary>
        public BitmapInfo? BitmapInfo { get; set; }

        /// <summary>
        /// 画像サイズ
        /// </summary>
        public Size Size { get; set; }

        /// <summary>
        /// 本来の画像サイズ
        /// </summary>
        public Size OriginalSize { get; set; }

        /// <summary>
        /// 画像サイズが制限された本来の画像サイズと異なる値である
        /// </summary>
        public bool IsLimited => Size != OriginalSize;

        /// <summary>
        /// 画像解像度を適用したサイズ
        /// </summary>
        public Size AspectSize
        {
            get => _aspectSize.IsEmpty ? Size : _aspectSize;
            set => _aspectSize = value;
        }

        /// <summary>
        /// Metadata
        /// </summary>
        public BitmapMetadataDatabase? Metadata { get; set; }

        /// <summary>
        /// Decoder
        /// </summary>
        public string? Decoder { get; set; }


        // 実際に読み込まないとわからないもの

        /// <summary>
        /// 基本色
        /// </summary>
        public Color Color { get; set; } = Colors.Black;

        /// <summary>
        /// ピクセル深度
        /// </summary>
        public int BitsPerPixel { get; set; }

        public bool IsPixelInfoEnabled => BitsPerPixel > 0;

        /// <summary>
        /// アルファ所持
        /// </summary>
        public bool? HasAlpha { get; set; }



        /// <summary>
        /// 画素情報。
        /// </summary>
        public void SetPixelInfo(BitmapSource bitmap)
        {
            // 設定は1回だけで良い
            if (_isPixelInfoInitialized) return;
            _isPixelInfoInitialized = true;

            // 補助情報なので重要度は低いので、取得できなくても問題ない。
            try
            {
                this.Color = bitmap.GetOneColor();
            }
            catch
            {
            }
        }

        public static async ValueTask<PictureInfo> CreateAsync(IStreamSource streamSource, string? decoder, CancellationToken token)
        {
            using (var stream = await streamSource.OpenStreamAsync(token))
            {
                return Create(stream, decoder);
            }
        }

        public static PictureInfo Create(Stream stream, string? decoder)
        {
            var bitmapInfo = BitmapInfo.Create(stream, true);
            return Create(bitmapInfo, decoder);
        }


        public static PictureInfo Create(BitmapInfo bitmapInfo, string? decoder)
        {
            var pictureInfo = new PictureInfo();

            pictureInfo.BitmapInfo = bitmapInfo;
            var originalSize = bitmapInfo.IsTranspose ? bitmapInfo.GetPixelSize().Transpose() : bitmapInfo.GetPixelSize();
            pictureInfo.OriginalSize = originalSize;

            var maxSize = bitmapInfo.IsTranspose ? Config.Current.Performance.MaximumSize.Transpose() : Config.Current.Performance.MaximumSize;
            var size = (Config.Current.Performance.IsLimitSourceSize && !maxSize.IsContains(originalSize)) ? originalSize.Uniformed(maxSize) : Size.Empty;
            pictureInfo.Size = size.IsEmpty ? originalSize : size;
            pictureInfo.AspectSize = bitmapInfo.IsTranspose ? bitmapInfo.GetAspectSize().Transpose() : bitmapInfo.GetAspectSize();

            pictureInfo.Decoder = decoder ?? "(Unknown)";
            pictureInfo.BitsPerPixel = bitmapInfo.BitsPerPixel;
            pictureInfo.Metadata = bitmapInfo.Metadata;

            pictureInfo.HasAlpha = bitmapInfo.HasAlpha;

            return pictureInfo;
        }


    }
}
