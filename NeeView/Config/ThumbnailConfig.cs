using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class ThumbnailConfig : BindableBase
    {
        public static bool IsVideoThumbnailSupported => Windows10Tools.IsWindows10_OrGreater(10240);

        private bool _isCacheEnabled = true;
        private TimeSpan _cacheLimitSpan;
        private BitmapImageFormat _format = BitmapImageFormat.Jpeg;
        private int _quality = 80;
        private int _thumbnailBookCapacity = 200;
        private int _thumbnailPageCapacity = 100;
        private int _imageWidth = 256;
#if USE_WINRT
        private bool _isVideoThumbnailEnabled = true;
#endif

        [JsonInclude, JsonPropertyName(nameof(ThumbnailCacheFilePath))]
        public string? _thumbnailCacheFilePath;


        [PropertyMember]
        public bool IsCacheEnabled
        {
            get { return _isCacheEnabled; }
            set { SetProperty(ref _isCacheEnabled, value); }
        }

        // キャッシュの保存場所
        [JsonIgnore]
        [PropertyPath(FileDialogType = FileDialogType.SaveFile, Filter = "DB|*.db")]
        public string ThumbnailCacheFilePath
        {
            get { return _thumbnailCacheFilePath ?? ThumbnailCache.DefaultThumbnailCacheFilePath; }
            set { SetProperty(ref _thumbnailCacheFilePath, string.IsNullOrWhiteSpace(value) || value.Trim() == ThumbnailCache.DefaultThumbnailCacheFilePath ? null : value.Trim()); }
        }

        /// <summary>
        /// キャッシュ制限(時間)
        /// </summary>
        [PropertyMember]
        public TimeSpan CacheLimitSpan
        {
            get { return _cacheLimitSpan; }
            set { SetProperty(ref _cacheLimitSpan, value < TimeSpan.Zero ? TimeSpan.Zero : value); }
        }

        /// <summary>
        /// 画像サイズ
        /// </summary>
        [PropertyRange(64, 512, TickFrequency = 8, IsEditable = true, Format="{0} × {0}")]
        public int ImageWidth
        {
            get { return _imageWidth; }
            set { SetProperty(ref _imageWidth, MathUtility.Max(value, 64)); }
        }

        /// <summary>
        /// 画像フォーマット
        /// </summary>
        [PropertyMember]
        public BitmapImageFormat Format
        {
            get { return _format; }
            set { SetProperty(ref _format, value); }
        }

        /// <summary>
        /// 画像品質
        /// </summary>
        [PropertyRange(5, 100, TickFrequency = 5)]
        public int Quality
        {
            get { return _quality; }
            set { SetProperty(ref _quality, MathUtility.Clamp(value, 5, 100)); }
        }

        [PropertyMember]
        public int ThumbnailBookCapacity
        {
            get { return _thumbnailBookCapacity; }
            set { SetProperty(ref _thumbnailBookCapacity, value); }
        }

        [PropertyMember]
        public int ThumbnailPageCapacity
        {
            get { return _thumbnailPageCapacity; }
            set { SetProperty(ref _thumbnailPageCapacity, value); }
        }

#if USE_WINRT
        [PropertyMember]
        public bool IsVideoThumbnailEnabled
        {
            get { return _isVideoThumbnailEnabled && IsVideoThumbnailSupported; }
            set { SetProperty(ref _isVideoThumbnailEnabled, value); }
        }
#endif




        #region Obsolete

        [Obsolete("no used"), Alternative(nameof(ImageWidth), 39)] // ver.39
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double Resolution
        {
            get { return 0.0; }
            set { ImageWidth = (int)value; }
        }

        #endregion

        /// <summary>
        /// サムネイル画像生成パラメータのハッシュ値
        /// </summary>
        public int GetThumbnailImageGenerateHash()
        {
            int hash = (int)ImageWidth;
            if (Format == BitmapImageFormat.Jpeg)
            {
                hash |= 0x40000000;
                hash |= Quality << 16;
            }
            return hash;
        }
    }
}
