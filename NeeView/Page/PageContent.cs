using NeeLaboratory.Generators;
using NeeLaboratory.Threading;
using NeeView.ComponentModel;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    [NotifyPropertyChanged]
    public abstract partial class PageContent : IDataSource, IMemoryOwner, IMemoryElement, INotifyPropertyChanged, IDisposable
    {
        public static Size DefaultSize { get; } = new(480, 640);

        /// <summary>
        /// サイズ取得前のサイズ
        /// </summary>
        /// <reamerks>
        /// 直前の選択ページのサイズが保持される。
        /// サイズ確定前のLoadingページのサイズになる。
        /// </reamerks>
        public static Size UndefinedSize { get; set; } = DefaultSize;

        private readonly ArchiveEntry _archiveEntry;
        private PageDataSource? _pageDataSource;
        private readonly BookMemoryService? _bookMemoryService;
        public PageContentState _state;
        private readonly AsyncLock _asyncLock = new();
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _disposedValue;

        public PageContent(ArchiveEntry archiveEntry, BookMemoryService? bookMemoryService)
        {
            _archiveEntry = archiveEntry;
            _bookMemoryService = bookMemoryService;
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler? ContentChanged;

        public event EventHandler? SizeChanged;


        public IMemoryOwner Owner => this;

        public long MemorySize => DataSize;

        public int Index { get; set; }

        /// <summary>
        /// 要求状態
        /// </summary>
        public PageContentState State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }

        public virtual PageType PageType => PageType.File;

        public virtual bool IsBook => PageType == PageType.Folder;

        public virtual bool IsFileContent => false;

        public ArchiveEntry ArchiveEntry => _archiveEntry;

        public BookMemoryService? BookMemoryService => _bookMemoryService;

        public PictureInfo? PictureInfo => PageDataSource.PictureInfo;

        public Size Size => PageDataSource.Size;

        public Color Color => PictureInfo?.Color ?? Colors.Black;

        public bool Decrypt { get; set; } = true;


        /// <summary>
        /// 表示に必要な情報セット
        /// </summary>
        /// <remarks>
        /// PictureInfo は一度だけ設定可能。
        /// Size は PictureInfo から求められるが、未設定のときは初回アクセス時に UndefinedSize が設定される。ページ切り替えのちらつき軽減用の特殊処理。
        /// </remarks>
        public PageDataSource PageDataSource
        {
            get
            {
                return _pageDataSource = _pageDataSource ?? CreatePageDataSource(null, null);
            }
            private set
            {
                if (_disposedValue) return;
                var oldDataSource = _pageDataSource;
                if (SetProperty(ref _pageDataSource, value))
                {
                    Debug.Assert(_pageDataSource is not null);
                    RaisePropertyChanged(nameof(Data));
                    RaisePropertyChanged(nameof(DataSize));
                    RaisePropertyChanged(nameof(ErrorMessage));
                    if (oldDataSource?.PictureInfo != _pageDataSource.PictureInfo)
                    {
                        RaisePropertyChanged(nameof(PictureInfo));
                    }
                    if (oldDataSource?.Size != _pageDataSource.Size || oldDataSource?.AspectSize != _pageDataSource.AspectSize)
                    {
                        RaisePropertyChanged(nameof(Size));
                        SizeChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        // TODO: これら使ってる？
        public object? Data => PageDataSource.Data;
        public long DataSize => PageDataSource.DataSize;
        public string? ErrorMessage => PageDataSource.ErrorMessage;
        public bool IsLoaded => PageDataSource.IsLoaded;
        public bool IsFailed => PageDataSource.IsFailed;
        public DataState DataState => PageDataSource.DataState;

        public bool IsMemoryLocked => _state != PageContentState.None;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private PageDataSource CreatePageDataSource(IDataSource? dataSource, PictureInfo? pictureInfo)
        {
            return new PageDataSource()
            {
                Data = dataSource?.Data,
                DataSize = dataSource?.DataSize ?? 0,
                ErrorMessage = dataSource?.ErrorMessage,
                PictureInfo = pictureInfo,
                Size = pictureInfo?.Size ?? UndefinedSize,
                AspectSize = pictureInfo?.AspectSize ?? UndefinedSize,
            };
        }

        public async ValueTask<PageDataSource> LoadAsync(CancellationToken token)
        {
            if (_disposedValue) throw new ObjectDisposedException(this.GetType().FullName);

            using (await _asyncLock.LockAsync(token))
            {
                if (IsLoaded)
                {
                    return PageDataSource;
                }

                try
                {
                    // TODO: LockAsync されているのでこの CancellationTokenSource 再生成する必要なさそう？
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = new CancellationTokenSource();
                    using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, token);
                    if (linkedTokenSource.Token.IsCancellationRequested)
                    {
                        return PageDataSource;
                    }
                    var source = await LoadSourceAsync(token);
                    SetSource(source);
                    return PageDataSource;
                }
                catch (OperationCanceledException)
                {
                    return PageDataSource;
                }
                finally
                {
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = null;
                }
            }
        }

        protected abstract ValueTask<PageSource> LoadSourceAsync(CancellationToken token);

        public async ValueTask<PictureInfo?> LoadPictureInfoAsync(CancellationToken token)
        {
            if (PictureInfo is not null) return PictureInfo;

            if (_disposedValue) return new PictureInfo(DefaultSize);

            using (await _asyncLock.LockAsync(token))
            {
                var pictureInfo = await LoadPictureInfoCoreAsync(token);
                if (PictureInfo is null && pictureInfo is not null)
                {
                    PageDataSource = CreatePageDataSource(PageDataSource, pictureInfo);
                }
                return pictureInfo;
            }
        }

        protected virtual async ValueTask<PictureInfo?> LoadPictureInfoCoreAsync(CancellationToken token)
        {
            return await Task.FromResult<PictureInfo?>(null);
        }

        public virtual void Unload()
        {
            _cancellationTokenSource?.Cancel();
            PageDataSource = CreatePageDataSource(null, PictureInfo);
        }

        private void SetSource(PageSource source)
        {
            if (_disposedValue) return;

            if (source.PictureInfo is not null && source.PictureInfo.Size.IsEmptyArea())
            {
                SetSource(PageSource.CreateError("Image area is empty"));
                return;
            }

            PageDataSource = CreatePageDataSource(source, PictureInfo ?? source.PictureInfo);

            if (MemorySize > 0)
            {
                _bookMemoryService?.AddPageContent(this);
            }

            ContentChanged?.Invoke(this, EventArgs.Empty);
        }
    }

}
