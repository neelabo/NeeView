using NeeLaboratory.Linq;
using NeeLaboratory.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// TODO: 書庫内書庫 ストリームによる多重展開が可能？

namespace NeeView
{
    /// <summary>
    /// アーカイバー：標準Zipアーカイバー
    /// </summary>
    public class ZipArchive : Archive
    {
        private readonly AsyncLock _asyncLock = new();
        private Encoding? _encoding;


        public ZipArchive(string path, ArchiveEntry? source, ArchiveHint archiveHint) : base(path, source, archiveHint)
        {
        }


        public AsyncLock AsyncLock => _asyncLock;


        public override string ToString()
        {
            return Properties.TextResources.GetString("Archiver.Zip");
        }

        // サポート判定
        public override bool IsSupported()
        {
            return true;
        }

        /// <summary>
        /// ZIPヘッダチェック
        /// </summary>
        /// <returns></returns>
        private static bool CheckSignature(Stream stream)
        {
            var pos = stream.Position;

            byte[] signature = new byte[4];
            _ = stream.Read(signature, 0, 4);
            stream.Seek(pos, SeekOrigin.Begin);

            return (BitConverter.ToString(signature, 0) == "50-4B-03-04");
        }


        // エントリーリストを得る
        protected override async ValueTask<List<ArchiveEntry>> GetEntriesInnerAsync(bool decrypt, CancellationToken token)
        {
            var list = new List<ArchiveEntry>();
            var directories = new List<ArchiveEntry>();

            FileStream? stream = null;
            try
            {
                stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read);

                // ヘッダチェック
                if (!CheckSignature(stream))
                {
                    throw new FormatException(string.Format(CultureInfo.InvariantCulture, Properties.TextResources.GetString("NotZipException.Message"), Path));
                }

                // 文字エンコード取得
                _encoding = GetEncoding(stream);

                // エントリー取得
                stream.Seek(0, SeekOrigin.Begin);
                using (await _asyncLock.LockAsync(token))
                using (var archiver = new System.IO.Compression.ZipArchive(stream, ZipArchiveMode.Read, false, _encoding))
                {
                    stream = null;

                    var hashSet = new HashSet<string>(archiver.Entries.Count);

                    for (int id = 0; id < archiver.Entries.Count; ++id)
                    {
                        token.ThrowIfCancellationRequested();

                        var entry = archiver.Entries[id];

                        var archiveEntry = new ArchiveEntry(this)
                        {
                            IsValid = true,
                            Id = id,
                            Instance = new ZipArchiveEntryIdent(entry),
                            RawEntryName = entry.FullName,
                            Length = entry.Length,
                            CreationTime = default,
                            LastWriteTime = entry.LastWriteTime.LocalDateTime,
                        };

                        var success = hashSet.Add(archiveEntry.RawEntryName);
                        if (!success)
                        {
                            archiveEntry.Attributes |= ArchiveEntryAttributes.Duplicate;
                            foreach (var item in list.Where(e => e.RawEntryName == archiveEntry.RawEntryName))
                            {
                                item.Attributes |= ArchiveEntryAttributes.Duplicate;
                            }
                        }

                        if (!entry.IsDirectory())
                        {
                            list.Add(archiveEntry);
                        }
                        else
                        {
                            archiveEntry.Length = -1;
                            directories.Add(archiveEntry);
                        }
                    }

                    // 削除予約エントリを除外
                    if (ZipArchiveWriterManager.Current.Contains(Path))
                    {
                        list = list.Where(e => !ZipArchiveWriterManager.Current.Contains(Path, e.Instance as ZipArchiveEntryIdent)).ToList();
                    }

                    // ディレクトリエントリを追加
                    list.AddRange(CreateDirectoryEntries(list.Concat(directories)));
                }
            }
            finally
            {
                stream?.Dispose();
            }

            await ReadZoneIdentifierAsync(token);

            return list;
        }

