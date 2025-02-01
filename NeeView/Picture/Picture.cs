using NeeLaboratory.ComponentModel;
using NeeView.Media.Imaging;
using PhotoSauce.MagicScaler;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// エントリに対応する表示画像
    /// </summary>
    public class Picture : BindableBase
    {
        private readonly IPictureSource _pictureSource;

        /// <summary>
        /// リサイズパラメータのハッシュ。
        /// リサイズが必要かの判定に使用される
        /// </summary>
        private PictureSizeSource _sizeSource;

        /// <summary>
        /// ロックオブジェクト
        /// </summary>
        private readonly System.Threading.Lock _lock = new();


        public Picture(IPictureSource source)
        {
            _pictureSource = source;
            _sizeSource = new PictureSizeSource();
        }



        /// <summary>
        /// 画像情報
        /// </summary>
        public PictureInfo? PictureInfo => _pictureSource.PictureInfo;

        /// <summary>
        /// 表示する画像
        /// </summary>
        private ImageSource? _imageSource;
        public ImageSource? ImageSource
        {
            get { return _imageSource; }
            set { if (_imageSource != value) { _imageSource = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// 画像生成サイズ情報
        /// </summary>
        public PictureSizeSource SizeSource => _sizeSource;


        public long GetMemorySize()
        {
            if (_imageSource == null) return 0L;

            if (_imageSource is BitmapSource bitmapSource)
            {
                return (long)bitmapSource.Format.BitsPerPixel * bitmapSource.PixelWidth * bitmapSource.PixelHeight / 8;
            }
            else
            {
                return 1024 * 1024;
            }
        }

        // 画像生成に影響する設定のハッシュ値取得
        private static int GetEnvironmentHashCode()
        {
            return HashCode.Combine(Config.Current.ImageResizeFilter, Config.Current.ImageCustomSize);
        }

#if false
        // Imageが同じサイズであるか判定
        private bool IsEqualImageSizeMaybe(Size size, bool keepAspectRatio)
        {
            if (this.ImageSource is null) return false;
            if (this.PictureInfo is null) return false;

            size = size.IsEmpty ? this.PictureInfo.Size : size;

            const double margin = 1.1;
            if (keepAspectRatio)
            {
                // アスペクト比固定のため、PixelHeightのみで判定
                return Math.Abs(size.Height - this.ImageSource.GetPixelHeight()) < margin;
            }
            else
            {
                return Math.Abs(size.Height - this.ImageSource.GetPixelHeight()) < margin && Math.Abs(size.Width - this.ImageSource.GetPixelWidth()) < margin;
            }
        }
#endif

        /// <summary>
        /// 生成済チェック
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool IsCreated(Size size)
        {
            return this.ImageSource is not null && _sizeSource == CreateSizeSource(size);
        }

        /// <summary>

        /// <summary>
        /// サイズ情報生成
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public PictureSizeSource? CreateSizeSource(Size size)
        {
            if (this.PictureInfo is null) return null;

            size = size.IsEmpty ? this.PictureInfo.Size : size;
            size = _pictureSource.FixedSize(size);

            // 規定サイズ判定
            if (!this.PictureInfo.IsLimited && size.IsEqualMaybe(this.PictureInfo.Size))
            {
                size = Size.Empty;
            }

            int filterHashCode = GetEnvironmentHashCode(); // PDFやSVGには関係ないけどまあいっか？

            return new PictureSizeSource(size, filterHashCode, false);
        }


        /// <summary>
        /// ImageSource生成。
        /// サイズを指定し、必要であれば作り直す。不要であればなにもしない。
        /// </summary>
        /// TODO: 名前は UpdateImageSource のほうがふさわしい
        public async Task<bool> CreateImageSourceAsync(object data, Size size, CancellationToken token)
        {
            var sizeSource = CreateSizeSource(size);
            if (sizeSource is null) return false;

            if (this.ImageSource is not null && _sizeSource == sizeSource)
            {
                //Debug.WriteLine($"Equals SizeSource");
                return false;
            }

            // Debug.WriteLine($"## PDF: {_sizeSource.Size:f2}");

#if false
            Debug.WriteLine($"Resize: {this.PictureSource.ArchiveEntry.EntryLastName}");
            var nowSize = new Size(this.PictureInfo.BitmapInfo.PixelWidth, this.PictureInfo.BitmapInfo.PixelHeight);
            Debug.WriteLine($"Resize: {isDirtyResizeParameter}: {nowSize.Truncate()} -> {size.Truncate()}");
            Debug.WriteLine($"BMP: {this.PictureSource.ArchiveEntry.EntryName}: {this.PictureInfo.Size} -> {size}");
#endif

            var image = await CreateImageSourceAsync(data, sizeSource.Size, sizeSource.IsKeepAspectRatio, token);
            if (image == null)
            {
                return false;
            }

            lock (_lock)
            {
                _sizeSource = sizeSource;
                this.ImageSource = image;
            }

            return true;
        }

        public void ClearImageSource()
        {
            lock (_lock)
            {
                _sizeSource = new PictureSizeSource();
                this.ImageSource = null;
            }
        }

        private async Task<ImageSource> CreateImageSourceAsync(object data, Size size, bool keepAspectRatio, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var setting = new BitmapCreateSetting();

            if (!size.IsEmpty)
            {
                setting.IsKeepAspectRatio = keepAspectRatio;
                if (Config.Current.ImageResizeFilter.IsEnabled)
                {
                    setting.Mode = BitmapCreateMode.HighQuality;
                    setting.ProcessImageSettings = Config.Current.ImageResizeFilter.CreateProcessImageSetting();
                    setting.ProcessImageSettings.OrientationMode = this.PictureInfo?.Metadata?.IsOrientationEnabled == true ? OrientationMode.Normalize : OrientationMode.Ignore;
                }
            }

            return await MemoryControl.Current.RetryFuncWithMemoryCleanupAsync(_pictureSource.CreateImageSourceAsync(data, size, setting, token));
        }
    }

}
