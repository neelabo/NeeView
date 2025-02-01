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
        private readonly System.Threading.Lock _lock = new();


        public SevenZipAccessor(string fileName)
        {
            InitializeLibrary();
            _fileName = fileName;
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
                    _extractor = new SevenZipExtractor(_fileName);
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
                if (_disposedValue) return;
                lock (_lock)
                {
                    _extractor?.Dispose();
                    _extractor = null;
                }
            }
        }

#if false
        /// <summary>
        /// アーカイブ初期化 (未使用)
        /// </summary>
        /// <remarks>
        /// 他のアクセス時に自動的に呼ばれるアーカイブ情報収集を明示的に行う。
        /// IArchiveOpenCallback をうまく使えば進捗を取得できる。7zはなぜか取得できない。
        /// </remarks>
        public void GetArchiveInfo()
        {
            using (_asyncLock.Lock())
            {
                if (_disposedValue) return;
                GetExtractor().GetArchiveInfo(); // 未定義。IArchiveOpenCallback を使用する改造を行う場合にまとめて実装
            }
        }
#endif

        /// <summary>
        /// アーカイブ情報をまとめて取得
        /// </summary>
        /// <returns></returns>
        public ArchiveInfo GetArchiveInfo()
        {
            using (_asyncLock.Lock())
            {
                if (_disposedValue) return new();
                var extractor = GetExtractor();
                return new(extractor.IsSolid, extractor.Format.ToString(), extractor.ArchiveFileData);
            }
        }

        /// <summary>
        /// アーカイブエントリを出力する
        /// </summary>
        /// <param name="index">エントリ番号</param>
        /// <param name="extractStream">出力ストリーム</param>
        public void ExtractFile(int index, Stream extractStream)
        {
            using (_asyncLock.Lock())
            {
                if (_disposedValue) return;
                GetExtractor().ExtractFile(index, extractStream);
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
        public async Task PreExtractAsync(string directory, SevenZipFileExtraction fileExtraction, CancellationToken token)
        {
            Debug.Assert(!string.IsNullOrEmpty(directory));

            using (await _asyncLock.LockAsync(token))
            {
                if (_disposedValue) return;
                var preExtractor = new SevenZipHybridExtractor(GetExtractor(), directory, fileExtraction);
                await preExtractor.ExtractAsync(token);
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



    public record ArchiveInfo(bool IsSolid, string Format, ReadOnlyCollection<ArchiveFileInfo> ArchiveFileData)
    {
        public ArchiveInfo() : this(false, "", new List<ArchiveFileInfo>().AsReadOnly()) { }
    }

}
