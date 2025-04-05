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
    /// アーカイブエントリをディレクトリを含めて展開する
    /// </summary>
    public abstract class ArchiveExtractor
    {
        private readonly Archive _archive;

        public ArchiveExtractor(Archive archive)
        {
            _archive = archive;
        }

        /// <summary>
        /// エントリをファイルまたはディレクトリにエクスポート
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="exportFileName">エクスポート先のパス</param>
        /// <param name="isOverwrite">上書き許可</param>
        /// <param name="token"></param>
        public virtual async Task ExtractAsync(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            Debug.Assert(entry.Archive == _archive);

            // MTAスレッドに限定して実行する
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                await Task.Run(async () => await ExtractAsyncInner(entry, exportFileName, isOverwrite, token));
            }
            else
            {
                await ExtractAsyncInner(entry, exportFileName, isOverwrite, token);
            }
        }

        /// <summary>
        /// エントリをファイルまたはディレクトリにエクスポート (実体)
        /// </summary>
        private async Task ExtractAsyncInner(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            NVDebug.AssertMTA();
            Debug.Assert(entry.Archive == _archive);
            Debug.Assert(entry is not null);
            Debug.Assert(!string.IsNullOrEmpty(exportFileName));

            if (_archive.IsDisposed) return;

            if (entry.IsDirectory)
            {
                await ExtractDirectoryEntry(entry, exportFileName, isOverwrite, true, token);
            }
            else
            {
                await ExtractFileEntry(entry, exportFileName, isOverwrite, token);
            }
        }

        /// <summary>
        /// ファイルエントリのエクスポート
        /// </summary>
        private async Task ExtractFileEntry(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            if (entry.Id < 0) throw new ArgumentException("Cannot extract this entry: " + entry.EntryName);
            if (entry.IsDirectory) throw new InvalidOperationException("Archive directory: " + entry.EntryName);

            token.ThrowIfCancellationRequested();
            await ExtractEntry(entry, exportFileName, isOverwrite, token);
        }

        /// <summary>
        /// ディレクトリエントリのエクスポート
        /// </summary>
        private async Task ExtractDirectoryEntry(ArchiveEntry entry, string exportFileName, bool isOverwrite, bool decrypt, CancellationToken token)
        {
            if (!entry.IsDirectory) throw new InvalidOperationException("Not archive directory: " + entry.EntryName);

            if (!isOverwrite && Directory.Exists(exportFileName)) throw new IOException($"Directory already exists: {exportFileName}");

            var prefix = CreateEntryPrefix(entry);

            var entries = await CollectEntriesAsync(prefix, decrypt, token);
            if (entries.Count == 0) throw new InvalidOperationException();

            foreach (var child in entries)
            {
                token.ThrowIfCancellationRequested();
                var output = FileIO.CreateUniquePath(LoosePath.Combine(exportFileName, LoosePath.ValidPath(child.EntryName[prefix.Length..])));
                await ExtractEntry(child, output, false, token);
            }
        }

        /// <summary>
        /// エントリのエクスポート
        /// </summary>
        private async Task ExtractEntry(ArchiveEntry entry, string path, bool overwrite, CancellationToken token)
        {
            if (entry.IsDirectory)
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                var outputDir = System.IO.Path.GetDirectoryName(path) ?? throw new IOException($"Illegal path: {path}");
                if (!string.IsNullOrWhiteSpace(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }
                await ExtractCore(entry, path, overwrite, token);
            }
        }

        /// <summary>
        /// エントリのファイル展開 コア
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="exportFileName"></param>
        /// <param name="isOverwrite"></param>
        protected abstract Task ExtractCore(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token);

        /// <summary>
        /// ディレクトリエントリ以下のエントリ識別用プレフィックスを作成
        /// </summary>
        /// <param name="entry">ディレクトリエントリ</param>
        /// <returns></returns>
        private static string CreateEntryPrefix(ArchiveEntry entry)
        {
            Debug.Assert(entry.IsDirectory);
            return LoosePath.TrimDirectoryEnd(entry.EntryName);
        }

        /// <summary>
        /// ディレクトリエントリ以下のエントリを収集する
        /// </summary>
        /// <param name="entryPrefix"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<ArchiveEntry>> CollectEntriesAsync(string entryPrefix, bool decrypt, CancellationToken token)
        {
            return (await _archive.GetEntriesAsync(decrypt, token)).Where(e => e.EntryName.StartsWith(entryPrefix, StringComparison.Ordinal)).ToList();
        }
    }
}
