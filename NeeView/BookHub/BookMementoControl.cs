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
        private int _pageChangedCount;


        public BookMementoControl(Book book, BookHistoryCollection historyCollection)
        {
            _book = book;
            _historyCollection = historyCollection;

            _disposables.Add(_book.SubscribeCurrentPageChanged(Book_CurrentPageChanged));
            _disposables.Add(_historyCollection.SubscribeHistoryChanged(BookHistoryCollection_HistoryChanged));
        }


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
            if (e.HistoryChangedType == BookMementoCollectionChangedType.Remove && (book.Path == e.Key || e.Key == null))
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
                _pageChangedCount++;
                _historyRemoved = false;
            }

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

            bool allowUpdateHistory = !book.IsKeepHistoryOrder || Config.Current.History.IsForceUpdateHistory;

            // 履歴更新
            if (allowUpdateHistory && !_historyEntry && CanHistory(book))
            {
                _historyEntry = true;
                var memento = book.CreateMemento();
                if (memento is not null)
                {
                    BookHistoryCollection.Current.Add(memento, false);
                    LocalDebug.WriteLine("Try save BookMemento: Saved");
                }
            }
        }


        // 設定の保存
        public void SaveBookMemento()
        {
            var book = _book;
            if (book is null) return;

            SaveBookMemento(book);
        }

        //設定の保存
        private void SaveBookMemento(Book book)
        {
            if (book is null) return;

            var memento = BookMementoTools.CreateBookMemento(book);
            if (memento is null) return;

            bool isKeepHistoryOrder = book.IsKeepHistoryOrder || Config.Current.History.IsForceUpdateHistory;
            SaveBookMemento(book, memento, isKeepHistoryOrder);
        }

        private void SaveBookMemento(Book book, BookMemento memento, bool isKeepHistoryOrder)
        {
            if (memento == null) return;

            // ブックマークの更新
            BookmarkCollection.Current.Update(memento, _pageChangedCount > 1);

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

            // 履歴閲覧時の履歴更新は最低１操作を必要とする
            var historyEntryPageCount = Config.Current.History.HistoryEntryPageCount;
            if (book.IsKeepHistoryOrder && Config.Current.History.IsForceUpdateHistory && historyEntryPageCount <= 0)
            {
                historyEntryPageCount = 1;
            }

            return !_historyRemoved
                && book.Pages.Count > 0
                && (_historyEntry || _pageChangedCount >= historyEntryPageCount || book.CurrentPages.LastOrDefault() == book.Pages.Last())
                && (Config.Current.History.IsInnerArchiveHistoryEnabled || book.Source.ArchiveEntryCollection.Archive?.Parent == null)
                && (Config.Current.History.IsUncHistoryEnabled || !LoosePath.IsUnc(book.Path));
        }

    }
}

