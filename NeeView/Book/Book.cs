using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NeeView
{
    public partial class Book : IDisposable, IBook
    {
        public static Book? Default { get; private set; }

        private readonly BookSource _source;
        private BookSettingConfig _setting;
        private readonly BookPageMarker _marker;
        private readonly string _sourcePath;
        private readonly BookLoadOption _loadOption;
        private readonly BookAddress _address;
        private List<Page> _currentPages = new();
        private readonly DisposableCollection _disposables = new();

        public Book(BookAddress address, BookSource source, BookMemento memento, BookLoadOption option, bool isNew, ArchiveHint archiveHint)
        {
            Book.Default = this;

            this.Memento = memento.Clone();
            this.Memento.IsRecursiveFolder = source.IsRecursiveFolder;
            this.Memento.SortMode = source.Pages.SortMode;

            _address = address;
            _source = source;
            _sourcePath = address.SourcePath?.SimplePath ?? "";
            _marker = new BookPageMarker(_source);
            _loadOption = option;

            var currentMemento = this.Memento.Clone();
            AttachBookSetting(currentMemento.ToBookSetting());
            Debug.Assert(_setting != null);

            _source.Pages.PagesSorted += (s, e) => PagesChanged?.Invoke(s, e);

            IsNew = isNew;
            ArchiveHint = archiveHint;

            _disposables.Add(_source);

            _disposables.Add(Config.Current.Book.SubscribePropertyChanged(nameof(BookConfig.FolderSortOrder),
                (s, e) => _source.Pages.UpdatePagesAsync()
            ));
        }



        [Subscribable]
        public event EventHandler? PagesChanged;

        [Subscribable]
        public event EventHandler<CurrentPageChangedEventArgs>? CurrentPageChanged;


        public BookSource Source => _source;
        public BookPageCollection Pages => _source.Pages;
        IReadOnlyList<Page> IBook.Pages => _source.Pages;


        public IReadOnlyList<Page> CurrentPages => _currentPages;
        public Page? CurrentPage => _currentPages.FirstOrDefault();
        public BookPageMarker Marker => _marker;
        public BookMemoryService BookMemoryService => _source.BookMemoryService;
        public BookAddress BookAddress => _address;
        public string Path => _source.Path;
        public string SourcePath => _sourcePath;
        public bool IsMedia => _source.IsMedia;
        public bool IsPlaylist => _source.IsPlaylist;
        public bool IsTemporary => _source.Path.StartsWith(Temporary.Current.TempDirectory, StringComparison.Ordinal);
        public PageSortModeClass PageSortModeClass => IsPlaylist ? PageSortModeClass.WithEntry : PageSortModeClass.Normal;
        public BookLoadOption LoadOption => _loadOption;

        // はじめて開く本
        public bool IsNew { get; private set; }

        // 見つからなかった開始ページ名。通知用。
        // TODO: 不要？
        public string? NotFoundStartPage { get; private set; }

        // 開始ページ
        // TODO: 再読込時に必要だが、なくすことできそう？
        public string? StartEntry { get; private set; }

        public bool IsKeepHistoryOrder => (_loadOption & BookLoadOption.KeepHistoryOrder) == BookLoadOption.KeepHistoryOrder;

        /// <summary>
        /// 初期ページ座標
        /// </summary>
        public DirectionalPagePosition StartPosition { get; private set; } = new();

        /// <summary>
        /// 初期Memento
        /// </summary>
        public BookMemento Memento { get; private set; }

        /// <summary>
        /// 設定
        /// </summary>
        public BookSettingConfig Setting => _setting;

        /// <summary>
        /// アーカイブヒント。再読み込みのときに使用される
        /// </summary>
        public ArchiveHint ArchiveHint { get; private set; }


        public void AttachBookSetting(BookSettingConfig setting)
        {
            if (_disposedValue) return;

            DetachBookSetting();

            // 新しい設定の反映
            _setting = setting;
            _setting.PropertyChanged += Setting_PropertyChanged;
            _source.IsRecursiveFolder = _setting.IsRecursiveFolder;
            _source.Pages.SortMode = _setting.SortMode;
        }

        private void DetachBookSetting()
        {
            if (_setting is null) return;

            _setting.PropertyChanged -= Setting_PropertyChanged;
        }

        private void Setting_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_disposedValue) return;
            if (sender is not BookSettingConfig setting) return;

            var isAll = string.IsNullOrEmpty(e.PropertyName);
            if (isAll || e.PropertyName == nameof(setting.IsRecursiveFolder))
            {
                _source.IsRecursiveFolder = setting.IsRecursiveFolder;
            }
            if (isAll || e.PropertyName == nameof(setting.SortMode))
            {
                _source.Pages.SortMode = setting.SortMode;
            }
        }

        public void SetStartPage(object? sender, BookStartPage startPage)
        {
            if (_disposedValue) return;

            // スタートページ取得
            PagePosition position = _source.Pages.FirstPosition();
            int direction = 1;
            if (startPage.StartPageType == BookStartPageType.FirstPage)
            {
                position = _source.Pages.FirstPosition();
                direction = 1;
            }
            else if (startPage.StartPageType == BookStartPageType.LastPage)
            {
                position = _source.Pages.LastPosition();
                direction = -1;
            }
            else
            {
                int index = !string.IsNullOrEmpty(startPage.PageName) ? _source.Pages.FindIndex(e => e.EntryName == startPage.PageName) : 0;
                if (index < 0)
                {
                    this.NotFoundStartPage = startPage.PageName;
                }
                position = index >= 0 ? new PagePosition(index, 0) : _source.Pages.FirstPosition();
                direction = 1;

                // 最終ページリセット
                // NOTE: ワイドページ判定は行わないため、2ページモードの場合に不正確な場合がある
                int lastPageOffset = (Config.Current.GetFramePageSize(_setting.PageMode) == 2 && !_setting.IsSupportedSingleLastPage) ? 1 : 0;
                if (startPage.IsResetLastPage && index >= _source.Pages.LastPosition().Index - lastPageOffset)
                {
                    position = _source.Pages.FirstPosition();
                }
            }

            // 開始ページ記憶
            this.StartEntry = _source.Pages.Count > 0 ? _source.Pages[position.Index].EntryName : null;

            //this.Memento.Page = this.StartEntry ?? "";
            var page = _source.Pages.Count > 0 ? _source.Pages[position.Index] : null;
            if (page is not null)
            {
                SetCurrentPages(new[] { page });
            }

            // 初期ページ設定 
            StartPosition = new DirectionalPagePosition(position, direction);
        }

        public void Start()
        {
            if (_disposedValue) return;

            // TODO: スタートページへ移動
            //_controller.Start();
        }


        public void SetCurrentPages(IEnumerable<Page> pages)
        {
            if (_disposedValue) return;
            if (_currentPages.SequenceEqual(pages)) return;

            var oldPages = _currentPages;
            _currentPages = pages.ToList();

            var eventArgs = new CurrentPageChangedEventArgs(_currentPages, oldPages);
            CurrentPageChanged?.Invoke(this, eventArgs);
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        public bool IsDisposed => _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    DetachBookSetting();

                    if (Book.Default == this)
                    {
                        Book.Default = null;
                    }

                    _disposables.Dispose();
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

        #region Memento

        // bookの設定を取得する
        public BookMemento CreateMemento()
        {
            var memento = new BookMemento
            {
                Path = _source.Path,
                Page = this.CurrentPage?.EntryName ?? "",

                PageMode = _setting.PageMode,
                BookReadOrder = _setting.BookReadOrder,
                IsSupportedDividePage = _setting.IsSupportedDividePage,
                IsSupportedSingleFirstPage = _setting.IsSupportedSingleFirstPage,
                IsSupportedSingleLastPage = _setting.IsSupportedSingleLastPage,
                IsSupportedWidePage = _setting.IsSupportedWidePage,
                IsRecursiveFolder = _source.IsRecursiveFolder,
                SortMode = _source.Pages.SortMode,
                SortSeed = _source.Pages.SortSeed,
                AutoRotate = _setting.AutoRotate,
                BaseScale = _setting.BaseScale,
            };

            return memento;
        }

#if false
        // bookに設定を反映させる
        public void Restore(BookMemento memento)
        {
            if (memento == null) return;

            if (_disposedValue) return;

            _setting.PageMode = memento.PageMode;
            _setting.BookReadOrder = memento.BookReadOrder;
            _setting.IsSupportedDividePage = memento.IsSupportedDividePage;
            _setting.IsSupportedSingleFirstPage = memento.IsSupportedSingleFirstPage;
            _setting.IsSupportedSingleLastPage = memento.IsSupportedSingleLastPage;
            _setting.IsSupportedWidePage = memento.IsSupportedWidePage;
            _source.IsRecursiveFolder = memento.IsRecursiveFolder;
            _source.Pages.SortMode = memento.SortMode;
            _setting.AutoRotate = memento.AutoRotate;
            _setting.BaseScale = memento.BaseScale;
        }
#endif

        #endregion
    }


    public class DirectionalPagePosition
    {
        public DirectionalPagePosition() : this(PagePosition.Zero, 1)
        {
        }

        public DirectionalPagePosition(PagePosition position, int direction)
        {
            Debug.Assert(!position.IsEmpty());
            Debug.Assert(direction is -1 or +1);
            Position = position;
            Direction = direction == -1 ? -1 : +1;
        }

        public PagePosition Position { get; init; }
        public int Direction { get; init; }
    }


    public class CurrentPageChangedEventArgs : EventArgs
    {
        public CurrentPageChangedEventArgs(List<Page> newValue, List<Page> oldValue)
        {
            NewValue = newValue;
            OldValue = oldValue;
        }

        public List<Page> NewValue { get; init; }
        public List<Page> OldValue { get; init; }

        public bool IsTopPageChanged => NewValue.FirstOrDefault() != OldValue.FirstOrDefault();
    }
}
