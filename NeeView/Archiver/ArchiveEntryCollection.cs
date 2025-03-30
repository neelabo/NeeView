using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{

    /// <summary>
    /// 指定パス以下のArchiveEntryの収集
    /// </summary>
    public class ArchiveEntryCollection
    {
        private readonly ArchiveEntryCollectionMode _mode;
        private readonly ArchiveEntryCollectionMode _modeIfArchive;
        private readonly bool _ignoreCache;
        private readonly ArchiveHint _archiveHint;
        private List<ArchiveEntryNode>? _entries;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path">対象のパス</param>
        /// <param name="mode">標準再帰モード</param>
        /// <param name="modeIfArchive">圧縮ファイルの再帰モード</param>
        /// <param name="option"></param>
        public ArchiveEntryCollection(string path, ArchiveEntryCollectionMode mode, ArchiveEntryCollectionMode modeIfArchive, ArchiveEntryCollectionOption option, ArchiveHint archiveHint)
        {
            Path = LoosePath.TrimEnd(path);
            Mode = mode;
            _mode = mode;
            _modeIfArchive = modeIfArchive;
            _ignoreCache = option.HasFlag(ArchiveEntryCollectionOption.IgnoreCache);
            _archiveHint = archiveHint;
        }

        public string Path { get; }
        public Archive? Archive { get; private set; }

        public ArchiveEntryCollectionMode Mode { get; private set; }

        /// <summary>
        /// ArchiveEntry収集
        /// </summary>
        public async Task<List<ArchiveEntryNode>> GetEntriesAsync(CancellationToken token)
        {
            if (_entries != null) return _entries;

            var rootEntry = await ArchiveEntryUtility.CreateAsync(Path, _archiveHint, token);

            Archive? rootArchive;
            string rootArchivePath;

            if (rootEntry.IsFileSystem)
            {
                if (rootEntry.IsDirectory)
                {
                    rootArchive = await ArchiveManager.Current.CreateArchiveAsync(StaticFolderArchive.Default.CreateArchiveEntry(Path, _archiveHint), _ignoreCache, token);
                    rootArchivePath = "";
                }
                else
                {
                    rootArchive = await ArchiveManager.Current.CreateArchiveAsync(rootEntry, _ignoreCache, token);
                    rootArchivePath = "";
                }
            }
            else
            {
                if (rootEntry.IsArchive() || rootEntry.IsMedia())
                {
                    rootArchive = await ArchiveManager.Current.CreateArchiveAsync(rootEntry, _ignoreCache, token);
                    rootArchivePath = "";
                }
                else
                {
                    rootArchive = rootEntry.Archive;
                    rootArchivePath = rootEntry.EntryName;
                }
            }

            if (rootArchive is null)
            {
                return new List<ArchiveEntryNode>() { new ArchiveEntryNode(null, rootEntry) };
            }

            Archive = rootArchive;

            Mode = (Archive is FolderArchive || Archive is PlaylistArchive) ? _mode : _modeIfArchive;

            var includeSubDirectories = Mode == ArchiveEntryCollectionMode.IncludeSubDirectories || Mode == ArchiveEntryCollectionMode.IncludeSubArchives;
            var entries = (await rootArchive.GetEntriesAsync(rootArchivePath, includeSubDirectories, token)).Select(e => new ArchiveEntryNode(null, e, rootArchivePath)).ToList();

            var includeAllSubDirectories = Mode == ArchiveEntryCollectionMode.IncludeSubArchives;
            if (includeAllSubDirectories)
            {
                entries = await GetSubArchivesEntriesAsync(entries, token);
            }

            _entries = entries;
            return _entries;
        }


        private async Task<List<ArchiveEntryNode>> GetSubArchivesEntriesAsync(List<ArchiveEntryNode> entries, CancellationToken token)
        {
            var result = new List<ArchiveEntryNode>();

            foreach (var entry in entries)
            {
                result.Add(entry);

                if (entry.ArchiveEntry.IsArchive())
                {
                    // 無限ループを避けるためショートカットは除外する
                    if (entry.ArchiveEntry.IsShortcut)
                    {
                        continue;
                    }

                    try
                    {
                        var entityEntry = entry.ArchiveEntry.TargetArchiveEntry;
                        var subArchive = await ArchiveManager.Current.CreateArchiveAsync(entityEntry, _ignoreCache, token);
                        var subEntries = (await subArchive.GetEntriesAsync(token)).Select(e => new ArchiveEntryNode(entry, e)).ToList();
                        result.AddRange(await GetSubArchivesEntriesAsync(subEntries, token));
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        Debug.WriteLine($"ArchiveEntryCollection.Skip: {entry.ArchiveEntry.EntryName}");
                    }
                }
            }

            return result;
        }


        // filter: ページとして画像ファイルのみリストアップ
        public async Task<List<ArchiveEntryNode>> GetEntriesWhereImageAsync(CancellationToken token)
        {
            var entries = await GetEntriesAsync(token);
            return entries.Where(e => e.ArchiveEntry.IsImage()).ToList();
        }

        // filter: ページとして画像ファイルとアーカイブをリストアップ
        public async Task<List<ArchiveEntryNode>> GetEntriesWhereImageAndArchiveAsync(CancellationToken token)
        {
            var entries = await GetEntriesAsync(token);
            if (Mode == ArchiveEntryCollectionMode.CurrentDirectory)
            {
                return entries.Where(e => e.ArchiveEntry.IsImage() || e.ArchiveEntry.IsBook()).ToList();
            }
            else
            {
                return entries.WherePageAll().Where(e => e.ArchiveEntry.IsImage() || e.ArchiveEntry.IsBook()).ToList();
            }
        }

        // filter: ページとしてすべてのファイルをリストアップ。フォルダーは空きフォルダーのみリストアップ
        public async Task<List<ArchiveEntryNode>> GetEntriesWherePageAllAsync(CancellationToken token)
        {
            var entries = await GetEntriesAsync(token);
            return entries.WherePageAll().ToList();
        }

        // filter: 含まれるサブアーカイブのみ抽出
        public async Task<List<ArchiveEntryNode>> GetEntriesWhereSubArchivesAsync(CancellationToken token)
        {
            var entries = await GetEntriesAsync(token);
            return entries.Where(e => e.ArchiveEntry.IsArchive() || e.ArchiveEntry.IsMedia()).ToList();
        }

        // filter: 含まれるブックを抽出
        public async Task<List<ArchiveEntryNode>> GetEntriesWhereBookAsync(CancellationToken token)
        {
            var entries = await GetEntriesAsync(token);
            if (Mode == ArchiveEntryCollectionMode.CurrentDirectory)
            {
                return entries.Where(e => e.ArchiveEntry.IsBook()).ToList();
            }
            else
            {
                return entries.Where(e => e.ArchiveEntry.IsBook() && !e.ArchiveEntry.IsArchiveDirectory()).ToList();
            }
        }

        /// <summary>
        /// フォルダーリスト上での親フォルダーを取得
        /// </summary>
        public string? GetFolderPlace()
        {
            if (Path == null || Archive == null)
            {
                return null;
            }

            if (Archive == null)
            {
                Debug.Assert(false, "Invalid operation");
                return null;
            }

            if (Mode == ArchiveEntryCollectionMode.IncludeSubArchives)
            {
                return LoosePath.GetDirectoryName(Archive.RootArchive?.SystemPath);
            }
            else if (Mode == ArchiveEntryCollectionMode.IncludeSubDirectories)
            {
                if (Archive.Parent != null)
                {
                    return Archive.Parent.SystemPath;
                }
                else
                {
                    return LoosePath.GetDirectoryName(Archive.SystemPath);
                }
            }
            else
            {
                return LoosePath.GetDirectoryName(Path);
            }
        }
    }

    public static class ArchiveEntryCollectionExtensions
    {
        /// <summary>
        /// filter: ディレクトリとなるエントリをすべて除外
        /// </summary>
        public static IEnumerable<ArchiveEntryNode> WherePageAll(this IEnumerable<ArchiveEntryNode> source)
        {
            var directories = source.Select(e => LoosePath.GetDirectoryName(e.ArchiveEntry.SystemPath)).Distinct().ToList();
            return source.Where(e => e.ArchiveEntry.IsShortcut || !directories.Contains(e.ArchiveEntry.SystemPath));
        }

        /// <summary>
        /// ArchiveEntryNode リストから ArchiveEntry リストを取得する
        /// </summary>
        public static List<ArchiveEntry> ToArchiveEntryCollection(this IEnumerable<ArchiveEntryNode> source)
        {
            return source.Select(e => e.ArchiveEntry).ToList();
        }
    }
}
