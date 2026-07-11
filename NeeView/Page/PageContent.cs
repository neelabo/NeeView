using CommunityToolkit.Mvvm.ComponentModel;
using NeeLaboratory.Threading;
using NeeView.ComponentModel;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public abstract partial class PageContent : ObservableObject, IDataSource, IMemoryOwner, IMemoryElement, IDisposable
    {
        public static Size DefaultSize { get; } = new(480, 640);

        /// <summary>
        /// サイズ取得前のサイズ
        /// </summary>
        /// <reamerks>
        /// 直前の選択ページのサイズが保持される。Task
        /// サイズ確定前のLoadingページのサイズになる。
        /// </reamerks>
        public static Size UndefinedSize { get; set; } = DefaultSize;

        private ArchiveEntry _archiveEntry;
        private PageDataSource? _pageDataSource;
        private readonly BookMemoryService? _bookMemoryService;
        public PageContentState _state;
        private readonly AsyncLock _asyncLock = new();
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _disposedValue;
        private bool _isArchiveEntryResolved;

        public PageContent(ArchiveEntry archiveEntry, BookMemoryService? bookMemoryService)
        {
            _archiveEntry = archiveEntry;
            _bookMemoryService = bookMemoryService;
        }


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

        public virtual bool IsBook => PageType.IsFolder();

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
                    OnPropertyChanged(nameof(Data));
                    OnPropertyChanged(nameof(DataSize));
                    OnPropertyChanged(nameof(ErrorMessage));
                    if (oldDataSource?.PictureInfo != _pageDataSource.PictureInfo)
                    {
                        OnPropertyChanged(nameof(PictureInfo));
                    }
                    if (oldDataSource?.Size != _pageDataSource.Size || oldDataSource?.AspectSize != _pageDataSource.AspectSize)
                    {
                        OnPropertyChanged(nameof(Size));
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

        public async Task<PageDataSource> LoadAsync(CancellationToken token)
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

        protected abstract Task<PageSource> LoadSourceAsync(CancellationToken token);

        public async Task<PictureInfo?> LoadPictureInfoAsync(CancellationToken token)
        {
            if (PictureInfo is not null)
            {
                return PictureInfo;
            }

            if (_disposedValue)
            {
                return new PictureInfo(DefaultSize);
            }

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

        protected virtual Task<PictureInfo?> LoadPictureInfoCoreAsync(CancellationToken token)
        {
            return Task.FromResult<PictureInfo?>(null);
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

        /// <summary>
        /// IsTemporary な ArchiveEntry を解決する
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<bool> ResolveArchiveEntryAsync(CancellationToken token)
        {
            if (_disposedValue) return false;

            if (_isArchiveEntryResolved)
            {
                return false;
            }

            _isArchiveEntryResolved = true;

            if (!_archiveEntry.IsTemporary)
            {
                return false;
            }

            var query = new QueryPath(_archiveEntry.SystemPath);
            query = query.ResolvePath();
            try
            {
                _archiveEntry = await ArchiveEntryUtility.CreateAsync(query.SimplePath, ArchiveHint.None, false, token);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ArchiveContent.Entry: {ex.Message}");
                return false;
            }
        }
    }

}