        protected override async ValueTask<Stream> OpenStreamInnerAsync(ArchiveEntry entry, bool decrypt, CancellationToken token)
        {
            if (entry.Id < 0) throw new ArgumentException("Cannot open this entry: " + entry.EntryName);
            if (entry.IsDirectory) throw new InvalidOperationException("Cannot open directory: " + entry.EntryName);

            using (await _asyncLock.LockAsync(token))
            using (var archiver = ZipFile.Open(Path, ZipArchiveMode.Read, _encoding))
            {
                var rawEntry = archiver.FindEntry(entry);
                if (rawEntry is null) throw new FileNotFoundException("Entry not found: " + entry.EntryName);

                using (var stream = rawEntry.Open())
                {
                    var ms = new MemoryStream();
                    await stream.CopyToAsync(ms, token);
                    ms.Seek(0, SeekOrigin.Begin);
                    return ms;
                }
            }
        }

        /// <summary>
        /// 実体化可能なエントリ？
        /// </summary>
        public override bool CanRealize(ArchiveEntry entry)
        {
            Debug.Assert(entry.Archive == this);
            return true;
        }

        /// <summary>
        /// エントリをファイルまたはディレクトリにエクスポート
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="exportFileName">エクスポート先のパス</param>
        /// <param name="isOverwrite">上書き許可</param>
        /// <param name="token"></param>
        protected override async ValueTask ExtractToFileInnerAsync(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            var extractor = new ZipArchiveExtractor(this, _encoding);
            await extractor.ExtractAsync(entry, exportFileName, isOverwrite, token);
        }

        /// <summary>
        /// exists?
        /// </summary>
        public override bool Exists(ArchiveEntry entry)
        {
            Debug.Assert(entry.Archive == this);
            if (entry.Id < 0) return false;
            if (entry.Instance is not ZipArchiveEntryIdent) return false;

            if (entry.IsDeleted) return false;

            using (_asyncLock.Lock())
            using (var archiver = ZipFile.Open(Path, ZipArchiveMode.Read, _encoding))
            {
                var rawEntry = archiver.FindEntry(entry);
                var exists = rawEntry is not null;
                if (!exists)
                {
                    entry.IsDeleted = true;
                }
                return exists;
            }
        }

