using NeeLaboratory.Threading;
using SevenZip;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{

    /// <summary>
    /// SevenZipSharpのインスタンスアクセサ。
    /// 一時的にファイルロックを解除できるようにしている。
    /// </summary>
    public class SevenZipAccessor : IDisposable
    {
        private static bool _isLibraryInitialized;

        // NOTE: 複数のアーカイブに同時にアクセスすると処理が極端に落ち込むようなので、並列アクセスを制限してみる
        private static readonly AsyncLock _asyncLock = new();

        public static void InitializeLibrary()
        {
            if (_isLibraryInitialized) return;

            string dllPath = Environment.IsX64 ? Config.Current.Archive.SevenZip.X64DllPath : Config.Current.Archive.SevenZip.X86DllPath;
            if (string.IsNullOrWhiteSpace(dllPath))
            {
                dllPath = System.IO.Path.Combine(Environment.LibrariesPlatformPath, "7z.dll");
            }

            SevenZipExtractor.SetLibraryPath(dllPath);

            FileVersionInfo dllVersionInfo = FileVersionInfo.GetVersionInfo(dllPath);
            Debug.WriteLine("7z.dll: ver" + dllVersionInfo?.FileVersion);

            _isLibraryInitialized = true;
        }


        private readonly string _fileName;
        private SevenZipExtractor? _extractor;
        private readonly Lock _lock = new();
        private readonly ArchiveKey _archiveKey;


        public SevenZipAccessor(string fileName)
        {
            InitializeLibrary();
            _fileName = fileName;

            _archiveKey = new ArchiveKey(fileName);
            _archiveKey.KeyChanged += (s, e) => UnlockCore();
        }


        public string Format
        {
            get
            {
                using (_asyncLock.Lock())
                {
                    if (_disposedValue) return "";
                    return GetExtractor().Format.ToString();
                }
            }
        }

        public bool IsSolid
        {
            get
            {
                using (_asyncLock.Lock())
                {
                    if (_disposedValue) return false;
                    return GetExtractor().IsSolid;
                }
            }
        }


        public ReadOnlyCollection<ArchiveFileInfo> ArchiveFileData
        {
            get
            {
                using (_asyncLock.Lock())
                {
                    if (_disposedValue) return new List<ArchiveFileInfo>().AsReadOnly();
                    // TODO: 重い処理。キャンセルできるようにしたい
                    return GetExtractor().ArchiveFileData;
                }
            }
        }


        /// <summary>
        /// エクストラクタの取得
        /// </summary>
        private SevenZipExtractor GetExtractor()
        {
            lock (_lock)
            {
                ThrowIfDisposed();

                if (_extractor is null)
                {
                    if (string.IsNullOrEmpty(_archiveKey.Key))
                    {
                        _extractor = new SevenZipExtractor(_fileName);
                    }
                    else
                    {
                        _extractor = new SevenZipExtractor(_fileName, _archiveKey.Key);
                    }
                }

                return _extractor;
            }
        }

        /// <summary>
        /// エクストラクタを開放し、ファイルをアンロックする
        /// </summary>
        public void Unlock()
        {
            using (_asyncLock.Lock())
            {
                UnlockCore();
            }
        }

        private void UnlockCore()
        {
            if (_disposedValue) return;
            lock (_lock)
            {
                _extractor?.Dispose();
                _extractor = null;
            }
        }

        /// <summary>
        /// アーカイブ情報をまとめて取得
        /// </summary>
        /// <returns></returns>
        public (SevenZipArchiveInfo Info, ReadOnlyCollection<ArchiveFileInfo> Entries) GetArchiveInfo(bool decrypt)
        {
            using (_asyncLock.Lock())
            {
                if (_disposedValue) return new();
                SevenZipExtractor? extractor = null;

                while (true)
                {
                    try
                    {
                        extractor = GetExtractor();
                        return new(new SevenZipArchiveInfo(extractor.Format.ToString(), extractor.IsSolid, extractor.PasswordRequired), extractor.ArchiveFileData);
                    }
                    catch (SevenZipArchiveException) when (decrypt)
                    {
                        if (extractor is not null && extractor.PasswordRequired)
                        {
                            if (_archiveKey.UpdateArchiveKeyByUser())
                            {
                                continue;
                            }
                        }

                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// アーカイブエントリを出力する
        /// </summary>
        /// <param name="index">エントリ番号</param>
        /// <param name="extractStream">出力ストリーム</param>
        /// <param name="decrypt">暗号解除</param>
        public void ExtractFile(SevenZipFileInfo info, Stream extractStream, bool decrypt)
        {
            using (_asyncLock.Lock())
            {
                if (_disposedValue) return;

                while (true)
                {
                    try
                    {
                        GetExtractor().ExtractFile(info.Index, extractStream);
                        break;
                    }
                    catch (ExtractionFailedException ex) when (decrypt && (ex.OperationResult == OperationResult.DataError || ex.OperationResult == OperationResult.WrongPassword))
                    {
                        if (!info.Encrypted)
                        {
                            throw;
                        }

                        if (_archiveKey.UpdateArchiveKeyByUser())
                        {
                            continue;
                        }

                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// すべてのアーカイブエントリを出力する
        /// </summary>
        /// <remarks>
        /// ソリッドアーカイブ用。
        /// すべてのエントリを連続処理し、コールバックで各エントリの出力を制御する。
        /// </remarks>
        /// <param name="directory">ファイルの出力フォルダー</param>
        /// <param name="fileExtraction">エントリの出力定義</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async ValueTask PreExtractAsync(bool encryptedMaybe, string directory, SevenZipFileExtraction fileExtraction, bool decrypt, CancellationToken token)
        {
            Debug.Assert(!string.IsNullOrEmpty(directory));

            using (await _asyncLock.LockAsync(token))
            {
                if (_disposedValue) return;

                while (true)
                {
                    try
                    {
                        var preExtractor = new SevenZipHybridExtractor(GetExtractor(), directory, fileExtraction);
                        await preExtractor.ExtractAsync(token);
                        break;
                    }
                    catch (ExtractionFailedException ex) when (decrypt && (ex.OperationResult == OperationResult.DataError || ex.OperationResult == OperationResult.WrongPassword))
                    {
                        if (!encryptedMaybe)
                        {
                            throw;
                        }

                        if (_archiveKey.UpdateArchiveKeyByUser())
                        {
                            continue;
                        }

                        throw;
                    }
                }
            }
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (_lock)
            {
                if (!_disposedValue)
                {
                    if (disposing)
                    {
                    }

                    // NOTE: ファイルロックを解除する
                    _extractor?.Dispose();
                    _extractor = null;

                    _disposedValue = true;
                }
            }
        }

        ~SevenZipAccessor()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }


    public readonly record struct SevenZipArchiveInfo(string? Format, bool IsSolid, bool Encrypted);

    public readonly record struct SevenZipFileInfo(int Index, bool Encrypted);

    public static partial class ArchiveEntryExtensions
    {
        public static SevenZipFileInfo ToSevenZipFileInfo(this ArchiveEntry entry)
        {
            return new SevenZipFileInfo(entry.Id, entry.Encrypted);
        }
    }
}
