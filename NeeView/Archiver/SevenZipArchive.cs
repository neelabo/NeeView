using NeeView.Properties;
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
        private SevenZipArchiveInfo _archiveInfo;

        public SevenZipArchive(string path, ArchiveEntry? source, ArchiveHint archiveHint) : base(path, source, archiveHint)
        {
            _accessor = new SevenZipAccessor(Path);
        }


        public bool IsSolid => _archiveInfo.IsSolid;

        public string? Format => _archiveInfo.Format;


        public override string ToString()
        {
            return TextResources.GetString("Archiver.SevenZip") + (Format != null ? $" ({Format})" : null);
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
        public bool Initialized() => Format != null;

        // エントリーリストを得る
        protected override async ValueTask<List<ArchiveEntry>> GetEntriesInnerAsync(bool decrypt, CancellationToken token)
        {
            NVDebug.AssertMTA();
            token.ThrowIfCancellationRequested();

            if (_disposedValue) return new List<ArchiveEntry>();

            var list = new List<ArchiveEntry>();
            var directories = new List<ArchiveEntry>();

            // NOTE: 最初のアーカイブ初期化に時間がかかることがあるが、外部DLL内なのでキャンセルできない。
            // NOTE: アーカイブの種類によっては進捗が取得できるようだ。
            (_archiveInfo, var entries) = _accessor.GetArchiveInfo(decrypt);

            Encrypted = _archiveInfo.Encrypted || entries.Any(e => e.Encrypted);

            for (int id = 0; id < entries.Count; ++id)
            {
                token.ThrowIfCancellationRequested();

                var entry = entries[id];
                Debug.Assert(entry.Index == id);

                var archiveEntry = new ArchiveEntry(this)
                {
                    IsValid = true,
                    Id = id,
                    RawEntryName = entry.FileName,
                    Length = (long)entry.Size,
                    CreationTime = entry.CreationTime,
                    LastWriteTime = entry.LastWriteTime,
                    Encrypted = entry.Encrypted,
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
        protected override async ValueTask<Stream> OpenStreamInnerAsync(ArchiveEntry entry, bool decrypt, CancellationToken token)
        {
            NVDebug.AssertMTA();
            Debug.Assert(entry is not null);
            Debug.Assert(Initialized());
            Debug.Assert(!CanPreExtract(), "Pre-extract, so no direct extract.");
            if (entry.Id < 0) throw new ArgumentException("Cannot open this entry: " + entry.EntryName);

            ThrowIfDisposed();

#if DEBUG
            var archiveEntry = _accessor.ArchiveFileData[entry.Id];
            if (archiveEntry.FileName != entry.RawEntryName)
            {
                throw new ApplicationException(TextResources.GetString("InconsistencyException.Message"));
            }
#endif

            var ms = new MemoryStream((int)entry.Length);
            token.ThrowIfCancellationRequested();
            await Task.Run(() => _accessor.ExtractFile(entry.ToSevenZipFileInfo(), ms, decrypt));
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
        protected override async ValueTask ExtractToFileInnerAsync(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
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
                _accessor.ExtractFile(entry.ToSevenZipFileInfo(), fs, true);
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
            return IsSolid;
        }

        /// <summary>
        /// 事前展開処理
        /// </summary>
        public override async ValueTask PreExtractAsync(string directory, bool decrypt, CancellationToken token)
        {
            Debug.Assert(!string.IsNullOrEmpty(directory));

            if (_disposedValue) return;

            var entries = await GetEntriesAsync(decrypt, token);
            token.ThrowIfCancellationRequested();

            await _accessor.PreExtractAsync(Encrypted, directory, new SevenZipFileExtraction(entries), decrypt, token);
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
                _accessor?.Dispose();
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
