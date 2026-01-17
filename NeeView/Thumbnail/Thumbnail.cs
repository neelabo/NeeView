//#define LOCAL_DEBUG

using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// サムネイル.
    /// Jpegで保持し、必要に応じてBitmapSourceを生成
    /// </summary>
    [LocalDebug]
    public partial class Thumbnail : BindableBase, IThumbnail, IDisposable
    {
        /// <summary>
        /// 開発用：キャッシュ読み込み無効
        /// </summary>
        public static bool DebugIgnoreCache { get; set; }

        /// <summary>
        /// 開発用：シリアル番号
        /// </summary>
        private static int _serialCount;
        private readonly int _serialNumber = _serialCount++;
        public int SerialNumber => _serialNumber;

        /// <summary>
        /// キャシュ用ヘッダ
        /// </summary>
        private ThumbnailCacheHeader _header;

        /// <summary>
        /// 排他制御ロック
        /// </summary>
        private readonly System.Threading.Lock _lock = new();

        /// <summary>
        /// サムネイルデータ
        /// </summary>
        /// <remarks>
        /// 非同期に値が変更される可能性があるので取り扱い注意
        /// </remarks>
        private byte[]? _image;

        /// <summary>
        /// ImageSource (寿命あり)
        /// </summary>
        private ImageSource? _imageSource;

        /// <summary>
        /// ImageSource 生成中
        /// </summary>
        private int _imageSourceLoading;


        public Thumbnail(ArchiveEntry entry)
        {
            // NOTE: シンボリックリンクはターゲットのファイル情報を使う
            // NOTE: ディレクトリは更新日をサイズとする

            try
            {
                var linkTarget = entry.ResolveLinkTarget();
                if (linkTarget is not null)
                {
                    if (linkTarget.Exists)
                    {
                        var path = linkTarget.FullName;
                        var length = linkTarget.Attributes.HasFlag(FileAttributes.Directory) ? linkTarget.GetSafeLastWriteTime().ToBinary() : (linkTarget as FileInfo)?.Length ?? 0;
                        var ghash = Config.Current.Thumbnail.GetThumbnailImageGenerateHash();
                        _header = new ThumbnailCacheHeader(path, length, null, ghash);
                    }
                    else
                    {
                        _header = ThumbnailCacheHeader.None;
                    }
                }
                else
                {
                    var path = entry.TargetPath;
                    var length = entry.IsDirectory ? entry.LastWriteTime.ToBinary() : entry.Length;
                    var ghash = Config.Current.Thumbnail.GetThumbnailImageGenerateHash();
                    _header = new ThumbnailCacheHeader(path, length, null, ghash);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _header = ThumbnailCacheHeader.None;
            }
        }


        /// <summary>
        /// 参照イベント
        /// </summary>
        [Subscribable]
        public event EventHandler? Touched;


        /// <summary>
        /// 寿命間利用シリアルナンバー
        /// </summary>
        public int LifeSerial { get; set; }

        /// <summary>
        /// キャッシュ使用
        /// </summary>
        public bool IsCacheEnabled { get; set; }

        /// <summary>
        /// キャッシュの読み込み許可フラグ
        /// </summary>
        /// <remarks>
        /// 再生成などでキャッシュ読み込みを回避したい場合に false にする。
        /// 初期化されると true になる。
        /// </remarks>
        public bool IsCacheReadEnabled { get; set; } = true;

        /// <summary>
        /// 有効判定
        /// </summary>
        public bool IsValid => _image != null;

        /// <summary>
        /// Jpeg化された画像
        /// </summary>
        public byte[]? Image
        {
            get { return _image; }
            private set
            {
                if (_disposedValue) return;
                if (_image != value)
                {
                    _image = value;
                    _imageSource = null;
                    LocalWriteLine($"Image={_image is not null}");
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(IsValid));
                    if (_image != null)
                    {
                        Touched?.Invoke(this, EventArgs.Empty);
                        RaisePropertyChanged(nameof(ImageSource));
                        RaisePropertyChanged(nameof(IsUniqueImage));
                        RaisePropertyChanged(nameof(IsNormalImage));
                        RaisePropertyChanged(nameof(Background));
                    }
                }
            }
        }

        /// <summary>
        /// ImageSource
        /// </summary>
        /// <remarks>
        /// 非同期でのImageSource生成トリガにもなる
        /// </remarks>
        public ImageSource? ImageSource
        {
            get
            {
                LoadImageSourceAsync();
                return _imageSource;
            }
        }

        /// <summary>
        /// ユニークイメージ？
        /// </summary>
        public bool IsUniqueImage
        {
            get
            {
                var image = _image;
                return image == null || (image != ThumbnailResource.EmptyImage && image != ThumbnailResource.MediaImage && image != ThumbnailResource.FolderImage && image != ThumbnailResource.NoEntryImage);
            }
        }

        /// <summary>
        /// 標準イメージ？
        /// バナーでの引き伸ばし許可
        /// </summary>
        public bool IsNormalImage
        {
            get
            {
                var image = _image;
                return image == null || (image != ThumbnailResource.MediaImage && image != ThumbnailResource.FolderImage);
            }
        }

        /// <summary>
        /// View用Bitmapの背景プロパティ
        /// </summary>
        public Brush Background
        {
            get
            {
                if (_image == ThumbnailResource.MediaImage)
                {
                    return ThumbnailResource.MediaBackground;
                }
                else
                {
                    return Brushes.Transparent;
                }
            }
        }


        private void LoadImageSourceAsync()
        {
            if (_imageSource is not null) return;

            var image = _image;
            if (image is null) return;

            if (_imageSourceLoading > 0) return;
            Interlocked.Increment(ref _imageSourceLoading);

            LocalWriteLine("Create ImageSourceAsync...");
            Task.Run(() =>
            {
                try
                {
                    var imageSource = CreateImageSource(image);
                    if (imageSource is not null)
                    {
                        lock (_lock)
                        {
                            _imageSource = imageSource;
                            ThumbnailLifetimeManagement.Current.Add(this);
                        }
                        LocalWriteLine("Create ImageSourceAsync done.");
                        RaisePropertyChanged(nameof(ImageSource));
                    }
                    else
                    {
                        LocalWriteLine("Create ImageSourceAsync is null");
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref _imageSourceLoading);
                }
            });
        }


        /// <summary>
        /// ImageSource 開放
        /// </summary>
        public void RemoveImageSource()
        {
            lock (_lock)
            {
                LocalWriteLine("Remove ImageSource");
                _imageSource = null;
            }
        }

        /// <summary>
        /// キャッシュを使用してサムネイル生成を試みる
        /// </summary>
        public async ValueTask InitializeFromCacheAsync(CancellationToken token)
        {
            if (_disposedValue) return;
            if (IsValid || !IsCacheEnabled) return;
            if (!IsCacheReadEnabled) return;

#if DEBUG
            if (DebugIgnoreCache)
            {
                Image = null;
                return;
            }
#endif
            var image = await ThumbnailCache.Current.LoadAsync(_header, token);
            LocalWriteLine($"Initialize: from cache={_header.Key}: {(image == null ? "Miss" : "Hit!")}");
            Image = image;
        }

        /// <summary>
        /// サムネイルデータを引き継いで初期化
        /// </summary>
        /// <param name="thumbnail"></param>
        public void Initialize(Thumbnail thumbnail)
        {
            if (_disposedValue) return;
            if (!thumbnail.IsValid) return;

            Initialize(thumbnail._image);
            _imageSource = thumbnail._imageSource;
            if (_imageSource is not null)
            {
                RaisePropertyChanged(nameof(ImageSource));
            }

            IsCacheReadEnabled = true;
        }

        /// <summary>
        /// 画像データから初期化
        /// </summary>
        /// <param name="source"></param>
        public void Initialize(byte[]? image)
        {
            if (_disposedValue) return;
            if (IsValid) return;

            LocalWriteLine($"Initialize: from binary={image?.Length ?? 0} byte");
            Image = image ?? ThumbnailResource.EmptyImage;

            SaveCacheAsync();
            
            IsCacheReadEnabled = true;
        }

        /// <summary>
        /// サムネイル基本タイプから初期化
        /// </summary>
        /// <param name="type"></param>
        public void Initialize(ThumbnailType type)
        {
            if (_disposedValue) return;

            LocalWriteLine($"Initialize: from type={type}");
            Image = type switch
            {
                ThumbnailType.Media => ThumbnailResource.MediaImage,
                ThumbnailType.Folder => ThumbnailResource.FolderImage,
                ThumbnailType.NoEntry => ThumbnailResource.NoEntryImage,
                _ => ThumbnailResource.EmptyImage,
            };

            IsCacheReadEnabled = true;
        }

        /// <summary>
        /// サムネイルソースから初期化
        /// </summary>
        /// <param name="source"></param>
        public void Initialize(ThumbnailSource source)
        {
            if (_disposedValue) return;

            if (source == null)
            {
                Initialize((byte[]?)null);
            }
            else if (source.Type == ThumbnailType.Unique)
            {
                Initialize(source.RawData);
            }
            else
            {
                Initialize(source.Type);
            }

            IsCacheReadEnabled = true;
        }

        /// <summary>
        /// キャッシュに保存
        /// </summary>
        public void SaveCacheAsync()
        {
            if (_disposedValue) return;
            if (!IsCacheEnabled || _header == null) return;

            var image = _image;
            if (image == null || image == ThumbnailResource.EmptyImage || image == ThumbnailResource.MediaImage || image == ThumbnailResource.FolderImage || image == ThumbnailResource.NoEntryImage) return;

            ThumbnailCache.Current.EntrySaveQueue(_header, image);
        }

        /// <summary>
        /// image無効
        /// </summary>
        public void Clear()
        {
            // 通知は不要なので直接パラメータ変更
            _image = null;
        }

        /// <summary>
        /// Touch
        /// </summary>
        public void Touch()
        {
            Touched?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// ImageSource生成
        /// </summary>
        public ImageSource? CreateImageSource()
        {
            return CreateImageSource(_image);
        }

        /// <summary>
        /// ImageSource生成
        /// </summary>
        private ImageSource? CreateImageSource(byte[]? image)
        {
            if (_disposedValue) return null;
            if (image is null) return null;

            Touched?.Invoke(this, EventArgs.Empty);
            if (image == ThumbnailResource.EmptyImage)
            {
                return ThumbnailResource.EmptyImageSource;
            }
            else if (image == ThumbnailResource.MediaImage)
            {
                return ThumbnailResource.MediaBitmapSource;
            }
            else if (image == ThumbnailResource.FolderImage)
            {
                return ThumbnailResource.FolderBitmapSource;
            }
            else if (image == ThumbnailResource.NoEntryImage)
            {
                return ThumbnailResource.NoEntryImageSource;
            }
            else
            {
                return DecodeFromImageData(image);
            }
        }

        public static ImageSource? CreateImageSource(ThumbnailType type)
        {
            switch (type)
            {
                case ThumbnailType.Empty:
                    return ThumbnailResource.EmptyImageSource;
                case ThumbnailType.Media:
                    return ThumbnailResource.MediaBitmapSource;
                case ThumbnailType.Folder:
                    return ThumbnailResource.FolderBitmapSource;
                case ThumbnailType.NoEntry:
                    return ThumbnailResource.NoEntryImageSource;
                default:
                    return null;
            }
        }

        public ThumbnailSource CreateSource()
        {
            var image = _image;
            if (image == ThumbnailResource.EmptyImage)
            {
                return new ThumbnailSource(ThumbnailType.Empty);
            }
            else if (image == ThumbnailResource.MediaImage)
            {
                return new ThumbnailSource(ThumbnailType.Media);
            }
            else if (image == ThumbnailResource.FolderImage)
            {
                return new ThumbnailSource(ThumbnailType.Folder);
            }
            else if (image == ThumbnailResource.NoEntryImage)
            {
                return new ThumbnailSource(ThumbnailType.NoEntry);
            }
            else
            {
                return new ThumbnailSource(ThumbnailType.Unique, image);
            }
        }


        /// <summary>
        /// ImageData to BitmapSource
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private static BitmapSource DecodeFromImageData(byte[] image)
        {
            using var stream = new MemoryStream(image, false);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = stream;
            bitmap.CreateOptions = BitmapCreateOptions.None;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _image = null;
                    Touched = null;
                    ResetPropertyChanged();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        public override string ToString()
        {
            var name = _header?.Key ?? "(none)";
            return $"{name}: LifeSerial={LifeSerial}: Length={_image?.Length ?? 0:#,0}";
        }

        [Conditional("LOCAL_DEBUG")]
        private void LocalWriteLine(string s, params object[] args)
        {
            LocalDebug.WriteLine($"({_serialNumber}): " + s);
        }
    }
}
