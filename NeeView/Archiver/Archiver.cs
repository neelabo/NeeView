﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイバー基底クラス
    /// </summary>
    public abstract class Archiver : IDisposable
    {
        private readonly ArchivePreExtractor _preExtractor;
        private int _preExtractorActivateCount;
        private bool _disposedValue;

        /// <summary>
        /// ArchiveEntry Cache
        /// </summary>
        private List<ArchiveEntry>? _entries;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path">アーカイブ実体へのパス</param>
        /// <param name="source">基となるエントリ</param>
        public Archiver(string path, ArchiveEntry? source)
        {
            Path = path;

            if (source != null)
            {
                Parent = source.Archiver;
                EntryName = source.EntryName;
                Id = source.Id;
                CreationTime = source.CreationTime;
                LastWriteTime = source.LastWriteTime;
                Length = source.Length;
                this.Source = source;
            }
            else if (string.IsNullOrEmpty(Path))
            {
                // for StaticArchive
                EntryName = "";
            }
            else
            {
                // ファイルシステムとみなし情報を取得する
                EntryName = LoosePath.GetFileName(Path);
                var fileSystemInfo = FileIO.CreateFileSystemInfo(Path);
                if (fileSystemInfo.Exists)
                {
                    Length = fileSystemInfo is FileInfo fileInfo ? fileInfo.Length : -1;
                    LastWriteTime = fileSystemInfo.LastWriteTime;
                }
            }

            _preExtractor = new ArchivePreExtractor(this);
            _preExtractor.Sleep();
        }


        // アーカイブ実体のパス
        public string Path { get; protected set; }

        // 内部アーカイブのテンポラリファイル。インスタンス保持用
        public FileProxy? ProxyFile { get; set; }

        // ファイルシステムの場合は true
        public virtual bool IsFileSystem { get; } = false;

        // ファイルシステムでのパスを取得
        // アーカイブ内パスの場合は null を返す
        public virtual string? GetFileSystemPath(ArchiveEntry entry) { return null; }

        // 対応判定
        public abstract bool IsSupported();

        /// <summary>
        /// 親アーカイブ
        /// </summary>
        public Archiver? Parent { get; private set; }

        /// <summary>
        /// 親アーカイブのエントリ表記
        /// </summary>
        public ArchiveEntry? Source { get; private set; }


        /// <summary>
        /// エントリでの名前
        /// </summary>
        public string EntryName { get; private set; }

        /// <summary>
        /// エントリでのID
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// アーカイブのサイズ
        /// </summary>
        public long Length { get; private set; }

        /// <summary>
        /// アーカイブの作成日時
        /// </summary>
        public DateTime CreationTime { get; private set; }

        /// <summary>
        /// アーカイブの最終更新日
        /// </summary>
        public DateTime LastWriteTime { get; private set; }

        /// <summary>
        /// ルート判定
        /// </summary>
        public bool IsRoot => Parent == null;

        /// <summary>
        /// ルートアーカイバー取得
        /// </summary>
        public Archiver RootArchiver => Parent == null ? this : Parent.RootArchiver;

        /// <summary>
        /// エクスプローラーで指定可能な絶対パス
        /// </summary>
        public string SystemPath => Parent == null ? Path : LoosePath.Combine(Parent.SystemPath, EntryName);

        /// <summary>
        /// 識別名
        /// </summary>
        public string Ident => (Parent == null || Parent is FolderArchive) ? Path : LoosePath.Combine(Parent.Ident, $"{Id}.{EntryName}");


        // 本来のファイルシスでのパスを取得
        public string GetSourceFileSystemPath()
        {
            if (IsCompressedChild() && this.Parent is not null)
            {
                return this.Parent.GetSourceFileSystemPath();
            }
            else
            {
                return LoosePath.TrimEnd(this.Path);
            }
        }

        // 圧縮ファイルの一部？
        public bool IsCompressedChild()
        {
            if (this.Parent != null)
            {
                if (this.Parent is FolderArchive)
                {
                    return this.Parent.IsCompressedChild();
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// ファイルロック解除
        /// </summary>
        public virtual void Unlock()
        {
        }


        /// <summary>
        /// エントリリストを取得 (Archive内でのみ使用)
        /// </summary>
        protected abstract Task<List<ArchiveEntry>> GetEntriesInnerAsync(CancellationToken token);

        /// <summary>
        /// エントリリストを取得
        /// </summary>
        public async ValueTask<List<ArchiveEntry>> GetEntriesAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (_entries != null)
            {
                return _entries;
            }

            // NOTE: MTAスレッドで実行。SevenZipSharpのCOM例外対策
            _entries = await Task.Run(async () =>
            {
                return (await GetEntriesInnerAsync(token))
                    .Where(e => !IsExcludedPath(e.EntryName))
                    .ToList();
            });

            return _entries;
        }

        /// <summary>
        /// 除外パス判定
        /// </summary>
        private bool IsExcludedPath(string path)
        {
            return path.Split('/', '\\').Any(e => Config.Current.Book.Excludes.ConainsOrdinalIgnoreCase(e));
        }

        /// <summary>
        /// エントリキャッシュをクリア
        /// </summary>
        public void ClearEntryCache()
        {
            _entries = null;
        }

        /// <summary>
        /// 指定階層のエントリのみ取得
        /// </summary>
        public async Task<List<ArchiveEntry>> GetEntriesAsync(string path, bool isRecursive, CancellationToken token)
        {
            path = LoosePath.TrimDirectoryEnd(path);

            var entries = (await GetEntriesAsync(token))
                .Where(e => path.Length < e.EntryName.Length && e.EntryName.StartsWith(path));

            if (!isRecursive)
            {
                entries = entries.Where(e => LoosePath.Split(e.EntryName[path.Length..]).Length == 1);
            }

            return entries.ToList();
        }

        /// <summary>
        /// エントリーのストリームを取得
        /// </summary>
        public async Task<Stream> OpenStreamAsync(ArchiveEntry entry, CancellationToken token)
        {
            if (entry.Id < 0) throw new ApplicationException("Cannot open this entry: " + entry.EntryName);

            await WaitPreExtractAsync(entry, token);

            if (entry.Data is byte[] rawData)
            {
                return new MemoryStream(rawData, 0, rawData.Length, false, true);
            }
            else if (entry.Data is string fileName)
            {
                return new FileStream(fileName, FileMode.Open, FileAccess.Read);
            }
            else
            {
                return await OpenStreamInnerAsync(entry, token);
            }
        }

        /// <summary>
        /// エントリのストリームを取得 (Inner)
        /// </summary>
        protected abstract Task<Stream> OpenStreamInnerAsync(ArchiveEntry entry, CancellationToken token);

        /// <summary>
        /// エントリーをファイルとして出力
        /// </summary>
        public async Task ExtractToFileAsync(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            if (entry.Id < 0) throw new ApplicationException("Cannot open this entry: " + entry.EntryName);

            await WaitPreExtractAsync(entry, token);

            if (entry.Data is string fileName)
            {
                await FileIO.CopyFileAsync(fileName, exportFileName, isOverwrite, token);
            }
            else if (entry.Data is byte[] rawData)
            {
                FileIO.CheckOverwrite(exportFileName, isOverwrite);
                await File.WriteAllBytesAsync(exportFileName, rawData, token);
            }
            else
            {
                await ExtractToFileInnerAsync(entry, exportFileName, isOverwrite, token);
            }
        }

        /// <summary>
        /// エントリをファイルとして出力 (Inner)
        /// </summary>
        protected abstract Task ExtractToFileInnerAsync(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token);


        /// <summary>
        /// 所属している場所を得る
        /// 多重圧縮フォルダーの場合は最上位のアーカイブの場所になる
        /// </summary>
        /// <returns>ファイルパス</returns>
        public string GetPlace()
        {
            return (Parent == null || Parent is FolderArchive) ? Path : Parent.GetPlace();
        }


        /// <summary>
        /// フォルダーリスト上での親フォルダーを取得
        /// </summary>
        /// <returns></returns>
        public string GetParentPlace()
        {
            if (this.Parent != null)
            {
                return this.Parent.SystemPath;
            }
            else
            {
                return LoosePath.GetDirectoryName(this.SystemPath);
            }
        }

        /// <summary>
        /// エントリ群からディレクトリエントリを生成する
        /// </summary>
        /// <param name="entries">アーカイブのエントリ群</param>
        /// <returns>ディレクトリエントリのリスト</returns>
        protected List<ArchiveEntry> CreateDirectoryEntries(IEnumerable<ArchiveEntry> entries)
        {
            var tree = new ArchiveEntryTree();
            tree.AddRange(entries);

            var directories = tree.GetDirectories()
                .Select(e => e.ArchiveEntry ?? new ArchiveEntry(this)
                {
                    IsValid = true,
                    Id = -1,
                    Instance = null,
                    RawEntryName = e.Path,
                    Length = -1,
                    CreationTime = e.CreationTime,
                    LastWriteTime = e.LastWriteTime,
                })
                .ToList();

            return directories;
        }

        /// <summary>
        /// 事前展開する？
        /// </summary>
        public bool CanPreExtract()
        {
            return _preExtractorActivateCount > 0 && CanPreExtractInner();
        }

        protected virtual bool CanPreExtractInner()
        {
            return false;
        }

        /// <summary>
        /// 事前展開
        /// </summary>
        public virtual async Task PreExtractAsync(string directory, CancellationToken token)
        {
            var entries = await GetEntriesAsync(token);
            foreach (var entry in entries)
            {
                token.ThrowIfCancellationRequested();
                if (entry.IsDirectory) continue;
                var filename = $"{entry.Id:000000}{System.IO.Path.GetExtension(entry.EntryName)}";
                var path = System.IO.Path.Combine(directory, filename);
                await entry.ExtractToFileAsync(path, true, token);
                entry.SetData(path);
            }
        }

        /// <summary>
        /// RawData 開放
        /// </summary>
        /// <remarks>
        /// メモリ上の展開データを開放する。
        /// ファイルに展開したデータはそのまま。
        /// </remarks>
        public void ClearRawData()
        {
            if (_entries is null) return;

            foreach (var entry in _entries)
            {
                if (entry.Data is byte[])
                {
                    entry.ResetData();
                }
            }
        }

        /// <summary>
        /// 事前展開を許可
        /// </summary>
        public void ActivatePreExtractor()
        {
            if (Interlocked.Increment(ref _preExtractorActivateCount) == 1)
            {
                _preExtractor.Resume();
            }
        }

        /// <summary>
        /// 事前展開を停止
        /// </summary>
        public void DeactivatePreExtractor()
        {
            if (Interlocked.Decrement(ref _preExtractorActivateCount) == 0)
            {
                _preExtractor.Sleep();
            }
        }

        /// <summary>
        /// エントリの事前展開完了を待機
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task WaitPreExtractAsync(ArchiveEntry entry, CancellationToken token)
        {
            await _preExtractor.WaitPreExtractAsync(entry, token);
        }

        // エントリー実体のファイルシステム判定
        public virtual bool IsFileSystemEntry(ArchiveEntry entry)
        {
            return IsFileSystem || entry.Link != null;
        }

        /// <summary>
        /// exists?
        /// </summary>
        public virtual bool Exists(ArchiveEntry entry)
        {
            return entry.Archiver == this && !entry.IsDeleted;
        }

        /// <summary>
        /// can delete
        /// </summary>
        public bool CanDelete(ArchiveEntry entry)
        {
            return CanDelete(new List<ArchiveEntry>() { entry });
        }

        /// <summary>
        /// can delete entries
        /// </summary>
        public virtual bool CanDelete(List<ArchiveEntry> entries)
        {
            return false;
        }

        /// <summary>
        /// delete
        /// </summary>
        public async Task<bool> DeleteAsync(ArchiveEntry entry)
        {
            return await DeleteAsync(new List<ArchiveEntry>() { entry });
        }

        /// <summary>
        /// delete entries
        /// </summary>
        public virtual async Task<bool> DeleteAsync(List<ArchiveEntry> entries)
        {
            return await Task.FromResult(false);
        }

        /// <summary>
        /// can rename?
        /// </summary>
        public virtual bool CanRename(ArchiveEntry entry)
        {
            return false;
        }

        /// <summary>
        /// rename
        /// </summary>
        public virtual async Task<bool> RenameAsync(ArchiveEntry entry, string name)
        {
            return await Task.FromResult(false);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }
                _preExtractor.Dispose();
                _disposedValue = true;
            }
        }

        ~Archiver()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}

