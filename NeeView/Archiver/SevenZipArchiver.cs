﻿using SevenZip;
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
    /// SevenZipSharp のインスタンス管理。
    /// 頻繁にアクセスされる場合にインスタンスを使い回すようにしてアクセスを高速化する。
    /// </summary>
    public class SevenZipSource : IDisposable
    {
        #region Fields

        private SevenZipExtractor _extractor;
        private string _fileName;
        private Stream _stream;

        #endregion

        #region Constructors

        public SevenZipSource(string fileName, object lockObject)
        {
            _fileName = fileName;
            Initialize();
        }

        public SevenZipSource(Stream stream, object lockObject)
        {
            _stream = stream;
            Initialize();
        }

        #endregion

        #region Properties

        public bool IsStream => _stream != null;

        #endregion

        #region Methods

        private void Initialize()
        {
        }

        public SevenZipExtractor Open()
        {
            if (_extractor == null)
            {
                _extractor = IsStream ? new SevenZipExtractor(_stream) : new SevenZipExtractor(_fileName);
            }

            return _extractor;
        }

        public void Close(bool isForce = false)
        {
            if (_extractor == null) return;

            if (isForce)
            {
                _extractor.Dispose();
                _extractor = null;
            }
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                Close(true);
                _stream = null;

                disposedValue = true;
            }
        }

        ~SevenZipSource()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }


    /// <summary>
    /// SevenZipSharpのインスタンスアクセサ
    /// </summary>
    public class SevenZipDescriptor : IDisposable
    {
        private SevenZipSource _source;
        private SevenZipExtractor _extractor;

        public SevenZipDescriptor(SevenZipSource source)
        {
            _source = source;
            _extractor = _source.Open();
        }

        public ReadOnlyCollection<ArchiveFileInfo> ArchiveFileData
        {
            get { return _extractor.ArchiveFileData; }
        }

        public void ExtractFile(int index, Stream extractStream)
        {
            _extractor.ExtractFile(index, extractStream);
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _source.Close();
                    _extractor = null;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }


    /// <summary>
    /// アーカイバー：7z.dll
    /// </summary>
    public class SevenZipArchiver : Archiver
    {
        #region Statics

        private static object _staticLock = new object();

        private static bool _isLibraryInitialized;

        public static void InitializeLibrary()
        {
            if (_isLibraryInitialized) return;

            string dllPath = Config.IsX64 ? SevenZipArchiverProfile.Current.X64DllPath : SevenZipArchiverProfile.Current.X86DllPath;
            if (string.IsNullOrWhiteSpace(dllPath))
            {
                dllPath = System.IO.Path.Combine(Config.Current.LibrariesPlatformPath, "7z.dll");
            }

            SevenZipExtractor.SetLibraryPath(dllPath);

            FileVersionInfo dllVersionInfo = FileVersionInfo.GetVersionInfo(dllPath);
            Debug.WriteLine("7z.dll: ver" + dllVersionInfo?.FileVersion);

            _isLibraryInitialized = true;
        }

        #endregion

        #region Fields

        private object _lock = new object();
        private SevenZipSource _source;
        private Stream _stream;

        #endregion

        #region Constructors

        public SevenZipArchiver(string path, ArchiveEntry source, bool isRoot) : base(path, source, isRoot)
        {
            InitializeLibrary();

            _source = new SevenZipSource(Path, _lock);
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return "7-Zip";
        }

        public override void Unlock()
        {
            // 直接の圧縮ファイルである場合のみアンロック
            if (this.Parent == null || this.Parent is FolderArchive)
            {
                lock (_lock)
                {
                    _source?.Close(true);
                }
            }
        }


        // サポート判定
        public override bool IsSupported()
        {
            return true;
        }

        // エントリーリストを得る
        public override List<ArchiveEntry> GetEntries(CancellationToken token)
        {
            if (_disposedValue) throw new ApplicationException("Archive already colosed.");

            token.ThrowIfCancellationRequested();

            var list = new List<ArchiveEntry>();
            var directoryEntries = new List<ArchiveEntry>();

            lock (_lock)
            {
                if (_source == null) throw new ApplicationException("Archive already colosed.");

                using (var extractor = new SevenZipDescriptor(_source))
                {
                    ReadOnlyCollection<ArchiveFileInfo> entries;

                    // NOTE: 異なるスレッドで処理するととても重くなることがあるので排他処理にする
                    lock (_staticLock)
                    {
                        entries = extractor.ArchiveFileData;
                    }

                    for (int id = 0; id < entries.Count; ++id)
                    {
                        token.ThrowIfCancellationRequested();

                        var entry = entries[id];

                        var archiveEntry = new ArchiveEntry()
                        {
                            Archiver = this,
                            Id = id,
                            RawEntryName = entry.FileName,
                            Length = (long)entry.Size,
                            LastWriteTime = entry.LastWriteTime,
                        };

                        if (!entry.IsDirectory)
                        {
                            list.Add(archiveEntry);
                        }
                        else
                        {
                            archiveEntry.Length = -1;
                            directoryEntries.Add(archiveEntry);
                        }
                    }

                    // 空ディレクトリー追加
                    if (BookProfile.Current.IsEnableNoSupportFile)
                    {
                        list.AddDirectoryEntries(directoryEntries);
                    }
                }
            }

            return list;
        }


        // エントリーのストリームを得る
        public override Stream OpenStream(ArchiveEntry entry)
        {
            if (_disposedValue) throw new ApplicationException("Archive already colosed.");

            lock (_lock)
            {
                if (_source == null) throw new ApplicationException("Archive already colosed.");

                using (var extractor = new SevenZipDescriptor(_source))
                {
                    var archiveEntry = extractor.ArchiveFileData[entry.Id];
                    if (archiveEntry.FileName != entry.RawEntryName)
                    {
                        throw new ApplicationException(Properties.Resources.ExceptionInconsistency);
                    }

                    var ms = new MemoryStream();
                    extractor.ExtractFile(entry.Id, ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    return ms;
                }
            }
        }


        // ファイルに出力
        public override void ExtractToFile(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            if (_disposedValue) throw new ApplicationException("Archive already colosed.");

            lock (_lock)
            {
                using (var extractor = new SevenZipExtractor(Path)) // 専用extractor
                using (Stream fs = new FileStream(exportFileName, FileMode.Create, FileAccess.Write))
                {
                    extractor.ExtractFile(entry.Id, fs);
                }
            }
        }

        #endregion

        #region IDisposable Support

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    lock (_lock)
                    {
                        if (_source != null)
                        {
                            _source.Dispose();
                            _source = null;
                        }
                        if (_stream != null)
                        {
                            _stream.Dispose();
                            _stream = null;
                        }
                    }
                }
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
