using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Diagnostics;
using NeeView.IO;
using NeeLaboratory.Threading.Tasks;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NeeLaboratory.Threading.Jobs;
using NeeView.Interop;
using NeeLaboratory.Generators;
using System.Globalization;

// TODO: コマンド類の何時でも受付。ロード中だから弾く、ではない別の方法を。

namespace NeeView
{
    /// <summary>
    /// 本の管理
    /// ロード、本の操作はここを通す
    /// </summary>
    [NotifyPropertyChanged]
    public partial class BookHub : IDisposable, INotifyPropertyChanged
    {
        // Singleton
        static BookHub() => Current = new BookHub();
        public static BookHub Current { get; }

        private Toast? _bookHubToast;
        private bool _isLoading;
        private bool _isBookLocked;
        private Book? _book;
        private string? _address;
        private readonly BookHubCommandEngine _commandEngine;
        private int _requestLoadCount;
        private readonly DisposableCollection _disposables = new();


        private BookHub()
        {
            _disposables.Add(SubscribeBookChanged(
                (s, e) =>
                {
                    if (_disposedValue) return;

                    var book = _book;
                    if (book?.NotFoundStartPage != null && book.Pages.Count > 0)
                    {
                        InfoMessage.Current.SetMessage(InfoMessageType.BookName, string.Format(CultureInfo.InvariantCulture, Properties.TextResources.GetString("Notice.CannotOpen"), LoosePath.GetFileName(book.NotFoundStartPage)), null, 2.0);
                    }
                    else
                    {
                        InfoMessage.Current.SetMessage(InfoMessageType.BookName, LoosePath.GetFileName(Address), null, 2.0, e.BookMementoType);
                    }
                }));

            _disposables.Add(BookmarkCollection.Current.SubscribeBookmarkChanged(
                (s, e) => BookmarkChanged?.Invoke(s, e)));

            // command engine
            _commandEngine = new BookHubCommandEngine();
            _disposables.Add(_commandEngine.SubscribeIsBusyChanged(
                (s, e) => IsBusyChanged?.Invoke(s, e)));
            _commandEngine.StartEngine();
            _disposables.Add(_commandEngine);

            // アプリ終了前の開放予約
            ApplicationDisposer.Current.Add(this);
        }


        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;

        // 本の変更開始通知
        [Subscribable]
        public event EventHandler<BookChangingEventArgs>? BookChanging;

        // 本の変更通知
        [Subscribable]
        public event EventHandler<BookChangedEventArgs>? BookChanged;

        // ロードリクエスト開始
        [Subscribable]
        public event EventHandler<BookPathEventArgs>? LoadRequesting;

        // ロードリクエスト処理完了
        [Subscribable]
        public event EventHandler<BookPathEventArgs>? LoadRequested;

        // コマンドエンジン処理中通知
        [Subscribable]
        public event EventHandler<JobIsBusyChangedEventArgs>? IsBusyChanged;

        // フォルダー列更新要求
        [Subscribable]
        public event EventHandler<FolderListSyncEventArgs>? FolderListSync;

        // 履歴リスト更新要求
        [Subscribable]
        public event EventHandler<BookPathEventArgs>? HistoryListSync;

        // ブックマークにに追加、削除された
        [Subscribable]
        public event EventHandler<BookmarkCollectionChangedEventArgs>? BookmarkChanged;

        // アドレスが変更された
        [Subscribable]
        public event EventHandler? AddressChanged;


