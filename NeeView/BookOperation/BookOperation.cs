using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.PageFrames;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// 本の操作
    /// </summary>
    public partial class BookOperation : BindableBase
    {
        // System Object
        static BookOperation() => Current = new BookOperation();
        public static BookOperation Current { get; }


        private readonly PageFrameBoxPresenter _presenter;
        private readonly BookHub _bookHub;
        private Book? _book;
        private PageFrameBox? _box;
        private bool _isLoading;

        public BookPageTerminatorProxy _terminator = new();
        public BookControlProxy _bookControl = new();
        public BookPageControlProxy _control = new();
        public BookPlaylistControlProxy _playlist = new();


        private BookOperation()
        {
            _bookHub = BookHub.Current;

            _presenter = PageFrameBoxPresenter.Current;
            _presenter.PageFrameBoxChanging += Presenter_PageFrameBoxChanging;
            _presenter.PageFrameBoxChanged += Presenter_PageFrameBoxChanged;
        }


        // ブックが変更される
        [Subscribable]
        public event EventHandler<BookChangingEventArgs>? BookChanging;

        // ブックが変更された
        [Subscribable]
        public event EventHandler<BookChangedEventArgs>? BookChanged;


        public bool IsLoading => _presenter.IsLoading || _isLoading;

        public Book? Book => _book;

        public string? Address => Book?.Path;

        public BookControlProxy BookControl => _bookControl;

        public BookPlaylistControlProxy Playlist => _playlist;

        public BookPageControlProxy Control => _control;

        public IReadOnlyList<Page> ViewPages => _presenter.ViewPages;


        private void Presenter_PageFrameBoxChanging(object? sender, PageFrameBoxChangingEventArgs e)
        {
            _isLoading = true;
            SetBook(null);
            BookChanging?.Invoke(sender, e);
        }

        private void Presenter_PageFrameBoxChanged(object? sender, PageFrameBoxChangedEventArgs e)
        {
            SetBook(e.Box);
            _isLoading = false;
            BookChanged?.Invoke(sender, e);
        }


        private void SetBook(PageFrameBox? box)
        {
            if (_box == box) return;
            _box = box;
            _book = _box?.Book;

            _bookControl.SetSource(CreateBookController(_box));
            _control.SetSource(CreateController(_box));
            _playlist.SetSource(CreatePlaylistController(_box));
            _terminator.SetSource(CreatePageTerminator(_box));

            RaisePropertyChanged(nameof(Book));
            RaisePropertyChanged(nameof(Address));
        }

        private BookPageTerminator? CreatePageTerminator(PageFrameBox? box)
        {
            return box is null ? null : new BookPageTerminator(box, _control);
        }

        private static IBookControl? CreateBookController(PageFrameBox? box)
        {
            return box is null ? null : new BookControl(box);
        }

        private IBookPageControl? CreateController(PageFrameBox? box)
        {
            Debug.Assert(_presenter != null);
            return box is null ? null : new BookPageControl(box, _bookControl);
        }

        private BookPlaylistControl? CreatePlaylistController(PageFrameBox? box)
        {
            return (box is null || box.Book.IsMedia) ? null : new BookPlaylistControl(box, _control);
        }



        #region BookCommand : ページ操作

        // パスを指定して移動
        public bool JumpPageWithPath(object? sender, string path)
        {
            if (this.Book == null || this.Book.IsMedia) return false;

            var page = this.Book.Pages.GetPageWithEntryFullName(path);
            if (page is null) return false;
            _control.MoveTo(sender, page.Index);
            return true;
        }

        // ターゲットパスを指定して移動
        public bool JumpPageWithTarget(object? sender, string path)
        {
            if (this.Book == null || this.Book.IsMedia) return false;

            var page = this.Book.Pages.GetPageWithTarget(path);
            if (page is null) return false;
            _control.MoveTo(sender, page.Index);
            return true;
        }

        // ページを指定して移動
        public void JumpPageAs(object? sender)
        {
            if (this.Book == null || this.Book.IsMedia) return;

            var dialogModel = new PageSelectDialogModel((this.Book.CurrentPage?.Index ?? 0) + 1, 1, this.Book.Pages.Count);

            var dialog = new PageSelectDialog(dialogModel);
            dialog.Owner = MainViewComponent.Current.GetWindow();
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var result = dialog.ShowDialog();

            if (result == true)
            {
                var page = this.Book.Pages.GetPage(dialogModel.Value - 1);
                if (page is null) return;
                _control.MoveTo(sender, page.Index);
            }
        }

        // 指定ページに移動
        public void JumpPage(object? sender, Page? page)
        {
            if (page is null) return;
            _control.MoveTo(sender, page.Index);
        }

        // ページの存在確認
        public void ValidatePages(IEnumerable<Page> pages)
        {
            var targetPages = pages.Where(e => e.PageType != PageType.Empty && !e.ArchiveEntry.Exists()).ToList();

            // 非同期での確認
            AppDispatcher.BeginInvoke(async () =>
            {
                try
                {
                    await _control.RemovePagesAsync(targetPages);
                }
                catch
                {
                    // 処理できなくても大きな問題はないので例外を無視する
                }
            });
        }

        #endregion

        #region BookCommand : メディア操作

        /// <summary>
        /// ページを指定して有効な MediaPlayer を取得
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ViewContentMediaPlayer? GetMediaPlayer(Page page)
        {
            return _presenter.ViewContents.FirstOrDefault(e => e.Page == page)?.Player;
        }

        /// <summary>
        /// 動画有効？
        /// </summary>
        /// <returns></returns>
        public bool MediaExists()
        {
            return MediaPlayerOperator.CurrentMediaOperator is not null;
        }

        /// <summary>
        /// 動画再生中？
        /// </summary>
        /// <returns></returns>
        public bool IsMediaPlaying()
        {
            return MediaPlayerOperator.CurrentMediaOperator?.IsPlaying ?? false;
        }

        /// <summary>
        /// 動画再生ON/OFF
        /// </summary>
        public void ToggleMediaPlay()
        {
            MediaPlayerOperator.CurrentMediaOperator?.TogglePlay();
        }

        #endregion

        #region BookCommand : 下位ブックに移動

        public bool CanMoveToChildBook()
        {
            var page = Book?.CurrentPage;
            return page != null && page.PageType.IsFolder();
        }

        public void MoveToChildBook(object sender)
        {
            var page = Book?.CurrentPage;
            if (page != null && page.PageType.IsFolder())
            {
                _bookHub.RequestLoad(sender, page.ArchiveEntry.SystemPath, null, BookLoadOption.IsBook | BookLoadOption.SkipSamePlace, true);
            }
        }

        #endregion

        #region ページ読み込み完了待機

        /// <summary>
        /// 表示ページ読み込み完了まで待機
        /// </summary>
        public async ValueTask WaitAsync(CancellationToken token)
        {
            await _presenter.WaitForViewPageStableAsync(token);
        }

        #endregion
    }
}
