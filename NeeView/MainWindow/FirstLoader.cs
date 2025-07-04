using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// 最初のブック、フォルダーのロード
    /// </summary>
    public class FirstLoader
    {
        private BookProfile? _lastBook;
        private FolderProfile? _lastFolder;
        private BookProfile? _book;
        private FolderProfile? _folder;
        private BookLoadOption _bookLoadOptions;

        public void Load()
        {
            _book = null;
            _folder = FolderProfile.Create(App.Current.Option.FolderListQuery?.SimpleQuery);

            _lastBook = GetLastBookProfile();
            _lastFolder = GetLastFolderProfile();

            SetBookPlace();
            SetFolderPlace();
            LoadBook();
            LoadFolder();
        }

        private static BookProfile? GetLastBookProfile()
        {
#pragma warning disable CS0612 // 型またはメンバーが旧型式です
            var path = Config.Current.StartUp.LastBook?.Path ?? Config.Current.StartUp.LastBookPath;
            Config.Current.StartUp.LastBookPath = null;
#pragma warning restore CS0612 // 型またはメンバーが旧型式です

            if (!Config.Current.StartUp.IsOpenLastBook || string.IsNullOrEmpty(path) || path.StartsWith(Temporary.Current.TempRootPath, StringComparison.Ordinal) == true)
            {
                return null;
            }

            return new BookProfile(path, Config.Current.StartUp.LastBook);
        }

        private static FolderProfile? GetLastFolderProfile()
        {
#pragma warning disable CS0612 // 型またはメンバーが旧型式です
            var path = Config.Current.StartUp.LastFolder?.Path ?? Config.Current.StartUp.LastFolderPath;
            Config.Current.StartUp.LastFolderPath = null;
#pragma warning restore CS0612 // 型またはメンバーが旧型式です

            if (!Config.Current.StartUp.IsOpenLastFolder || string.IsNullOrEmpty(path) || path.StartsWith(Temporary.Current.TempRootPath, StringComparison.Ordinal) == true)
            {
                return null;
            }

            return new FolderProfile(path, Config.Current.StartUp.LastFolder);
        }


        private void SetBookPlace()
        {
            if (App.Current.Option.IsBlank == SwitchOption.on)
            {
                _book = null;
                return;
            }

            // 起動引数の場所で開く
            if (App.Current.Option.Values.Count >= 1)
            {
                _book = BookProfile.Create(App.Current.Option.Values);

                // 起動引数でブックを指定した場合は本棚を復元しない
                _lastFolder = null;
                return;
            }

            // 最後に開いたブックを復元する
            if (_lastBook != null)
            {
                _book = _lastBook;
                _bookLoadOptions = BookLoadOption.Resume | BookLoadOption.IsBook;
                return;
            }
        }

        private void SetFolderPlace()
        {
            if (_folder != null)
            {
                return;
            }

            // 前回開いていたフォルダーを復元する
            if (_lastFolder != null)
            {
                _folder = _lastFolder;
                return;
            }

            // Bookが指定されていなければ既定の場所を開く
            if (_book == null)
            {
                _folder = FolderProfile.Create(BookshelfFolderList.Current.GetFixedHome().SimpleQuery);
                return;
            }
        }

        private void LoadBook()
        {
            if (_book is null) return;

            var options = _bookLoadOptions | BookLoadOption.FocusOnLoaded;
            BookHubTools.RequestLoad(this, _book.Paths, options, _folder == null, _book.BookMemento);
        }

        private void LoadFolder()
        {
            if (_folder is null) return;

            var path = _book?.Path ?? _folder.FolderMemento?.Select;
            var select = path is not null ? new FolderItemPosition(new QueryPath(path)) : null;

            _folder.FolderMemento?.Register();
            BookshelfFolderList.Current.RequestPlace(new QueryPath(_folder.Path), select, FolderSetPlaceOption.UpdateHistory);
        }


        /// <summary>
        /// 最初のブックの情報
        /// </summary>
        internal class BookProfile
        {
            public BookProfile(string path, BookMemento? memento)
            {
                Paths = [path];
                BookMemento = memento;
            }

            public BookProfile(IEnumerable<string> paths)
            {
                Paths = [.. paths];
            }

            public string Path => Paths.First();
            public List<string> Paths { get; set; }
            public BookMemento? BookMemento { get; set; }

            public static BookProfile? Create(string? path, BookMemento? memento = null)
            {
                if (string.IsNullOrEmpty(path)) return null;

                return new BookProfile(path, memento);
            }

            public static BookProfile? Create(IEnumerable<string>? paths)
            {
                var fixPaths = paths?.Where(e => !string.IsNullOrEmpty(e));
                if (fixPaths is null || !fixPaths.Any()) return null;

                return new BookProfile(fixPaths);
            }
        }


        /// <summary>
        /// 最初のフォルダーの情報
        /// </summary>
        internal class FolderProfile
        {
            public FolderProfile(string path, BookshelfFolderMemento? memento)
            {
                Path = path;
                FolderMemento = memento;
            }

            public string Path { get; set; }
            public BookshelfFolderMemento? FolderMemento { get; set; }

            public static FolderProfile? Create(string? path, BookshelfFolderMemento? memento = null)
            {
                if (string.IsNullOrEmpty(path)) return null;

                return new FolderProfile(path, memento);
            }
        }

    }

}
