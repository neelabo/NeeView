using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using NeeLaboratory.Generators;
using NeeLaboratory.IO.Search;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class Page : IDisposable, INotifyPropertyChanged, IPageContentLoader, IPageThumbnailLoader, IHasPage, IRenameable, ISearchItem, IPendingItem
    {
        private int _index;
        private readonly ArchiveEntryNode _entryNode;
        private readonly PageContent _content;
        private bool _isVisible;
        private bool _isMarked;
        private bool _isDeleted;
        private bool _disposedValue;
        private int _pendingCount;

        public Page(PageContent content) : this(content, new ArchiveEntryNode(null, content.ArchiveEntry))
        {
        }

        public Page(PageContent content, ArchiveEntryNode entryNode)
        {
            Debug.Assert(content.ArchiveEntry == entryNode.ArchiveEntry);

            _entryNode = entryNode;

            _content = content;
            _content.ContentChanged += Content_ContentChanged;
            _content.SizeChanged += Content_SizeChanged;

            _thumbnailSource = PageThumbnailFactory.Create(_content);
        }


        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;

        [Subscribable]
        public event EventHandler? ContentChanged;

        [Subscribable]
        public event EventHandler? SizeChanged;



        public bool IsLoaded => _content.IsLoaded;

        public ArchiveEntry ArchiveEntry => _content.ArchiveEntry;

        public PageContent Content => _content;


        // 登録番号
        public int EntryIndex { get; set; }

        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
                _content.Index = value;
            }
        }

        // TODO: 表示番号と内部番号のずれ
        public int IndexPlusOne => Index + 1;

        // ブックのパス
        public string BookPath => _entryNode.RootSystemPath;

        // ページ名 : エントリ名
        public string EntryName => _entryNode.EntryName;

        // ページ名：ファイル名のみ
        public string EntryLastName => LoosePath.GetFileName(EntryName);

        // ページ名：スマートパス
        public string EntrySmartName => Prefix == null ? EntryName : EntryName[Prefix.Length..];

        // ページ名：フルパス名 (リンクはそのまま)
        public string EntryFullName => ArchiveEntry.Archive is MediaArchive ? BookPath : LoosePath.Combine(BookPath, EntryName);

        // ページ名：システムパス (リンクは実体に変換済)
        public string TargetPath => ArchiveEntry.TargetPath;

        // ページ名：スマート名用プレフィックス
        public string? Prefix { get; set; }

        // ファイル情報：ファイル作成日
        public DateTime CreationTime => ArchiveEntry != null ? ArchiveEntry.CreationTime : default;

        // ファイル情報：最終更新日
        public DateTime LastWriteTime => ArchiveEntry != null ? ArchiveEntry.LastWriteTime : default;

        // ファイル情報：ファイルサイズ
        public long Length => ArchiveEntry.Length;

        // コンテンツ幅
        public double Width => Size.Width;

        // コンテンツ高
        public double Height => Size.Height;

        /// <summary>
        /// コンテンツサイズ
        /// </summary>
        public Size Size => _content.Size;

        /// <summary>
        /// コンテンツカラー
        /// </summary>
        public Color Color => _content.Color;

        /// <summary>
        /// ページの種類
        /// </summary>
        public PageType PageType => _content.PageType;

        /// <summary>
        /// ブックとして開くことができる
        /// </summary>
        public bool IsBook => _content.IsBook;

        // 表示中?
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }

        public bool IsMarked
        {
            get { return _isMarked; }
            set { SetProperty(ref _isMarked, value); }
        }

        /// <summary>
        /// 要求状態
        /// </summary>
        public PageContentState State
        {
            get { return _content.State; }
            set { _content.State = value; }
        }

        /// <summary>
        /// 削除済フラグ
        /// </summary>
        public bool IsDeleted
        {
            get { return _isDeleted | ArchiveEntry.IsDeleted; }
            set { _isDeleted = value; }
        }

        /// <summary>
        /// 削除未確定カウント
        /// </summary>
        public int PendingCount
        {
            get { return _pendingCount; }
        }

        public string Detail
        {
            get
            {
                var s = new StringBuilder();
                s.AppendLine(BookPath);
                if (ArchiveEntry.Archive is not MediaArchive)
                {
                    s.AppendLine(EntryName);
                }
                s.Append(LastWriteTime.ToString());
                if (Length >= 0)
                {
                    s.AppendLine();
                    s.Append($"{Length / 1024:N0} KB");
                }
                return s.ToString();
            }
        }

        #region Thumbnail

        private readonly PageThumbnail _thumbnailSource;


        /// <summary>
        /// サムネイル
        /// </summary>
        public Thumbnail Thumbnail => _thumbnailSource.Thumbnail;

        public bool IsThumbnailValid => _thumbnailSource.Thumbnail.IsValid;

        /// <summary>
        /// サムネイル読み込み
        /// </summary>
        public async ValueTask<ImageSource?> LoadThumbnailAsync(CancellationToken token)
        {
            if (_disposedValue) return null;

            try
            {
                token.ThrowIfCancellationRequested();
                await _thumbnailSource.LoadAsync(token);
                return this.Thumbnail?.CreateImageSource();
            }
            catch
            {
                // nop.
                return null;
            }
        }

        #endregion


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _content.ContentChanged -= Content_ContentChanged;
                    _content.SizeChanged -= Content_SizeChanged;
                    _content.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Content_ContentChanged(object? sender, EventArgs e)
        {
            if (_disposedValue) return;

            RaisePropertyChanged(nameof(Content));
            RaisePropertyChanged(nameof(Color));
            ContentChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Content_SizeChanged(object? sender, EventArgs e)
        {
            if (_disposedValue) return;

            RaisePropertyChanged(nameof(Size));
            SizeChanged?.Invoke(this, EventArgs.Empty);
        }

        // TODO: これをPageのメソッドとして公開するのは？
        public async ValueTask LoadContentAsync(CancellationToken token)
        {
            if (_disposedValue) return;

            await _content.LoadAsync(token);
        }

        // TODO: PageLoader管理と競合している問題
        public void Unload()
        {
            if (_disposedValue) return;

            _content.Unload();
            ContentChanged?.Invoke(this, EventArgs.Empty);
        }

        public override string? ToString()
        {
            //return $"Page:{Index}, {EntryFullName}";
            return GetDisplayName(Config.Current.PageList.Format);
        }

        public Page? GetPage()
        {
            return this;
        }

        public SearchValue GetValue(SearchPropertyProfile profile, string? parameter, CancellationToken token)
        {
            switch (profile.Name)
            {
                case "text":
                    return new StringSearchValue(GetDisplayName(Config.Current.PageList.Format));
                case "date":
                    return new DateTimeSearchValue(LastWriteTime);
                case "size":
                    return new IntegerSearchValue(Length);
                case "playlist":
                    return new BooleanSearchValue(IsMarked);
                case "meta":
                    return new StringSearchValue(PageMetadataTools.GetValueString(this, parameter, token));
                case "rating":
                    return new IntegerSearchValue(PageMetadataTools.GetRating(this, token));
                default:
                    throw new NotSupportedException($"Not supported SearchProperty: {profile.Name}");
            }
        }

        public string GetDisplayName(PageNameFormat format)
        {
            return format switch
            {
                PageNameFormat.Smart => GetSmartlDisplayString(),
                PageNameFormat.NameOnly => EntryLastName,
                PageNameFormat.PageNumber => (Index + 1).ToString(),
                _ => EntryName,
            };
        }

        /// <summary>
        /// Page のコンテンツ PictureInfo を取得する
        /// </summary>
        /// <param name="pictureInfo">指定する場合にのみ設定する。Page から取得する場合は null</param>
        /// <returns>コンテンツ PictureInfo。存在しなければ null</returns>
        public PictureInfo? GetContentPictureInfo(PictureInfo? pictureInfo = null)
        {
            if (Content.IsFileContent) return null;
            return pictureInfo ?? Content.PictureInfo;
        }

        #region Page functions

        // ページ名：ソート用分割
        public string[] GetEntryNameTokens()
        {
            return LoosePath.Split(EntryName);
        }

        // ページ名：プレフィックスを除いたフルパス
        public string GetSmartlDisplayString()
        {
            return EntrySmartName.Replace("\\", " > ", StringComparison.Ordinal);
        }

        public string GetSmartDirectoryDisplayString()
        {
            return LoosePath.GetDirectoryName(EntrySmartName).Replace("\\", " > ", StringComparison.Ordinal);
        }

        // ファイルの場所を取得
        public string GetFilePlace()
        {
            return ArchiveEntry.EntityPath ?? ArchiveEntry.Archive.GetPlace();
        }

        // フォルダーを開く、で取得するパス
        public string GetFolderOpenPlace()
        {
            if (ArchiveEntry.Archive is FolderArchive)
            {
                return GetFilePlace();
            }
            else
            {
                return GetFolderPlace();
            }
        }

        // フォルダーの場所を取得
        public string GetFolderPlace()
        {
            return ArchiveEntry.Archive.GetSourceFileSystemPath();
        }

        /// <summary>
        /// can delete?
        /// </summary>
        public bool CanDelete(bool strict)
        {
            return ArchiveEntry.CanDelete(strict);
        }

        /// <summary>
        /// delete
        /// </summary>
        public async ValueTask<DeleteResult> DeleteAsync()
        {
            return await ArchiveEntry.DeleteAsync();
        }

        public string GetRenameText()
        {
            return ArchiveEntry.GetRenameText();
        }

        public bool CanRename()
        {
            return ArchiveEntry.CanRename();
        }

        public async ValueTask<bool> RenameAsync(string name)
        {
            if (_disposedValue) return false;

            var oldPath = EntryFullName;
            var isSuccess = await ArchiveEntry.RenameAsync(name);
            if (isSuccess)
            {
                RaiseNamePropertyChanged();
                FileInformation.Current.Update(); // TODO: 伝達方法がよろしくない

                // 名前変更をブックマーク等に反映
                var newPath = EntryFullName;
                BookMementoCollection.Current.RenameRecursive(oldPath, newPath);
                QuickAccessCollection.Current.RenameRecursive(oldPath, newPath);
                PlaylistHub.Current.RenameItemPathRecursive(oldPath, newPath);
            }
            return isSuccess;
        }

        private void RaiseNamePropertyChanged()
        {
            if (_disposedValue) return;

            RaisePropertyChanged(nameof(EntryName));
            RaisePropertyChanged(nameof(EntryLastName));
            RaisePropertyChanged(nameof(EntrySmartName));
            RaisePropertyChanged(nameof(EntryFullName));
            RaisePropertyChanged(nameof(TargetPath));
            RaisePropertyChanged(nameof(Detail));
        }

        public string GetMetaValue(string key, CancellationToken token)
        {
            return PageMetadataTools.GetValueString(this, key.ToLowerInvariant(), token);
        }

        public Dictionary<string, string> GetMetaValueMap(CancellationToken token)
        {
            return PageMetadataTools.GetValueStringMap(this, token);
        }

        public void IncrementPendingCount()
        {
            Interlocked.Increment(ref _pendingCount);
            ArchiveEntry.Archive.StartWatch();
            RaisePropertyChanged(nameof(PendingCount));
        }

        public void DecrementPendingCount()
        {
            Interlocked.Decrement(ref _pendingCount);
            ArchiveEntry.Archive.StopWatch();
            RaisePropertyChanged(nameof(PendingCount));
        }

        #endregion
    }

}
