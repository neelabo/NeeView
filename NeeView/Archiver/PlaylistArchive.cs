using NeeLaboratory.Linq;
using NeeView;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイバー：プレイリスト方式
    /// </summary>
    public class PlaylistArchive : Archive
    {
        public const string Extension = ".nvpls";


        public PlaylistArchive(string path, ArchiveEntry? source, ArchiveHint archiveHint) : base(path, source, archiveHint)
        {
        }


        public static bool IsSupportExtension(string path)
        {
            return LoosePath.GetExtension(path) == Extension;
        }

        public override string ToString()
        {
            return TextResources.GetString("Archiver.Playlist");
        }

        // サポート判定
        public override bool IsSupported()
        {
            return true;
        }

        // リスト取得
        protected override async ValueTask<List<ArchiveEntry>> GetEntriesInnerAsync(bool decrypt, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var playlist = PlaylistSourceTools.LoadFileResolved(Path);
            var list = new List<ArchiveEntry>();

            foreach (var item in playlist.Items)
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    var entry = await CreateEntryAsync(item, list.Count, token);
                    list.Add(entry);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            Debug.Assert(list.All(e => e is PlaylistArchiveEntry));
            return list;
        }

        private async ValueTask<PlaylistArchiveEntry> CreateEntryAsync(PlaylistSourceItem item, int id, CancellationToken token)
        {
            var targetPath = item.Path;

            // プレイリストに動画ブックの特殊形式 (/path/to/movie.mp4/movie.mp4) があるときの補正
            if (ArchiveManager.Current.GetSupportedType(targetPath) == ArchiveType.MediaArchive && !File.Exists(targetPath))
            {
                var targetDirectory = LoosePath.GetDirectoryName(targetPath);
                if (ArchiveManager.Current.GetSupportedType(targetDirectory) == ArchiveType.MediaArchive)
                {
                    targetPath = targetDirectory;
                }
            }

            var innerEntry = await ArchiveEntryUtility.CreateAsync(targetPath, ArchiveHint.None, false, token);

            var entry = new PlaylistArchiveEntry(this)
            {
                IsValid = true,
                Id = id,
                RawEntryName = item.Name,
                Length = innerEntry.Length,
                CreationTime = innerEntry.CreationTime,
                LastWriteTime = innerEntry.LastWriteTime,
                InnerEntry = innerEntry,
            };

            return entry;
        }

        // ストリームを開く
        protected override async ValueTask<Stream> OpenStreamInnerAsync(ArchiveEntry entry, bool decrypt, CancellationToken token)
        {
            Debug.Assert(entry.Archive == this);
            if (entry is not PlaylistArchiveEntry e) throw new InvalidCastException();
            return await e.InnerEntry.OpenEntryAsync(decrypt, token);
        }

        // ファイル出力
        protected override async ValueTask ExtractToFileInnerAsync(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            Debug.Assert(entry.Archive == this);
            if (entry is not PlaylistArchiveEntry e) throw new InvalidCastException();
            await e.InnerEntry.ExtractToFileAsync(exportFileName, isOverwrite, token);
        }

        /// <summary>
        /// can delete
        /// </summary>
        /// <exception cref="ArgumentException">Not registered with this archiver.</exception>
        public override bool CanDelete(List<ArchiveEntry> entries, bool strict)
        {
            if (entries.Any(e => e.Archive != this)) throw new ArgumentException("There are elements not registered with this archiver.", nameof(entries));
            if (!IsFileSystem) return false;

            return entries.All(e => e is PlaylistArchiveEntry);
        }

        /// <summary>
        /// delete entries
        /// </summary>
        /// <exception cref="ArgumentException">Not registered with this archiver.</exception>
        public override async ValueTask<DeleteResult> DeleteAsync(List<ArchiveEntry> entries)
        {
            if (entries.Any(e => e.Archive != this)) throw new ArgumentException("There are elements not registered with this archiver.", nameof(entries));

            var paths = entries.Select(e => e.EntityPath).WhereNotNull().ToList();
            if (!paths.Any()) return DeleteResult.Success;

            RemoveCachedEntry(entries);
            PlaylistTools.Delete(Path, entries.OfType<PlaylistArchiveEntry>().Select(e => CreateSourceItem(e)).ToList());

            return DeleteResult.Success;
        }

        /// <summary>
        /// can rename?
        /// </summary>
        public override bool CanRename(ArchiveEntry entry)
        {
            if (entry.Archive != this) throw new ArgumentException("There are elements not registered with this archiver.", nameof(entry));
            if (!IsFileSystem) return false;

            return entry is PlaylistArchiveEntry;
        }

        /// <summary>
        /// rename
        /// </summary>
        public override async ValueTask<bool> RenameAsync(ArchiveEntry entry, string name)
        {
            if (entry.Archive != this) throw new ArgumentException("There are elements not registered with this archiver.", nameof(entry));
            if (!IsFileSystem) return false;
            if (entry is not PlaylistArchiveEntry) return false;

            var newName = PlaylistTools.Rename(Path, CreateSourceItem(entry), name);
            if (newName is not null)
            {
                entry.RawEntryName = newName;
                return true;
            }

            return false;
        }

        private static PlaylistSourceItem CreateSourceItem(ArchiveEntry entry)
        {
            return new PlaylistSourceItem(entry.SystemPath, entry.EntryName);
        }
    }
}

