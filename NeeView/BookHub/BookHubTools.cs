using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeeView
{
    public static class BookHubTools
    {
        /// <summary>
        /// ブックを開く。複数対応版
        /// </summary>
        /// <remarks>
        /// 複数ファイルの場合はプレイリスト化して本棚の場所として開く
        /// </remarks>
        public static string? RequestLoad(object sender, IEnumerable<string> paths)
        {
            return RequestLoad(sender, paths, BookLoadOption.None, true, null);
        }

        /// <summary>
        /// ブックを開く。複数対応版
        /// </summary>
        /// <remarks>
        /// 複数ファイルの場合はプレイリスト化して本棚の場所として開く
        /// </remarks>
        public static string? RequestLoad(object sender, IEnumerable<string> paths, BookLoadOption options, bool isRefreshFolderList, BookMemento? bookMemento)
        {
            if (paths is null) return null;

            var firstFile = paths.FirstOrDefault();
            if (firstFile is null) return null;

            // Import
            if (LoosePath.GetExtension(firstFile) == ".nvzip")
            {
                var parameter = new ImportBackupCommandParameter() { FileName = firstFile };
                ExportDataPresenter.Current.Import(parameter);
                return null;
            }

            if (paths.Count() >= 2)
            {
                var path = PlaylistSourceTools.CreateTempPlaylist(paths);
                BookHub.Current.RequestLoad(sender, path, null, options, false);
                if (isRefreshFolderList)
                {
                    BookshelfFolderList.Current.RequestPlace(new QueryPath(path), null, FolderSetPlaceOption.UpdateHistory);
                }
                return path;
            }
            else
            {
                var path = paths.First();
                BookHub.Current.RequestLoad(sender, path, null, options, isRefreshFolderList, ArchiveHint.None, bookMemento);
                return path;
            }
        }

        /// <summary>
        /// このブックはサブフォルダーを読み込む設定？
        /// </summary>
        public static bool IsRecursiveBook(QueryPath query)
        {
            if (query is null) return false;

            // 開いているブックは現状の設定を返す ... これいらない？下の計算でやってる？
            var book = BookHub.Current.GetCurrentBook();
            if (book != null && book.Path == query.SimplePath)
            {
                return book.Source.IsRecursiveFolder;
            }

            // 開いていないブックは履歴と設定から計算する
            var lastBookMemento = book?.Path != null ? book.CreateMemento() : null;
            var loadOption = BookLoadOption.Resume | (IsFolderRecursive(query.GetParent()) ? BookLoadOption.DefaultRecursive : BookLoadOption.None);
            var setting = BookMementoTools.CreateOpenBookMemento(query.SimplePath, lastBookMemento, loadOption);
            return setting.IsRecursiveFolder;
        }

        /// <summary>
        /// この場所のブックは既定でサブフォルダーを読み込む設定？
        /// </summary>
        public static bool IsFolderRecursive(QueryPath query)
        {
            var memento = BookHistoryCollection.Current.GetFolderMemento(query.SimplePath);
            return memento.IsFolderRecursive;
        }

        /// <summary>
        /// 指定パスのブックならば閉じる
        /// </summary>
        /// <param name="path">パス</param>
        /// <returns>閉じたなら true</returns>
        public static async ValueTask<CloseBookResult> CloseBookAsync(string path)
        {
            return await CloseBookAsync(new List<string>() { path });
        }

        /// <summary>
        /// 指定パスのブックならば閉じる
        /// </summary>
        /// <param name="paths">パス候補</param>
        /// <returns>閉じたなら true</returns>
        public static async ValueTask<CloseBookResult> CloseBookAsync(IEnumerable<string> paths)
        {
            bool isClosed;
            var bookAddress = BookHub.Current.Address;
            if (bookAddress != null && paths.Contains(bookAddress))
            {
                // 本を閉じる
                await BookHub.Current.RequestUnload(null, false).WaitAsync();
                isClosed = true;
            }
            else
            {
                isClosed = false;
            }

            return new CloseBookResult(isClosed, BookHub.Current.RequestLoadCount);
        }

        /// <summary>
        /// 本を開きなおす。Renames処理で使用する
        /// </summary>
        /// <param name="path">開くパス</param>
        /// <param name="oldPath">古いパス</param>
        /// <param name="requestLoadCount">閉じたときのリクエストカウント。変化があれば別の要求があったとみなして本は開かない</param>
        public static void RestoreBook(string path, string oldPath, int requestLoadCount)
        {
            if (path is null) return;

            // 履歴を新しい名前に変更
            if (oldPath != null)
            {
                BookMementoCollection.Current.RenameRecursive(oldPath, path);
                QuickAccessCollection.Current.RenameRecursive(oldPath, path);
                PlaylistHub.Current.RenameItemPathRecursive(oldPath, path);
            }

            // 本を開く
            if (requestLoadCount == BookHub.Current.RequestLoadCount)
            {
                BookHub.Current.RequestLoad(null, path, null, BookLoadOption.Resume | BookLoadOption.IsBook | BookLoadOption.Rename, false);
            }
        }
    }

    public struct CloseBookResult
    {
        public CloseBookResult(bool isClosed, int requestLoadCount)
        {
            IsClosed = isClosed;
            RequestLoadCount = requestLoadCount;
        }

        public bool IsClosed { get; }
        public int RequestLoadCount { get; }
    }
}