        // アドレス
        public string? Address
        {
            get { return _address; }
            private set
            {
                if (_disposedValue) return;

                if (_address != value)
                {
                    _address = value;
                    Config.Current.StartUp.LastBookPath = Address;
                    AddressChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// ロード可能フラグ
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// ロード中フラグ
        /// </summary>
        public bool IsLoading
        {
            get { return _isLoading; }
            private set
            {
                if (_disposedValue) return;

                if (SetProperty(ref _isLoading, value))
                {
                    BookSettings.Current.CanEdit = !_isLoading;
                }
            }
        }

        /// <summary>
        /// ロード中ブックのパス
        /// </summary>
        public string? LoadingPath { get; private set; }

        public bool IsBusy => _commandEngine.IsBusy;

        /// <summary>
        /// ロードリクエストカウント.
        /// 名前変更時の再読込判定に使用される
        /// </summary>
        public int RequestLoadCount => _requestLoadCount;

        /// <summary>
        /// ブックの切り替えを禁止する
        /// </summary>
        public bool IsBookLocked
        {
            get { return _isBookLocked; }
            set { SetProperty(ref _isBookLocked, value); }
        }


        /// <summary>
        /// 現在のブック
        /// </summary>
        public Book? GetCurrentBook() => _book;

        #region Callback Methods


        private void BookSource_DirtyBook(object? sender, EventArgs e)
        {
            RequestLoad(this, this.Address, null, BookLoadOption.ReLoad | BookLoadOption.IsBook, false);
        }

        #endregion Callback Methods

        #region Requests

        /// <summary>
        /// リクエスト：フォルダーを開く
        /// </summary>
        /// <param name="path">開く場所</param>
        /// <param name="start">ページの指定</param>
        /// <param name="option"></param>
        /// <param name="isRefreshFolderList">フォルダーリストを同期する？</param>
        /// <returns></returns>
        public BookHubCommandLoad? RequestLoad(object? sender, string? path, string? start, BookLoadOption option, bool isRefreshFolderList)
        {
            return RequestLoad(sender, path, start, option, isRefreshFolderList, ArchiveHint.None); 
        }

        public BookHubCommandLoad? RequestLoad(object? sender, string? path, string? start, BookLoadOption option, bool isRefreshFolderList, ArchiveHint archiveHint)
        {
            if (path == null) return null;

            if (_disposedValue) return null;

            if (!this.IsEnabled) return null;

            var query = new QueryPath(path).Normalize();
            var sourcePath = query.SimpleQuery;

            query = query.ResolvePath();

            // Legacy:
            if (query.SimplePath.StartsWith("pagemark:", StringComparison.Ordinal) == true)
            {
                query = new QueryPath(Config.Current.Playlist.PagemarkPlaylist);
            }

            if (_address == query.SimplePath && option.HasFlag(BookLoadOption.KeepArchiveHint) && archiveHint == ArchiveHint.None)
            {
                archiveHint = _book?.ArchiveHint ?? archiveHint;
            }

            ////DebugTimer.Start($"\nStart: {path}");
            if (_address == query.SimplePath && option.HasFlag(BookLoadOption.SkipSamePlace) && _book?.ArchiveHint == archiveHint)
            {
                return null;
            }

            // ブック固定ならば同じアドレスでのみ有効
            if (this.IsBookLocked && _address != query.SimplePath)
            {
                return null;
            }

            this.Address = query.SimplePath;

            Interlocked.Increment(ref _requestLoadCount);

            var command = new BookHubCommandLoad(this, new BookHubCommandLoadArgs(query.SimpleQuery, sourcePath)
            {
                Sender = sender,
                //Path = path,
                //SourcePath = sourcePath,
                StartEntry = start,
                Option = option,
                IsRefreshFolderList = isRefreshFolderList,
                ArchiveHint = archiveHint
            });

            command.Completed += JobCommand_Completed;
            LoadRequesting?.Invoke(this, new BookPathEventArgs(query.SimplePath));
            _commandEngine.Enqueue(command);
            return command;

            void JobCommand_Completed(object? sender, JobCompletedEventArgs e)
            {
                command.Completed -= JobCommand_Completed;
                LoadRequested?.Invoke(this, new BookPathEventArgs(query.SimplePath));
            }
        }


        // アンロード可能?
        public bool CanUnload()
        {
            if (_disposedValue) return false;

            return _book != null || _commandEngine.IsBusy;
        }

        /// <summary>
        /// リクエスト：フォルダーを閉じる
        /// </summary>
        /// <param name="isClearViewContent"></param>
        /// <returns></returns>
        public BookHubCommandUnload RequestUnload(object? sender, bool isClearViewContent, string? message = null)
        {
            ThrowIfDisposed();

            IsBookLocked = false;

            var command = new BookHubCommandUnload(this, new BookHubCommandUnloadArgs()
            {
                Sender = sender,
                IsClearViewContent = isClearViewContent,
                Message = message
            });

            _commandEngine.Enqueue(command);

            return command;
        }


        // 再読込可能？
        public bool CanReload()
        {
            if (_disposedValue) return false;

            return (!string.IsNullOrWhiteSpace(Address));
        }

        /// <summary>
        /// リクエスト：再読込
        /// </summary>
        public void RequestReLoad(object sender)
        {
            RequestReLoad(sender, null, null);
        }

        public void RequestReLoad(object sender, string? start)
        {
            RequestReLoad(sender, start, null);
        }

        public void RequestReLoad(object sender, string? start, ArchiveHint? hint)
        {
            if (_disposedValue) return;

            if (_isLoading || Address == null) return;

            var book = _book;
            BookLoadOption options = book != null ? (book.LoadOption & BookLoadOption.KeepHistoryOrder) | BookLoadOption.Resume : BookLoadOption.None;
            ArchiveHint archiveHint = hint ?? (book != null ? book.ArchiveHint : ArchiveHint.None);

            var query = new QueryPath(Address, book?.Pages.SearchKeyword);
            RequestLoad(sender, query.SimpleQuery, start, options | BookLoadOption.IsBook | BookLoadOption.IgnoreCache, false, archiveHint);
        }

        // 上の階層に移動可能？
        public bool CanLoadParent()
        {
            if (_disposedValue) return false;

            var parent = _book?.BookAddress?.Place;
            return parent?.Path != null && parent.Scheme == QueryScheme.File;
        }

        /// <summary>
        /// リクエスト：上の階層に移動
        /// </summary>
        public void RequestLoadParent(object sender)
        {
            if (_disposedValue) return;

            var bookAddress = _book?.BookAddress;

            var current = bookAddress?.TargetPath;
            if (current is null) return;

            var parent = bookAddress?.Place;
            if (parent is null) return;

            if (parent.Path != null && parent.Scheme == QueryScheme.File)
            {
                var entryName = current.SimplePath[parent.SimplePath.Length..].TrimStart(LoosePath.Separators);
                var option = BookLoadOption.SkipSamePlace;
                RequestLoad(sender, parent.SimplePath, entryName, option, true);
            }
        }

        #endregion Requests

        #region BookHubCommand.Load

        /// <summary>
        /// ロード中状態更新
        /// </summary>
        /// <param name="path"></param>
        private void NotifyLoading(string? path)
        {
            if (_disposedValue) return;

            this.LoadingPath = path;
            this.IsLoading = (path != null);
        }

        /// <summary>
        /// 本を読み込む
        /// </summary>
        public async ValueTask LoadAsync(BookHubCommandLoadArgs args, CancellationToken token)
        {
            if (_disposedValue) return;

            token.ThrowIfCancellationRequested();

            ////DebugTimer.Check("LoadAsync...");

            // 現在の設定を記憶
            var oldBook = _book;
            var lastBookMemento = oldBook?.Path != null ? oldBook.CreateMemento() : null;

            this.Address = args.SourcePath;

            string place = args.Path;

            var bookChangedEventArgs = new BookChangedEventArgs(Address, null, BookMementoType.None);
            var isEmptyBook = false;

            try
            {
                // Now Loading ON
                NotifyLoading(args.Path);

                // 本の変更開始通知
                // NOTE: パスはまだページの可能性があるので不完全
                BookChanging?.Invoke(this, new BookChangingEventArgs(Address));

                // 現在の本を開放
                UnloadCore();

                if (_bookHubToast != null)
                {
                    _bookHubToast.Cancel();
                    _bookHubToast = null;
                }

                // address
                var address = await BookAddress.CreateAsync(new QueryPath(args.Path), new QueryPath(args.SourcePath), args.StartEntry, Config.Current.System.ArchiveRecursiveMode, args.Option, token);

                // 履歴リスト更新
                if ((args.Option & BookLoadOption.SelectHistoryMaybe) != 0)
                {
                    HistoryListSync?.Invoke(this, new BookPathEventArgs(address.TargetPath.SimplePath));
                }

                // 本の設定
                var setting = BookMementoTools.CreateOpenBookMemento(address.TargetPath.SimplePath, lastBookMemento, args.Option);

                // 最終ページなら初期化？
                bool isResetLastPage = address.EntryName == null && Config.Current.BookSettingPolicy.Page == BookSettingPageSelectMode.RestoreOrDefaultReset;

                address.EntryName = address.EntryName ?? LoosePath.NormalizeSeparator(setting.Page);
                place = address.SystemPath;

                // 移動履歴登録
                BookHubHistory.Current.Add(args.Sender, address.TargetPath);

                // フォルダーリスト更新
                if (args.IsRefreshFolderList)
                {
                    FolderListSync?.Invoke(this, new FolderListSyncEventArgs(address.TargetPath.SimplePath, address.Place.SimplePath, false));
                }

                var isNew = BookMementoCollection.Current.GetValid(address.TargetPath.SimplePath) == null;

                // Load本体
                var loadOption = args.Option | (isResetLastPage ? BookLoadOption.ResetLastPage : BookLoadOption.None);
                var book = await LoadAsyncCore(args.Sender, address, loadOption, setting, isNew, args.ArchiveHint, token);

                //_historyEntry = false;
                //_historyRemoved = false;

                // 現在の設定を更新
                book.Setting.CopyTo(Config.Current.BookSetting);
                book.AttachBookSetting(Config.Current.BookSetting);

                this.Address = book.Path;
                _book = book;
                SubscribeBook(book);

                ////DebugTimer.Check("LoadCore");
                bookChangedEventArgs = new BookChangedEventArgs(book.Path, book, BookMementoTools.GetBookMementoType(book));

                // ページ存在チェック
                isEmptyBook = (book != null && book.Pages.Count <= 0);
                if (isEmptyBook)
                {
                    bookChangedEventArgs.EmptyMessage = string.Format(CultureInfo.InvariantCulture, Properties.TextResources.GetString("Notice.NoPages"), book?.Path);
                }

                // フォーカス更新
                if (args.Option.HasFlag(BookLoadOption.FocusOnLoaded))
                {
                    await AppDispatcher.BeginInvoke(() => FocusMainViewOnBookLoaded(args.Sender));
                }
            }
            catch (OperationCanceledException)
            {
                // nop.
            }
            catch (Exception ex)
            {
                if (ex is BookAddressException)
                {
                    bookChangedEventArgs.EmptyMessage = ex.Message;
                }
                else
                {
                    // ファイル読み込み失敗通知
                    var message = string.Format(CultureInfo.InvariantCulture, Properties.TextResources.GetString("LoadFailedException.Message"), place, ex.Message);
                    bookChangedEventArgs.EmptyMessage = message;
                }

                // 現在表示されているコンテンツを無効
                //ViewContentsChanged?.Invoke(args.Sender, new ViewContentSourceCollectionChangedEventArgs());

                // 履歴リスト更新
                if ((args.Option & BookLoadOption.SelectHistoryMaybe) != 0)
                {
                    HistoryListSync?.Invoke(this, new BookPathEventArgs(Address));
                }
            }
            finally
            {
                // 本の変更通知
                NotifyLoading(null);
                BookChanged?.Invoke(this, bookChangedEventArgs);
                ////DebugTimer.Check("Done.");
            }

            // ページがなかった時の処理
            if (_book is not null && isEmptyBook)
            {
                BookMementoTools.ResetBookMementoPage(_book.Path);

                if (Config.Current.Book.IsConfirmRecursive && (args.Option & BookLoadOption.ReLoad) == 0 && !_book.Source.IsRecursiveFolder && _book.Source.SubFolderCount > 0)
                {
                    AppDispatcher.Invoke(() => ConfirmRecursive(args.Sender, _book, token));
                }
            }
        }

        /// <summary>
        /// ブック読み込み後のフォーカス調整
        /// </summary>
        /// <param name="targetWindow"></param>
        private static void FocusMainViewOnBookLoaded(object? sender)
        {
            // TODO: BookHub で UI操作を行っていることに違和感
            var layoutPanelManager = CustomLayoutPanelManager.Current;
            var mainView = MainViewManager.Current.MainView;
            var window = sender is System.Windows.DependencyObject element ? System.Windows.Window.GetWindow(element) : null;

            // 本棚へのフォーカスを試す
            if (layoutPanelManager.IsPanelVisible(nameof(FolderPanel)) && !layoutPanelManager.IsPanelFloating(nameof(FolderPanel)))
            {
                var panel = layoutPanelManager.GetPanel(nameof(FolderPanel));
                if (window is null || window == System.Windows.Window.GetWindow(panel.View.Value))
                {
                    layoutPanelManager.Focus(nameof(FolderPanel));
                    return;
                }
            }

            // メインビューへのフォーカスを試す
            if (window is null || window == System.Windows.Window.GetWindow(mainView))
            {
                mainView.FocusMainView();
            }
        }

        // 再帰読み込み確認
        // TODO: UI操作を行うのはいかがなものか。イベントかMessengerにする必要あり。
        private void ConfirmRecursive(object? sender, Book book, CancellationToken token)
        {
            if (book is null) throw new ArgumentNullException(nameof(book));

            token.ThrowIfCancellationRequested();

            var dialog = new MessageDialog(string.Format(CultureInfo.InvariantCulture, Properties.TextResources.GetString("ConfirmRecursiveDialog.Message"), book.Path), Properties.TextResources.GetString("ConfirmRecursiveDialog.Title"));
            dialog.Commands.Add(UICommands.Yes);
            dialog.Commands.Add(UICommands.No);

            using (var register = token.Register(() => dialog.Close()))
            {
                var result = dialog.ShowDialog();
                token.ThrowIfCancellationRequested();

                if (result.Command == UICommands.Yes)
                {
                    RequestReloadRecursive(sender, book);
                }
            }
        }

        private void RequestReloadRecursive(object? sender, Book book)
        {
            // TODO: BookAddressそのまま渡せばよいと思う
            RequestLoad(sender, book.Path, book.StartEntry, BookLoadOption.Recursive | BookLoadOption.ReLoad, true);
        }

        /// <summary>
        /// ブックを読み込む(本体)
        /// </summary>
        private static async ValueTask<Book> LoadAsyncCore(object? sender, BookAddress address, BookLoadOption option, BookMemento setting, bool isNew, ArchiveHint archiveHint, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var memento = ((option & BookLoadOption.ReLoad) == BookLoadOption.ReLoad) ? Config.Current.BookSetting.ToBookMemento() : setting;

            var bookSetting = new BookCreateSetting()
            {
                StartPage = BookLoadOptionHelper.CreateBookStartPage(address.EntryName, option),
                IsRecursiveFolder = BookLoadOptionHelper.CreateIsRecursiveFolder(memento.IsRecursiveFolder, option),
                ArchiveRecursiveMode = Config.Current.System.ArchiveRecursiveMode,
                BookPageCollectMode = Config.Current.System.BookPageCollectMode,
                SortMode = memento.SortMode,
                IsIgnoreCache = option.HasFlag(BookLoadOption.IgnoreCache),
                IsNew = isNew,
                LoadOption = option,
                ArchiveHint = archiveHint,
            };

            var book = await BookFactory.CreateAsync(sender, address, bookSetting, memento, token);

            // auto recursive
            if (Config.Current.Book.IsAutoRecursive && !book.Source.IsRecursiveFolder && book.Source.SubFolderCount == 1)
            {
                bookSetting.IsRecursiveFolder = true;
                book = await BookFactory.CreateAsync(sender, address, bookSetting, memento, token);
            }

            return book;
        }


        /// <summary>
        /// ブックイベント購読
        /// </summary>
        private void SubscribeBook(Book book)
        {
            if (book is null) return;

            //book.Viewer.Loader.ViewContentsChanged += BookViewer_ViewContentsChanged;
            //book.Viewer.Loader.NextContentsChanged += BookViewer_NextContentsChanged;
            book.Source.DirtyBook += BookSource_DirtyBook;
        }

        /// <summary>
        /// ブックイベント購読解除
        /// </summary>
        private void UnsubscribeBook(Book book)
        {
            if (book is null) return;

            //book.Viewer.Loader.ViewContentsChanged -= BookViewer_ViewContentsChanged;
            //book.Viewer.Loader.NextContentsChanged -= BookViewer_NextContentsChanged;
            book.Source.DirtyBook -= BookSource_DirtyBook;
        }

        #endregion BookHubCommand.Load

        #region BookHubCommand.Unload

        /// <summary>
        /// 本の開放
        /// </summary>
        public void Unload(BookHubCommandUnloadArgs parameter)
        {
            if (_disposedValue) return;

            var bookChangedEventArgs = new BookChangedEventArgs("", null, BookMementoType.None);

            try
            {
                // 本の変更通
                BookChanging?.Invoke(parameter.Sender, new BookChangingEventArgs(""));

                UnloadCore();

                if (parameter.IsClearViewContent)
                {
                    this.Address = null;

                    // 現在表示されているコンテンツを無効
                    //ViewContentsChanged?.Invoke(parameter.Sender, new ViewContentSourceCollectionChangedEventArgs());
                }

                if (parameter.Message != null)
                {
                    bookChangedEventArgs.EmptyMessage = parameter.Message;
                }
            }
            finally
            {
                // 本の変更通
                NotifyLoading(null);
                BookChanged?.Invoke(parameter.Sender, bookChangedEventArgs);
            }
        }

        /// <summary>
        /// 本の開放
        /// </summary>
        private void UnloadCore()
        {
            // 再生中のメディアをPAUSE
            // TODO: ここでUI操作を行うのはいかがなものか。イベントかMessengerにする必要あり。
            AppDispatcher.Invoke(() => MediaPlayerOperator.BookMediaOperator?.Pause());

            var book = _book;
            _book = null;

            if (book is not null)
            {
                // 現在の本を開放
                UnsubscribeBook(book);
                book.Dispose();
            }
        }

        #endregion BookHubCommand.Unload


        #region IDisposable Support
        private bool _disposedValue = false;

        private void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }

        void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();

                    // reset event
                    // 非同期で処理されるため、イベントを確実に停止させるためにクリアしている
                    this.BookChanging = null;
                    this.BookChanged = null;
                    this.LoadRequesting = null;
                    this.LoadRequested = null;
                    this.FolderListSync = null;
                    this.HistoryListSync = null;
                    this.BookmarkChanged = null;
                    this.AddressChanged = null;
                    this.PropertyChanged = null;

                    //SaveBookMemento();

                    _book?.Dispose();

                    Debug.WriteLine("BookHub Disposed.");
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        /// <summary>
        /// 処理完了を待機
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async ValueTask WaitAsync(CancellationToken token)
        {
            if (!_commandEngine.IsBusy) return;

            // BookHubのコマンド処理が終わるまで待機
            using var eventFlag = new ManualResetEventSlim();
            using var isBusyEvent = _commandEngine.SubscribeIsBusyChanged((s, e) => eventFlag.Set());
            while (_commandEngine.IsBusy)
            {
                await eventFlag.WaitHandle.AsTask().WaitAsync(token);
            }
        }


    }


    
}

