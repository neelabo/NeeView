//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using NeeLaboratory.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// 存在しないパスの復元を試みる
    /// </summary>
    [LocalDebug]
    public partial class FileResolver
    {
        private static readonly Lazy<FileResolver> _current = new();
        public static FileResolver Current => _current.Value;

        private readonly Lazy<FileIdDatabase> _fileIdTable = new(() => new(Database.Current));
        private readonly Lazy<VolumeDatabaseCache> _volumeTable = new(() => new(Database.Current));


        public FileResolver()
        {
            BookmarkCollection.Current.SubscribeBookmarkChanged(BookmarkCollection_Changed);
            PlaylistHub.Current.SubscribePlaylistCollectionChanged(PlaylistCollection_Changed);
            FolderConfigCollection.Current.SubscribeFolderChanged(FolderConfigCollection_Changed);
        }


        private FileIdDatabase FileIdTable => _fileIdTable.Value;
        private VolumeDatabaseCache VolumeTable => _volumeTable.Value;


        /// <summary>
        /// ブックマーク追加時に登録する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BookmarkCollection_Changed(object? sender, BookmarkCollectionChangedEventArgs e)
        {
            if (e.Action == EntryCollectionChangedAction.Add)
            {
                if (e.Item?.Value is Bookmark bookmark)
                {
                    LocalDebug.WriteLine($"Bookmark.Add: {bookmark.Path}");
                    AddArchivePath(bookmark.Path);
                }
            }
        }

        /// <summary>
        /// プレイリスト追加時に登録する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void PlaylistCollection_Changed(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var newItems = e.NewItems?.Cast<PlaylistItem>().ToList() ?? throw new InvalidOperationException("newItems must not be null when Replace");
                foreach (var item in newItems)
                {
                    LocalDebug.WriteLine($"Playlist.Add: {item.Path}");
                    AddArchivePath(item.Path);
                }
            }
        }

        /// <summary>
        /// デフォルトでないフォルダー設定追加時に登録する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderConfigCollection_Changed(object? sender, FolderConfigChangedEventArgs e)
        {
            if (e.FolderConfig is null) return;

            switch (e.Action)
            {
                case FolderConfigChangedAction.Add:
                    if (!e.FolderConfig.IsDefault())
                    {
                        LocalDebug.WriteLine($"FolderConfig.Add: {e.FolderConfig.Place}");
                        if (QuerySchemeExtensions.GetScheme(e.FolderConfig.Place) == QueryScheme.File)
                        {
                            AddArchivePath(e.FolderConfig.Place);
                        }
                    }
                    break;
                case FolderConfigChangedAction.Replace:
                    if (e.ThumbsChanged)
                    {
                        AddArchivePath(e.FolderConfig.Place);
                    }
                    break;
            }
        }

        /// <summary>
        /// アーカイブパスをまとめて登録する
        /// </summary>
        /// <param name="paths">登録するアーカイブパス</param>
        /// <returns></returns>
        public int AddRangeArchivePath(IEnumerable<string> paths)
        {
            int count = 0;
            var sw = Stopwatch.StartNew();

            using (var transaction = Database.Current.BeginTransaction())
            {
                try
                {
                    var targets = paths.Select(e => ArchivePath.GetSystemPath(e, true)).WhereNotNull().Distinct();
                    foreach (var path in targets)
                    {
                        var swNow = sw.ElapsedMilliseconds;
                        var result = AddArchivePath(path);
                        if (result) count++;
                        LocalDebug.WriteLine($"Cost: {sw.ElapsedMilliseconds - swNow} ms");
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            sw.Stop();
            LocalDebug.WriteLine($"Total: Count = {count}, {sw.ElapsedMilliseconds} ms");
            return count;
        }

        /// <summary>
        /// アーカイブパスを復元対象として登録する
        /// </summary>
        /// <param name="path">登録するアーカイブパス</param>
        /// <returns></returns>
        public bool AddArchivePath(string path)
        {
            var systemPath = ArchivePath.GetSystemPath(path, true);
            return systemPath is not null && Add(systemPath, false);
        }


        /// <summary>
        /// ターゲットが存在する場合のみ パスを復元対象として登録する
        /// </summary>
        /// <remarks>
        /// 復元対象のパスを追加するために使用する。
        /// ターゲットが存在しない場合、そもそも復元対象ではないということ。
        /// </remarks>
        /// <param name="path">登録するパス</param>
        /// <param name="target">存在チェック用パス</param>
        /// <param name="excludeUnc">UNCパスを除外する</param>
        /// <returns></returns>
        public bool AddIfTargetExists(string path, string target, bool excludeUnc = true)
        {
            LocalDebug.WriteLine($"Path: {path}");

            // UNCパスは除外
            if (excludeUnc && LoosePath.IsUnc(path))
            {
                LocalDebug.WriteLine($"UNC skip.");
                return false;
            }

            if (!FileIdTable.TargetExists(target))
            {
                return false;
            }

            return Add(path, false);
        }

        /// <summary>
        /// パスを復元対象として登録する
        /// </summary>
        /// <param name="path">登録するパス</param>
        /// <param name="excludeUnc">UNCパスを除外する</param>
        /// <returns></returns>
        public bool Add(string path, bool excludeUnc = true)
        {
            LocalDebug.WriteLine($"Path: {path}");

            // UNCパスは除外
            if (excludeUnc && LoosePath.IsUnc(path))
            {
                LocalDebug.WriteLine($"UNC skip.");
                return false;
            }

            // パスが存在しない場合は登録解除して false を返す
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                LocalDebug.WriteLine($"File not found.");
                //_database.Remove(path); .. 解除必要か？
                return false;
            }

            try
            {
                // パスのFileIDを取得して登録する
                var fileId = FileIdResolver.GetFileIdFromPath(path);
                LocalDebug.WriteLine($"Add:{fileId}");
                FileIdTable.Write(path, fileId.ToFileIdEx(VolumeTable));
                return true;
            }
            catch (Exception ex)
            {
                // 取得に失敗した場合は登録解除して false を返す
                LocalDebug.WriteLine($"Failed: {ex.Message}");
                //Remove(path);
                return false;
            }
        }

        /// <summary>
        /// パスを復元する
        /// </summary>
        /// <param name="path">パス</param>
        /// <param name="excludeUnc">UNCパスを除外し、null を返す</param>
        /// <returns>実在する入力パスもしくは復元されたパス。復元できなかったら null</returns>
        public string? Resolve(string path, bool excludeUnc = true)
        {
            LocalDebug.WriteLine($"Path: {path}");

            if (excludeUnc && LoosePath.IsUnc(path))
            {
                LocalDebug.WriteLine($"UNC skip.");
                return null;
            }

            if (File.Exists(path) || Directory.Exists(path))
            {
                LocalDebug.WriteLine($"Exists.");
                return path;
            }

            var fileId = FileIdTable.Read(path)?.ToFileId(VolumeTable);
            if (fileId == null)
            {
                LocalDebug.WriteLine($"Cache not found.");
                return null;
            }

            var resolved = FileIdResolver.ResolvePathFromFileId(fileId);
            LocalDebug.WriteLine($"Resolved: {resolved}");
            if (resolved is null)
            {
                return null;
            }

            // 簡易ゴミ箱判定
            if (resolved.IndexOf(@"\$RECYCLE.BIN\", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                LocalDebug.WriteLine($"in $Recycle.Bin");
                return null;
            }

            return resolved;
        }

        /// <summary>
        /// アーカイブパスを復元する
        /// </summary>
        /// <param name="path">アーカイブパス</param>
        /// <param name="excludeUnc">UNCパスを除外し、null を返す</param>
        /// <returns>実在もしくは復元されたアーカイブパス。復元できなかったら null</returns>
        public ArchivePath? ResolveArchivePath(string path, bool excludeUnc = true)
        {
            Debug.Assert(System.IO.Path.IsPathFullyQualified(path));

            // UNCパスは除外
            if (excludeUnc && LoosePath.IsUnc(path))
            {
                LocalDebug.WriteLine($"UNC skip.");
                return null;
            }

            // まず完全一致を試みる
            var resolved = Resolve(path, false);
            if (resolved != null)
            {
                return new ArchivePath(resolved, 0);
            }

            // アーカイブパスの可能性を考える
            // 実在するならそのまま帰す
            var archivePath = ArchivePath.Create(path);
            if (archivePath.IsArchivePath)
            {
                return archivePath;
            }

            // アーカイブパスだった可能も考えつつ復元を試みる
            // 末端から順に区切り文字を探し、そこまでをシステムパスとして解決を試みる
            int index = path.Length - 1;
            while (index >= 0)
            {
                if (path[index] == '\\' || path[index] == '/')
                {
                    string systemPath = path.Substring(0, index);
                    var systemResolved = Resolve(systemPath, false);
                    if (systemResolved != null)
                    {
                        if (File.Exists(systemResolved))
                        {
                            // 有効ならアーカイブパスを返す
                            resolved = systemResolved + path.Substring(index);
                            return new ArchivePath(resolved, systemResolved.Length);
                        }
                        else
                        {
                            // ディレクトリはアーカイブパスではないので検索の対象外で、取得失敗とする
                            return null;
                        }
                    }
                }
                index--;
            }

            return null;
        }

        /// <summary>
        /// データベースに登録された同じFileIdのパスを取得する
        /// </summary>
        /// <param name="path">FileIdのもととなるパス</param>
        /// <param name="predicate">パスの受け入れ条件</param>
        /// <param name="excludeUnc">UNCパスを除外し、nullを返す</param>
        /// <returns></returns>
        public string? GetOldPath(string path, Func<string, bool> predicate, bool excludeUnc = true)
        {
            Debug.Assert(System.IO.Path.IsPathFullyQualified(path));

            if (excludeUnc && LoosePath.IsUnc(path))
            {
                LocalDebug.WriteLine($"UNC skip.");
                return null;
            }

            var fileId = FileIdResolver.GetFileIdFromPath(path);
            var resolved = FileIdTable.GetPath(fileId.ToFileIdEx(VolumeTable), predicate);
            return resolved;
        }

        /// <summary>
        /// 登録解除
        /// </summary>
        /// <param name="path"></param>
        public void Remove(string path)
        {
            LocalDebug.WriteLine($"Path: {path}");
            FileIdTable.Delete(path);
        }
    }
}

