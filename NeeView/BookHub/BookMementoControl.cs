//#define LOCAL_DEBUG

using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;


namespace NeeView
{
    /// <summary>
    /// ブックに対する履歴操作
    /// </summary>
    [LocalDebug]
    public partial class BookMementoControl : IDisposable
    {
        private readonly Book _book;
        private readonly BookHistoryCollection _historyCollection;
        private bool _historyEntry;
        private bool _historyRemoved;
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();
        private int _pageChangeCount;


        public BookMementoControl(Book book, BookHistoryCollection historyCollection)
        {
            _book = book;
            _historyCollection = historyCollection;

            _disposables.Add(_book.SubscribeCurrentPageChanged(Book_CurrentPageChanged));
            _disposables.Add(_historyCollection.SubscribeHistoryChanged(BookHistoryCollection_HistoryChanged));
        }


        /// <summary>
        /// 履歴保存条件にページ変更回数を使用するかどうか
        /// </summary>
        public bool IsPageChangeCountEnabled { get; set; } = true;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        private void BookHistoryCollection_HistoryChanged(object? sender, BookMementoCollectionChangedArgs e)
        {
            if (_disposedValue) return;

            var book = _book;
            if (book is null) return;

            // 履歴削除されたものを履歴登録しないようにする
            if (e.HistoryChangedType == BookMementoCollectionChangedType.Remove && e.OldItems.Any(e => e.Path == book.Path))
            {
                _historyRemoved = true;
            }
        }


        private void Book_CurrentPageChanged(object? sender, CurrentPageChangedEventArgs e)
        {
            if (_disposedValue) return;

            if (e.IsTopPageChanged)
            {
                LocalDebug.WriteLine("CurrentPageChanged");
                _pageChangeCount++;
                _historyRemoved = false;
            }

            TrySaveBookMemento();
        }

        /// <summary>
        /// 履歴保存要求
        /// </summary>
        public void RequestSaveBookMemento()
        {
            IsPageChangeCountEnabled = false;
            TrySaveBookMemento();
        }

        /// <summary>
        /// 必要であれば履歴保存する
        /// </summary>
        public void TrySaveBookMemento()
        {
            var book = _book;
            if (book is null) return;

            LocalDebug.WriteLine("Try save BookMemento...");

            // 履歴更新
            if (!_historyEntry && CanHistory(book))
            {
                _historyEntry = true;
                var memento = book.CreateMemento();
                if (memento is not null)
                {
                    bool isKeepHistoryOrder = book.IsKeepHistoryOrder && !Config.Current.History.IsForceUpdateHistory;
                    BookHistoryCollection.Current.Add(memento, isKeepHistoryOrder);
                    LocalDebug.WriteLine("Try save BookMemento: Saved");
                }
            }
        }

        /// <summary>
        /// 設定の作成
        /// </summary>
        /// <returns></returns>
        public BookMemento? CreateBookMement()
        {
            var book = _book;
            if (book is null)
            {
                return null;
            }

            return BookMementoTools.CreateBookMemento(book);
        }

        /// <summary>
        /// 設定の保存
        /// </summary>
        public void SaveBookMemento()
        {
            var book = _book;
            if (book is null)
            {
                return;
            }

            SaveBookMemento(book);
        }

        /// <summary>
        /// 設定の保存
        /// </summary>
        /// <param name="book"></param>
        private void SaveBookMemento(Book book)
        {
            if (book is null) return;

            var memento = BookMementoTools.CreateBookMemento(book);
            if (memento is null) return;

            bool isKeepHistoryOrder = book.IsKeepHistoryOrder && !Config.Current.History.IsForceUpdateHistory;
            SaveBookMemento(book, memento, isKeepHistoryOrder);
        }

        private void SaveBookMemento(Book book, BookMemento memento, bool isKeepHistoryOrder)
        {
            if (memento == null) return;

            // ブックマークの更新
            BookmarkCollection.Current.Update(memento, _pageChangeCount > 1);

            // 履歴の保存
            if (CanHistory(book))
            {
                LocalDebug.WriteLine("Save BookMemento.");
                BookHistoryCollection.Current.Add(memento, isKeepHistoryOrder);
            }
        }

        // 履歴登録可
        private bool CanHistory(Book book)
        {
            if (book is null) return false;

            // 履歴登録開始ページ操作回数
            var historyEntryPageCount = Config.Current.History.HistoryEntryPageCount;

            // 既に履歴登録されている場合は１操作で更新可能とする
            if (!book.IsNew)
            {
                historyEntryPageCount = 1;
            }

            return !_historyRemoved
                && book.Pages.Count > 0
                && (!IsPageChangeCountEnabled || _historyEntry || _pageChangeCount >= historyEntryPageCount || book.CurrentPages.LastOrDefault() == book.Pages.Last())
                && (Config.Current.History.IsInnerArchiveHistoryEnabled || book.Source.ArchiveEntryCollection.Archive?.Parent == null)
                && (Config.Current.History.IsUncHistoryEnabled || !LoosePath.IsUnc(book.Path));
        }

    }
}