        /// <summary>
        /// can delete
        /// </summary>
        /// <exception cref="ArgumentException">Not registered with this archiver.</exception>
        public override bool CanDelete(List<ArchiveEntry> entries, bool strict)
        {
            if (entries.Any(e => e.Archive != this)) throw new ArgumentException("There are elements not registered with this archiver.", nameof(entries));

            if (!Config.Current.Archive.Zip.IsFileWriteAccessEnabled) return false;

            var isRootArchive = entries.All(e => e.Archive == this && e.Archive.IsRoot);
            if (!isRootArchive) return false;

            if (strict)
            {
                var fileInfo = new FileInfo(Path);
                if (!fileInfo.Exists || fileInfo.Attributes.HasFlag(FileAttributes.ReadOnly))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// delete entries
        /// </summary>
        /// <remarks>
        /// ZIPエントリを削除するタスク処理。
        /// すでにタスクが動作しているときは削除要求をそのタスクに追加する。
        /// </remarks>
        /// <exception cref="ArgumentException">Not registered with this archiver.</exception>
        public override async ValueTask<DeleteResult> DeleteAsync(List<ArchiveEntry> entries)
        {
            if (!IsRoot) throw new ArgumentException("The archive is not a file.");
            if (entries.Any(e => e.Archive != this)) throw new ArgumentException("There are elements not registered with this archiver.", nameof(entries));
            if (!entries.Any()) return DeleteResult.Success;

            var removes = entries;
            var directories = entries.Where(e => e.IsDirectory);
            if (directories.Any())
            {
                var all = await entries.First().Archive.GetEntriesAsync(true, CancellationToken.None);
                var children = directories.SelectMany(d => all.Where(e => e.Id >= 0 && e.EntryName.StartsWith(LoosePath.TrimDirectoryEnd(d.EntryName), StringComparison.Ordinal)));
                removes = entries.Concat(children).Where(e => e.Id >= 0).Distinct().ToList();
            }
            Debug.Assert(removes.All(e => e.Id >= 0));
            if (removes.Count == 0) return DeleteResult.Success;

            var fileInfo = new FileInfo(Path);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException("The file does not exist.", Path);
            }
            if (fileInfo.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                throw new UnauthorizedAccessException($"The file is read-only.");
            }

            RemoveCachedEntry(removes);
            var idents = removes.Select(e => e.Instance as ZipArchiveEntryIdent).WhereNotNull().ToList();
            var task = ZipArchiveWriterManager.Current.CreateDeleteTask(Path, _encoding, idents, _asyncLock);
            if (task is not null)
            {
                await task;
                return DeleteResult.Success;
            }
            else
            {
                return DeleteResult.Ordered;
            }
        }

        /// <summary>
        /// Zipの文字エンコードを取得。既定(UTF8)ならば null を返す。
        /// </summary>
        /// <param name="stream">Zip stream</param>
        /// <returns></returns>
        private Encoding? GetEncoding(Stream stream)
        {
            return Config.Current.Archive.Zip.Encoding switch
            {
                ZipEncoding.Local
                    => Environment.Encoding,
                ZipEncoding.UTF8
                    => null,
                ZipEncoding.Auto
                    => IsUTF8EncodingMaybe(stream) ? null : Environment.Encoding,
                _
                    => null,
            };
        }

        /// <summary>
        /// Zipの文字エンコードがUTF8であるかを判定
        /// </summary>
        /// <param name="stream">Zip stream</param>
        /// <returns></returns>
        private bool IsUTF8EncodingMaybe(Stream stream)
        {
            try
            {
                using var analyzer = new ZipAnalyzer(stream, true);
                return analyzer.IsEncodingUTF8();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        // NOTE：ZipArchiveの項目削除にはいろいろ課題があるため保留
        // - 処理によりIDがずれてしまうためブックを開き直す必要がある -> ファイルと同様の操作感にならない
        //   - 同様の問題は削除処理にも存在するため、ブックのIDのずれを補正する包括的な処理がほしい
        // - ZipArchiveに名前変更機能がないため、削除追加という処理になってしまい、重く日付も変更されてしまう ... 7Zip だとどうだろう？
        // - フォルダーの変更処理。エントリすべてを変更する必要がある...重そうだな
#if false
        /// <summary>
        /// can rename?
        /// </summary>
        public override bool CanRename(ArchiveEntry entry)
        {
            if (entry.Archive != this) throw new ArgumentException("There are elements not registered with this archiver.", nameof(entry));

            return IsRoot && Config.Current.Archive.Zip.IsFileWriteAccessEnabled;
        }

        /// <summary>
        /// rename
        /// </summary>
        public override async ValueTask<bool> RenameAsync(ArchiveEntry entry, string name)
        {
            if (entry.Archive != this) throw new ArgumentException("There are elements not registered with this archiver.", nameof(entry));
            if (!IsRoot) throw new ArgumentException("The archive is not a file.");

            //throw new NotImplementedException();

            var tempFilename = this.Path;

            using (var archive = ZipFile.Open(tempFilename, ZipArchiveMode.Update, _encoding))
            {
                ClearEntryCache();

                var oldEntry = GetTargetEntry(archive, entry);
                if (oldEntry is null) return false;

                var directory = LoosePath.GetDirectoryName(oldEntry.FullName);
                var newEntryName = LoosePath.Combine(directory, name, '/');

                var mem = new MemoryStream();
                using (var stream = oldEntry.Open())
                {
                    await stream.CopyToAsync(mem);
                    await stream.FlushAsync();
                }

                oldEntry.Delete();

                var newEntry = archive.CreateEntry(newEntryName);
                using (var stream = newEntry.Open())
                {
                    mem.Seek(0, SeekOrigin.Begin);
                    await mem.CopyToAsync(stream);
                    await mem.FlushAsync();
                }
                newEntry.LastWriteTime = oldEntry.LastWriteTime;

                entry.RawEntryName = newEntryName;
            }

            // TODO: 現在ブックを開き直す
            return true;

            static ZipArchiveEntry? GetTargetEntry(ZipArchive archive, ArchiveEntry entry)
            {
                var zipArchiveEntry = archive.Entries[entry.Id];
                ZipArchiveEntryHelper.RepairEntryName(zipArchiveEntry);
                return IsValidEntry(entry, zipArchiveEntry) ? zipArchiveEntry : null;
            }
        }
#endif
    }
}
