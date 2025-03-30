using SevenZip;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイバー：7z.dll
    /// </summary>
    public class SevenZipArchive : Archive
    {
        private readonly SevenZipAccessor _accessor;
        private string? _format;
        private bool _isSolid;


        public SevenZipArchive(string path, ArchiveEntry? source, ArchiveHint archiveHint) : base(path, source, archiveHint)
        {
            _accessor = new SevenZipAccessor(Path);
        }


        public override string ToString()
        {
            return Properties.TextResources.GetString("Archiver.SevenZip") + (_format != null ? $" ({_format})" : null);
        }

        public override void Unlock()
        {
            // 直接の圧縮ファイルである場合のみアンロック
            if (this.Parent == null || this.Parent is FolderArchive)
            {
                NVDebug.AssertMTA();
                _accessor.Unlock();
            }
        }


        // サポート判定
        public override bool IsSupported()
        {
            return true;
        }

        // [開発用] 初期化済？
        public bool Initialized() => _format != null;

        // エントリーリストを得る
        protected override async Task<List<ArchiveEntry>> GetEntriesInnerAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();
            token.ThrowIfCancellationRequested();

            if (_disposedValue) return new List<ArchiveEntry>();

            var list = new List<ArchiveEntry>();
            var directories = new List<ArchiveEntry>();

            // NOTE: 最初のアーカイブ初期化に時間がかかることがあるが、外部DLL内なのでキャンセルできない。
            // NOTE: アーカイブの種類によっては進捗が取得できるようだ。
            (_isSolid, _format, var entries) = _accessor.GetArchiveInfo();

            for (int id = 0; id < entries.Count; ++id)
            {
                token.ThrowIfCancellationRequested();

                var entry = entries[id];

                var archiveEntry = new ArchiveEntry(this)
                {
                    IsValid = true,
                    Id = id,
                    RawEntryName = entry.FileName,
                    Length = (long)entry.Size,
                    CreationTime = entry.CreationTime,
                    LastWriteTime = entry.LastWriteTime,
                };

                if (!entry.IsDirectory)
                {
                    list.Add(archiveEntry);
                }
                else
                {
                    archiveEntry.Length = -1;
                    directories.Add(archiveEntry);
                }
            }

            // ディレクトリエントリを追加
            list.AddRange(CreateDirectoryEntries(list.Concat(directories)));

            await ReadZoneIdentifierAsync(token);

            return list;
        }

        // エントリーのストリームを得る
        protected override async Task<Stream> OpenStreamInnerAsync(ArchiveEntry entry, CancellationToken token)
        {
            NVDebug.AssertMTA();
            Debug.Assert(entry is not null);
            Debug.Assert(Initialized());
            Debug.Assert(!CanPreExtract(), "Pre-extract, so no direct extract.");
            if (entry.Id < 0) throw new ArgumentException("Cannot open this entry: " + entry.EntryName);

            ThrowIfDisposed();

            var archiveEntry = _accessor.ArchiveFileData[entry.Id];
            if (archiveEntry.FileName != entry.RawEntryName)
            {
                throw new ApplicationException(Properties.TextResources.GetString("InconsistencyException.Message"));
            }

            var ms = new MemoryStream((int)entry.Length);
            token.ThrowIfCancellationRequested();
            await Task.Run(() => _accessor.ExtractFile(entry.Id, ms));
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
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
        protected override async Task ExtractToFileInnerAsync(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            Debug.Assert(entry.Archive == this);
            var extractor = new SevenZipArchiveExtractor(this);
            await extractor.ExtractAsync(entry, exportFileName, isOverwrite, token);
        }

        public void Extract(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            NVDebug.AssertMTA();
            Debug.Assert(entry.Archive == this);
            if (entry.Id < 0) throw new ApplicationException("Cannot open this entry: " + entry.EntryName);
            if (entry.IsDirectory) throw new ApplicationException("This entry is directory: " + entry.EntryName);

            var mode = isOverwrite ? FileMode.Create : FileMode.CreateNew;
            using (Stream fs = new FileStream(exportFileName, mode, FileAccess.Write))
            {
                _accessor.ExtractFile(entry.Id, fs);
            }
            WriteZoneIdentifier(exportFileName);
        }

        /// <summary>
        /// 事前展開？
        /// </summary>
        protected override bool CanPreExtractInner()
        {
            if (_disposedValue) return false;
            Debug.Assert(Initialized());
            return _isSolid;
        }

        /// <summary>
        /// 事前展開処理
        /// </summary>
        public override async Task PreExtractAsync(string directory, CancellationToken token)
        {
            Debug.Assert(!string.IsNullOrEmpty(directory));

            if (_disposedValue) return;

            var entries = await GetEntriesAsync(token);
            token.ThrowIfCancellationRequested();

            await _accessor.PreExtractAsync(directory, new SevenZipFileExtraction(entries), token);
            token.ThrowIfCancellationRequested();
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }
                _accessor.Dispose();
                _disposedValue = true;
                base.Dispose(disposing);
            }
        }
        #endregion
    }


    /// <summary>
    /// ストリーム展開時のエントリアクセサ
    /// </summary>
    public class SevenZipFileExtraction : ISevenZipFileExtraction
    {
        private readonly Dictionary<int, ArchiveEntry> _map;

        public SevenZipFileExtraction(List<ArchiveEntry> entries)
        {
            _map = entries.Where(e => e.Id >= 0).ToDictionary(e => e.Id);
        }

        public bool DataExists(ArchiveFileInfo info)
        {
            if (_map.TryGetValue(info.Index, out var entry))
            {
                return entry.Data is not null;
            }
            return true;
        }

        public void SetData(ArchiveFileInfo info, object data)
        {
            if (_map.TryGetValue(info.Index, out var entry))
            {
                entry.SetData(data);
            }
            else
            {
                Debug.Assert(false, "Don't come here: Entry not found");
            }
        }

        public void WriteZoneIdentifier(ArchiveFileInfo info)
        {
            if (_map.TryGetValue(info.Index, out var entry) && entry.Data is string path)
            {
                entry.Archive.WriteZoneIdentifier(path);
            }
        }
    }
}
