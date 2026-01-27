using NeeLaboratory.ComponentModel;
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
    /// アーカイバーマネージャ
    /// </summary>
    public class ArchiveManager : BindableBase, IDisposable
    {
        static ArchiveManager() => Current = new ArchiveManager();
        public static ArchiveManager Current { get; }


        /// <summary>
        /// アーカイバのサポート拡張子
        /// </summary>
        private readonly Dictionary<ArchiveType, FileTypeCollection> _supportedFileTypes = new()
        {
            [ArchiveType.SevenZipArchive] = Config.Current.Archive.SevenZip.SupportFileTypes,
            [ArchiveType.ZipArchive] = Config.Current.Archive.Zip.SupportFileTypes,
            [ArchiveType.PdfArchive] = Config.Current.Archive.Pdf.SupportFileTypes,
            [ArchiveType.MediaArchive] = Config.Current.Archive.Media.SupportFileTypes,
            [ArchiveType.SusieArchive] = SusiePluginManager.Current.ArchiveExtensions,
            [ArchiveType.PlaylistArchive] = new FileTypeCollection(PlaylistArchive.Extension),
        };

        // アーカイバの適用順
        private List<ArchiveType> _orderList;
        private bool _isDirtyOrderList;

        private readonly DisposableCollection _disposables;
        private readonly ArchiveCache _cache;


        private ArchiveManager()
        {
            _disposables = new DisposableCollection();

            _cache = new ArchiveCache();
            _disposables.Add(_cache);

            _disposables.Add(Config.Current.Archive.Zip.SubscribePropertyChanged(
                (s, e) => _cache.Clear()));
            _disposables.Add(Config.Current.Archive.SevenZip.SubscribePropertyChanged(
                (s, e) => _cache.Clear()));
            _disposables.Add(Config.Current.Archive.Pdf.SubscribePropertyChanged(
                (s, e) => _cache.Clear()));
            _disposables.Add(Config.Current.Archive.Media.SubscribePropertyChanged(
                (s, e) => _cache.Clear()));
            _disposables.Add(Config.Current.Susie.SubscribePropertyChanged(
                (s, e) => _cache.Clear()));

            _disposables.Add(Config.Current.Archive.Zip.SubscribePropertyChanged(nameof(ZipArchiveConfig.IsEnabled),
                (s, e) => UpdateOrderList()));
            _disposables.Add(Config.Current.Archive.SevenZip.SubscribePropertyChanged(nameof(SevenZipArchiveConfig.IsEnabled),
                (s, e) => UpdateOrderList()));
            _disposables.Add(Config.Current.Archive.Pdf.SubscribePropertyChanged(nameof(PdfArchiveConfig.IsEnabled),
                (s, e) => UpdateOrderList()));
            _disposables.Add(Config.Current.Archive.Media.SubscribePropertyChanged(nameof(MediaArchiveConfig.IsEnabled),
                (s, e) => UpdateOrderList()));
            _disposables.Add(Config.Current.Susie.SubscribePropertyChanged(nameof(SusieConfig.IsEnabled),
                (s, e) => UpdateOrderList()));
            _disposables.Add(Config.Current.Susie.SubscribePropertyChanged(nameof(SusieConfig.IsFirstOrderSusieArchive),
                (s, e) => UpdateOrderList()));

            // 検索順初期化
            _orderList = CreateOrderList();

            ApplicationDisposer.Current.Add(this);
        }


        // 対応アーカイブ検索用リスト
        private List<ArchiveType> OrderList
        {
            get
            {
                if (_isDirtyOrderList)
                {
                    _orderList = CreateOrderList();
                    _isDirtyOrderList = false;
                }

                return _orderList;
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
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion


        private void UpdateOrderList()
        {
            _isDirtyOrderList = true;
        }

        // 検索順を更新
        private static List<ArchiveType> CreateOrderList()
        {
            var order = new List<ArchiveType>
            {
                ArchiveType.PlaylistArchive
            };

            if (Config.Current.Archive.Zip.IsEnabled)
            {
                order.Add(ArchiveType.ZipArchive);
            }

            if (Config.Current.Archive.SevenZip.IsEnabled)
            {
                order.Add(ArchiveType.SevenZipArchive);
            }

            if (Config.Current.Archive.Pdf.IsEnabled)
            {
                order.Add(ArchiveType.PdfArchive);
            }

            if (Config.Current.Archive.Media.IsEnabled)
            {
                order.Add(ArchiveType.MediaArchive);
            }

            if (Config.Current.Susie.IsEnabled)
            {
                if (Config.Current.Susie.IsFirstOrderSusieArchive)
                {
                    order.Insert(0, ArchiveType.SusieArchive);
                }
                else
                {
                    order.Add(ArchiveType.SusieArchive);
                }
            }

            return order;
        }

        public IEnumerable<string> GetFileTypes(bool includeMedia)
        {
            if (Config.Current.Archive.Zip.IsEnabled)
            {
                foreach (var ext in _supportedFileTypes[ArchiveType.ZipArchive].Items)
                {
                    yield return ext;
                }
            }

            if (Config.Current.Archive.SevenZip.IsEnabled)
            {
                foreach (var ext in _supportedFileTypes[ArchiveType.SevenZipArchive].Items)
                {
                    yield return ext;
                }
            }

            if (Config.Current.Archive.Pdf.IsEnabled)
            {
                foreach (var ext in _supportedFileTypes[ArchiveType.PdfArchive].Items)
                {
                    yield return ext;
                }
            }

            if (Config.Current.Archive.Media.IsEnabled && includeMedia)
            {
                foreach (var ext in _supportedFileTypes[ArchiveType.MediaArchive].Items)
                {
                    yield return ext;
                }
            }

            if (Config.Current.Susie.IsEnabled)
            {
                foreach (var ext in _supportedFileTypes[ArchiveType.SusieArchive].Items)
                {
                    yield return ext;
                }
            }

            foreach (var ext in _supportedFileTypes[ArchiveType.PlaylistArchive].Items)
            {
                yield return ext;
            }
        }

        // アーカイバを指定してサポートしているかを判定
        public bool IsSupported(string fileName, ArchiveType archiverType)
        {
            string ext = LoosePath.GetExtension(fileName);
            return _supportedFileTypes[archiverType].Contains(ext);
        }

        // サポートしているアーカイバーがあるか判定
        public bool IsSupported(string fileName, bool isAllowFileSystem = true, bool isAllowMedia = true)
        {
            if (_disposedValue) return false;

            return GetSupportedType(fileName, isAllowFileSystem, isAllowMedia) != ArchiveType.None;
        }

        // サポートしているアーカイバーを取得
        public ArchiveType GetSupportedType(string fileName, bool isArrowFileSystem = true, bool isAllowMedia = true)
        {
            return GetSupportedType(fileName, ArchiveHint.None, isArrowFileSystem, isAllowMedia).ArchiveType;
        }

        /// <summary>
        /// サポートしているアーカイバーを取得
        /// </summary>
        /// <remarks>
        /// ファイル名からアーカイバーを判定する。実在を問わない
        /// </remarks>
        public ArchiverIdentifier GetSupportedType(string fileName, ArchiveHint archiveHint, bool isArrowFileSystem = true, bool isAllowMedia = true)
        {
            if (_disposedValue) return ArchiverIdentifier.None;
            if (string.IsNullOrEmpty(fileName)) return ArchiverIdentifier.None;

            if (isArrowFileSystem && (fileName.Last() == '\\' || fileName.Last() == '/'))
            {
                return ArchiverIdentifier.FolderArchive;
            }

            string ext = LoosePath.GetExtension(fileName);

            // 優先アーカイブを検証
            if (archiveHint != ArchiveHint.None)
            {
                var archiver = archiveHint.Archiver;
                if (_supportedFileTypes[archiver.ArchiveType].Contains(ext))
                {
                    switch (archiver.ArchiveType)
                    {
                        case ArchiveType.MediaArchive:
                            if (isAllowMedia)
                            {
                                return ArchiverIdentifier.MediaArchive;
                            }
                            break;
                        case ArchiveType.SusieArchive:
                            if (archiver.PluginName is not null && SusieArchive.IsSupportedPlugin(ext, archiver.PluginName))
                            {
                                return new ArchiverIdentifier(ArchiveType.SusieArchive, archiver.PluginName);
                            }
                            break;
                        default:
                            return new ArchiverIdentifier(archiver.ArchiveType);
                    }
                }
            }

            // 優先リスト順に検索
            foreach (var type in this.OrderList)
            {
                if (_supportedFileTypes[type].Contains(ext))
                {
                    return (isAllowMedia || type != ArchiveType.MediaArchive) ? new ArchiverIdentifier(type) : ArchiverIdentifier.None;
                }
            }

            return ArchiverIdentifier.None;
        }

        /// <summary>
        /// サポートしているアーカイバーリストを取得
        /// </summary>
        /// <remarks>
        /// 実在ファイルに対してのみ判定を行う。
        /// </remarks>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public List<ArchiverIdentifier> GetSupportedArchiverList(string fileName)
        {
            if (_disposedValue) return [];
            if (string.IsNullOrEmpty(fileName)) return [];

            if (Directory.Exists(fileName))
            {
                return [new ArchiverIdentifier(ArchiveType.FolderArchive)];
            }

            if (!File.Exists(fileName))
            {
                return [];
            }

            var list = new List<ArchiverIdentifier>();
            string ext = LoosePath.GetExtension(fileName);

            foreach (var type in this.OrderList)
            {
                if (_supportedFileTypes[type].Contains(ext))
                {
                    switch (type)
                    {
                        case ArchiveType.MediaArchive:
                            // TODO: MediaPlayer もしくは VlcPlayer を選択可能にする？
                            list.Add(ArchiverIdentifier.MediaArchive);
                            break;
                        case ArchiveType.SusieArchive:
                            foreach (var plugin in SusieArchive.GetSupportedPluginList(ext))
                            {
                                list.Add(new ArchiverIdentifier(ArchiveType.SusieArchive, plugin));
                            }
                            break;
                        default:
                            list.Add(new ArchiverIdentifier(type));
                            break;
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// アーカイバー作成
        /// stream に null 以外を指定すると、そのストリームを使用してアーカイブを開きます。
        /// この stream はアーカイブ廃棄時に Dispose されます。
        /// </summary>
        /// <param name="type">アーカイブの種類</param>
        /// <param name="path">アーカイブファイルのパス</param>
        /// <param name="source">元となったアーカイブエントリ</param>
        /// <param name="isRoot">ルートアーカイブとする</param>
        /// <returns>作成されたアーカイバー</returns>
        private Archive CreateArchive(ArchiveType type, string path, ArchiveEntry? source, ArchiveHint archiveHint)
        {
            return CreateArchive(new ArchiverIdentifier(type), path, source, archiveHint);
        }

        private Archive CreateArchive(ArchiverIdentifier identifier, string path, ArchiveEntry? source, ArchiveHint archiveHint)
        {
            Archive archive;

            switch (identifier.ArchiveType)
            {
                case ArchiveType.FolderArchive:
                    archive = new FolderArchive(path, source, archiveHint);
                    break;
                case ArchiveType.ZipArchive:
                    archive = new ZipArchive(path, source, archiveHint);
                    break;
                case ArchiveType.SevenZipArchive:
                    archive = new SevenZipArchive(path, source, archiveHint);
                    break;
                case ArchiveType.PdfArchive:
                    archive = PdfArchiveFactory.Create(path, source, archiveHint);
                    break;
                case ArchiveType.MediaArchive:
                    archive = new MediaArchive(path, source, archiveHint);
                    break;
                case ArchiveType.SusieArchive:
                    archive = new SusieArchive(path, source, archiveHint);
                    break;
                case ArchiveType.PlaylistArchive:
                    archive = new PlaylistArchive(path, source, archiveHint);
                    break;
                default:
                    ////throw new ArgumentException("Not support archive type.");
                    string extension = LoosePath.GetExtension(path);
                    throw new NotSupportedFileTypeException(extension);
            }

            _cache.Add(archive);

            return archive;
        }

        // アーカイバー作成
        private Archive CreateArchive(string path, ArchiveEntry? source, ArchiveHint archiveHint)
        {
            // TODO: source が null のときに archiveHint が archive に記録されない
            if (Directory.Exists(path))
            {
                return CreateArchive(ArchiveType.FolderArchive, path, source, archiveHint);
            }
            else
            {
                return CreateArchive(GetSupportedType(path, archiveHint), path, source, archiveHint);
            }
        }

        /// <summary>
        /// アーカイバ作成。
        /// テンポラリファイルへの展開が必要になることもあるので非同期
        /// </summary>
        /// <param name="source">ArchiveEntry</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async ValueTask<Archive> CreateArchiveAsync(ArchiveEntry source, bool ignoreCache, CancellationToken token)
        {
            ThrowIfDisposed();

            // キャッシュがあればそれを返す。
            var targetPath = source.TargetPath;
            if (!ignoreCache && _cache.TryGetValue(targetPath, out var archive))
            {
                // 更新日、サイズ、アーカイブヒントを比較して再利用するかを判定
                if (archive is not null && archive.LastWriteTime == source.LastWriteTime && archive.Length == source.Length && archive.ArchiveHint == source.ArchiveHint)
                {
                    ////Debug.WriteLine($"Archive: Find cache: {targetPath}");
                    return archive;
                }
                else
                {
                    //// Debug.WriteLine($"Archive: Old cache: {targetPath}");
                }
            }
            else
            {
                if (ignoreCache)
                {
                    ////Debug.WriteLine($"Archive: Ignore cache: {targetPath}");
                }
                else
                {
                    ////Debug.WriteLine($"Archive: Cache not found: {targetPath}");
                }
            }

            if (source.IsFileSystem)
            {
                return CreateArchive(targetPath, null, source.ArchiveHint);
            }
            else
            {
                // TODO: テンポラリファイルの指定方法をスマートに。
                var proxyFile = await ArchiveEntryExtractorService.Current.ExtractAsync(source, token);
                var archiverTemp = CreateArchive(proxyFile.Path, source, source.ArchiveHint);
                ////Debug.WriteLine($"Archive: {archiverTemp.SystemPath} => {tempFile.Path}");
                Debug.Assert(archiverTemp.ProxyFile == null);
                archiverTemp.ProxyFile = proxyFile;
                return archiverTemp;
            }
        }


        /// <summary>
        /// パスが実在するアーカイブであるかを判定
        /// </summary>
        /// 
        public bool Exists(string path, bool isAllowFileSystem)
        {
            if (_disposedValue) return false;

            if (isAllowFileSystem)
            {
                return Directory.Exists(path) || (File.Exists(path) && IsSupported(path, true));
            }
            else
            {
                return File.Exists(path) && IsSupported(path, false);
            }
        }

        /// <summary>
        /// アーカイブパスからファイルシステムに実在するアーカイブファイルのパスを取得
        /// ex: C:\foo.zip\sub\test.txt -> C:\foo.zip
        /// </summary>
        /// <param name="path">アーカイブパス</param>
        /// <returns>実在するアーカイブファイルのパス。見つからなかった場合は null</returns>
        public string? GetExistPathName(string path)
        {
            if (_disposedValue) return null;

            if (Exists(path, true))
            {
                return path;
            }

            while (true)
            {
                path = LoosePath.GetDirectoryName(path);
                if (string.IsNullOrEmpty(path) || Directory.Exists(path))
                {
                    break;
                }

                if (Exists(path, false))
                {
                    return path;
                }
            }

            return null;
        }

        public static ArchiveType GetArchiveType(Archive archiver)
        {
            if (archiver is null) throw new ArgumentNullException(nameof(archiver));
            return archiver switch
            {
                FolderArchive => ArchiveType.FolderArchive,
                ZipArchive => ArchiveType.ZipArchive,
                SevenZipArchive => ArchiveType.SevenZipArchive,
                PdfArchive => ArchiveType.PdfArchive,
                MediaArchive => ArchiveType.MediaArchive,
                SusieArchive => ArchiveType.SusieArchive,
                PlaylistArchive => ArchiveType.PlaylistArchive,
                _ => ArchiveType.None,
            };
        }

        /// <summary>
        /// すべてのアーカイブのファイルロック解除
        /// </summary>
        public async ValueTask UnlockAllArchivesAsync()
        {
            if (_disposedValue) return;

            // NOTE: MTAスレッドで実行。SevenZipSharpのCOM例外対策
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                await Task.Run(() => _cache.Unlock());
            }
            else
            {
                _cache.Unlock();
            }
        }


        [Conditional("DEBUG")]
        public void DumpCache()
        {
            _cache.CleanUp();
            _cache.Dump();
        }
    }
}
