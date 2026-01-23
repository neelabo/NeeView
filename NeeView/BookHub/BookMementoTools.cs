using System;
using System.Diagnostics;


namespace NeeView
{
    /// <summary>
    /// BookMemento に関する処理
    /// </summary>
    public static class BookMementoTools
    {
        /// <summary>
        /// BookMementoの所属を取得
        /// </summary>
        public static BookMementoType GetBookMementoType(Book book)
        {
            if (book is null) return BookMementoType.None;

            if (BookmarkCollection.Current.Contains(book.Path))
            {
                return BookMementoType.Bookmark;
            }
            else if (BookHistoryCollection.Current.Contains(book.Path))
            {
                return BookMementoType.History;
            }
            else
            {
                return BookMementoType.None;
            }
        }

        /// <summary>
        /// 現在開いているブックの設定作成
        /// </summary>
        public static BookMemento? CreateBookMemento(Book book)
        {
            return (book != null && book.Pages.Count > 0) ? book.CreateMemento() : null;
        }

        // ブック設定の作成
        // 開いているブックならばその設定を取得する
        public static BookMemento CreateBookMemento(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            //var book = _book;
            var book = BookHub.Current.GetCurrentBook();
            var memento = book is not null ? CreateBookMemento(book) : null;
            if (memento == null || memento.Path != path)
            {
                memento = Config.Current.BookSettingDefault.ToBookMemento();
                memento.Path = path;
            }
            return memento;
        }

        /// <summary>
        /// 最新の本の設定を取得
        /// </summary>
        /// <param name="address">場所</param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static BookMemento GetLatestBookMemento(string address, BookLoadOption option)
        {
            if (address is null) throw new ArgumentNullException(nameof(address));

            var book = BookHub.Current.GetCurrentBook();
            if (book is not null && book.Path == address)
            {
                return book.CreateMemento();
            }

            return CreateOpenBookMemento(address, null, option);
        }

        /// <summary>
        /// 適切な設定を作成
        /// </summary>
        /// <param name="path">場所</param>
        /// <param name="latest">現在の情報</param>
        /// <param name="option">読み込みオプション</param>
        public static BookMemento CreateOpenBookMemento(string path, BookMemento? latest, BookLoadOption option)
        {
            var memory = CreateLatestBookMemento(path, latest);
            Debug.Assert(memory == null || memory.Path == path);

            BookMemento memento;
            if (memory != null && option.HasFlag(BookLoadOption.Resume))
            {
                memento = memory.Clone();
            }
            else
            {
                var restore = memory?.ToBookSetting();
                memento = Config.Current.BookSettingPolicy.Mix(Config.Current.BookSettingDefault, Config.Current.BookSetting, restore, option.HasFlag(BookLoadOption.DefaultRecursive)).ToBookMemento();
                memento.SortSeed = memory?.SortSeed ?? 0;
            }

            if (option.HasFlag(BookLoadOption.ReLoad) && memento.SortMode == PageSortMode.Random)
            {
                memento.Page = "";
                memento.SortSeed = 0;
            }

            return memento;
        }

        /// <summary>
        /// 最新の設定を取得
        /// </summary>
        /// <param name="path">場所</param>
        /// <param name="latest">現在の情報</param>
        private static BookMemento? CreateLatestBookMemento(string path, BookMemento? latest)
        {
            BookMemento? memento = null;

            if (latest?.Path == path)
            {
                memento = latest.Clone();
            }
            else
            {
                var unit = BookMementoCollection.Current.GetValid(path);
                if (unit != null)
                {
                    memento = unit.Memento.Clone();
                }
            }

            return memento;
        }


        /// <summary>
        /// 記録のページのみクリア
        /// </summary>
        public static void ResetBookMementoPage(string place)
        {
            if (place is null) throw new ArgumentNullException(nameof(place));

            var unit = BookMementoCollection.Current.GetValid(place);
            if (unit?.Memento != null)
            {
                unit.Memento.Page = "";
            }
        }


        /// <summary>
        /// 名前変更追従
        /// </summary>
        /// <remarks>
        /// TODO: BookMemento外の設定も変更するので、別の場所に定義したい
        /// </remarks>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public static void RenameRecursive(string src, string dst)
        {
            if (src == dst) return;

            FileResolver.Current.AddIfTargetExists(dst, src);

            BookMementoCollection.Current.RenameRecursive(src, dst);
            QuickAccessCollection.Current.RenameRecursive(src, dst);
            FolderConfigTools.RenameRecursive(src, dst);
            PlaylistHub.Current.RenameItemPathRecursive(src, dst);
        }
    }
}

