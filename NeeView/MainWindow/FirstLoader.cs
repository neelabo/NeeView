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
        private List<string>? _bookPaths;
        private string? _folderPath;
        private bool _isFolderLink;
        private BookLoadOption _bookLoadOptions;


        public void Load()
        {
            _bookPaths = null;
            _folderPath = App.Current.Option.FolderListQuery?.SimpleQuery;

            if (Config.Current.StartUp.LastBookPath?.StartsWith(Temporary.Current.TempRootPath, StringComparison.Ordinal) == true)
            {
                Config.Current.StartUp.LastBookPath = null;
            }

            if (Config.Current.StartUp.LastFolderPath?.StartsWith(Temporary.Current.TempRootPath, StringComparison.Ordinal) == true)
            {
                Config.Current.StartUp.LastFolderPath = null;
            }

            SetBookPlace();
            SetFolderPlace();
            LoadBook();
            LoadFolder();
        }

        private void SetBookPlace()
        {
            if (App.Current.Option.IsBlank == SwitchOption.on)
            {
                _bookPaths = null;
                return;
            }

            // 起動引数の場所で開く
            if (App.Current.Option.Values.Count >= 1)
            {
                _bookPaths = App.Current.Option.Values.ToList();
                return;
            }

            // 最後に開いたブックを復元する
            if (Config.Current.StartUp.IsOpenLastBook)
            {
                var path = Config.Current.StartUp.LastBookPath;
                if (path != null)
                {
                    _bookPaths = new List<string> { path };
                    _bookLoadOptions = BookLoadOption.Resume | BookLoadOption.IsBook;

                    if (_folderPath == null && Config.Current.StartUp.LastFolderPath != null)
                    {
                        // 前回開いていたフォルダーがブックマークであった場合、なるべくそのフォルダーをひらく
                        if (QueryScheme.Bookmark.IsMatch(Config.Current.StartUp.LastFolderPath))
                        {
                            var node = BookmarkCollection.Current.FindNode(Config.Current.StartUp.LastFolderPath);
                            if (node != null && node.Select(e => e.Value).OfType<Bookmark>().Any(e => e.Path == path))
                            {
                                _folderPath = Config.Current.StartUp.LastFolderPath;
                                _isFolderLink = true;
                                return;
                            }
                        }
                        // 前回開いていたフォルダーがプレイリストあった場合、なるべくそのフォルダーを開く
                        if (PlaylistArchive.IsSupportExtension(Config.Current.StartUp.LastFolderPath))
                        {
                            try
                            {
                                var playlist = PlaylistSourceTools.Load(Config.Current.StartUp.LastFolderPath);
                                if (playlist.Items.Any(e => e.Path == path))
                                {
                                    _folderPath = Config.Current.StartUp.LastFolderPath;
                                    _isFolderLink = true;
                                    return;
                                }
                            }
                            catch
                            {
                                // nop.
                            }
                        }
                    }
                    return;
                }
            }
        }

        private void SetFolderPlace()
        {
            if (_folderPath != null)
            {
                return;
            }

            // Bookが指定されていなければ既定の場所を開く
            if (_bookPaths == null)
            {
                // 前回開いていたフォルダーを復元する
                if (Config.Current.StartUp.LastFolderPath != null)
                {
                    _folderPath = Config.Current.StartUp.LastFolderPath;
                    return;
                }

                // ホームフォルダ
                _folderPath = BookshelfFolderList.Current.GetFixedHome().SimpleQuery;
                return;
            }
        }

        private void LoadBook()
        {
            if (_bookPaths != null)
            {
                var options = _bookLoadOptions | BookLoadOption.FocusOnLoaded;
                BookHubTools.RequestLoad(this, _bookPaths, options, _folderPath == null);
            }
        }

        private void LoadFolder()
        {
            if (_folderPath != null)
            {
                var select = _isFolderLink ? new FolderItemPosition(new QueryPath(_bookPaths?.FirstOrDefault())) : null;
                BookshelfFolderList.Current.RequestPlace(new QueryPath(_folderPath), select, FolderSetPlaceOption.UpdateHistory);
            }
        }
    }

}
