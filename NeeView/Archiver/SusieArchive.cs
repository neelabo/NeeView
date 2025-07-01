using NeeLaboratory.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイバー：Susieアーカイバー
    /// </summary>
    public class SusieArchive : Archive
    {
        private SusieArchivePluginAccessor? _susiePlugin;
        private static readonly AsyncLock _asyncLock = new();


        public SusieArchive(string path, ArchiveEntry? source, ArchiveHint archiveHint) : base(path, source, archiveHint)
        {
        }


        public override string? ToString()
        {
            return _susiePlugin?.Plugin.Name ?? "(none)";
        }

        // サポート判定
        public override bool IsSupported()
        {
            return GetPlugin() != null;
        }

        // 対応プラグイン取得
        public SusieArchivePluginAccessor? GetPlugin()
        {
            if (_susiePlugin == null)
            {
                _susiePlugin = SusiePluginManager.Current.GetArchivePluginAccessor(Path, null, true, GetPluginNameHint());
            }
            return _susiePlugin;
        }

        // プラグイン名取得
        public override string? GetPluginName()
        {
            return _susiePlugin?.Plugin.Name;
        }

        // アーカイブヒントで指定されたプラグイン名を取得
        private string? GetPluginNameHint()
        {
            if (ArchiveHint.Archiver.ArchiveType == ArchiveType.SusieArchive)
            {
                return ArchiveHint.Archiver.PluginName;
            }
            return null;
        }

        // エントリーリストを得る
        protected override async ValueTask<List<ArchiveEntry>> GetEntriesInnerAsync(bool decrypt, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var plugin = GetPlugin() ?? throw new NotSupportedException($"not archive: {Path}");
            var entries = plugin.GetArchiveEntries(Path) ?? throw new NotSupportedException();
            var list = new List<ArchiveEntry>();
            var directories = new List<ArchiveEntry>();

            for (int id = 0; id < entries.Count; ++id)
            {
                token.ThrowIfCancellationRequested();

                var entry = entries[id];

                var archiveEntry = new ArchiveEntry(this)
                {
                    IsValid = true,
                    Id = id,
                    RawEntryName = (entry.Path.TrimEnd('\\', '/') + "\\" + entry.FileName).TrimStart('\\', '/'),
                    Length = entry.FileSize,
                    CreationTime = default,
                    LastWriteTime = entry.TimeStamp,
                    Instance = entry,
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

            // NOTE: サイズ0であり、他のエントリ名のパスを含む場合はディレクトリとみなし除外する。
            list = list.Where(entry => entry.Length > 0 || list.All(e => e == entry || !e.EntryName.StartsWith(LoosePath.TrimDirectoryEnd(entry.EntryName), StringComparison.Ordinal))).ToList();

            // ディレクトリエントリを追加
            list.AddRange(CreateDirectoryEntries(list.Concat(directories)));

            await ReadZoneIdentifierAsync(token);

            return list;
        }


        // エントリーのストリームを得る
        protected override async ValueTask<Stream> OpenStreamInnerAsync(ArchiveEntry entry, bool decrypt, CancellationToken token)
        {
            if (entry.Id < 0) throw new ApplicationException("Cannot open this entry: " + entry.EntryName);

            using (await _asyncLock.LockAsync(token))
            {
                var buffer = ExtractToMemoryCore(entry);
                return new MemoryStream(buffer, 0, buffer.Length, false, true);
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
            var extractor = new SusieArchiveExtractor(this);
            await extractor.ExtractAsync(entry, exportFileName, isOverwrite, token);
        }

        /// <summary>
        /// エントリをプラグインからメモリに直接展開
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException"></exception>
        /// <exception cref="SusieException"></exception>
        private byte[] ExtractToMemoryCore(ArchiveEntry entry)
        {
            var info = entry.Instance as Susie.SusieArchiveEntry ?? throw new InvalidCastException();
            var plugin = GetPlugin() ?? throw new Susie.SusieException("Cannot found archive plugin");

            return plugin.ExtractArchiveEntry(Path, info.Position);
        }

        /// <summary>
        /// エントリをプラグインからファイルに直接展開
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="extractFileName">出力ファイル名</param>
        /// <param name="isOverwrite">上書き許可</param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException"></exception>
        /// <exception cref="SusieException"></exception>
        private string ExtractToFileCore(ArchiveEntry entry, string extractFileName, bool isOverwrite)
        {
            var info = entry.Instance as Susie.SusieArchiveEntry ?? throw new InvalidCastException();
            var plugin = GetPlugin() ?? throw new Susie.SusieException("Cannot found archive plugin");

            // NOTE: プラグインからのファイル出力は名前を指定できない
            using (var tempDirectory = new SusieExtractDirectory())
            {
                // 注意：失敗することがよくある。ファイル展開のAPIが実装されていない
                plugin.ExtractArchiveEntryToFolder(Path, info.Position, tempDirectory.Path);

                // 上書き時は移動前に削除
                FileIO.ReadyOverwrite(extractFileName, isOverwrite);

                // 出力ファイル名にかかわらずファイル移動を行う
                var files = Directory.GetFiles(tempDirectory.Path);
                File.Move(files[0], extractFileName);

                return extractFileName;
            }
        }

        /// <summary>
        /// エントリをファイルに展開。必要であればメモリ展開を経由する。
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="extractFileName"></param>
        /// <param name="isOverwrite"></param>
        /// <returns></returns>
        private string ExtractToFile(ArchiveEntry entry, string extractFileName, bool isOverwrite)
        {
            // 16MB以上のエントリは直接ファイル出力を試みる
            if (entry.Length > 16 * 1024 * 1024)
            {
                try
                {
                    ExtractToFileCore(entry, extractFileName, isOverwrite);
                    WriteZoneIdentifier(extractFileName);
                    return extractFileName;
                }
                catch (Susie.SusieException e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            // メモリ展開からのファイル保存
            var buffer = ExtractToMemoryCore(entry);
            using (var ms = new System.IO.MemoryStream(buffer, false))
            using (var stream = new System.IO.FileStream(extractFileName, System.IO.FileMode.Create))
            {
                ms.WriteTo(stream);
            }
            WriteZoneIdentifier(extractFileName);
            return extractFileName;
        }

        /// <summary>
        /// エントリをファイルに展開
        /// </summary>
        public async ValueTask ExtractAsync(ArchiveEntry entry, string extractFileName, bool isOverwrite, CancellationToken token)
        {
            if (entry.Id < 0) throw new ApplicationException("Cannot open this entry: " + entry.EntryName);
            if (entry.IsDirectory) throw new ApplicationException("This entry is directory: " + entry.EntryName);

            using (await _asyncLock.LockAsync(token))
            {
                ExtractToFile(entry, extractFileName, isOverwrite);
            }
        }

        /// <summary>
        /// 事前展開？
        /// </summary>
        protected override bool CanPreExtractInner()
        {
            var plugin = GetPlugin();
            return plugin != null && plugin.Plugin.IsPreExtract;
        }

        /// <summary>
        /// 事前展開
        /// </summary>
        /// <param name="directory">事前展開ファイル用フォルダ</param>
        public override async ValueTask PreExtractAsync(string directory, bool decrypt, CancellationToken token)
        {
            using (await _asyncLock.LockAsync(token))
            {
                var entries = await GetEntriesAsync(decrypt, token);
                foreach (var entry in entries)
                {
                    token.ThrowIfCancellationRequested();

                    if (entry.IsDirectory)
                    {
                        continue;
                    }
                    else if (entry.Length <= 0)
                    {
                        // no extract
                        entry.SetData(Array.Empty<byte>());
                    }
                    else if (PreExtractMemory.Current.IsFull(entry.Length))
                    {
                        // extract to file
                        var extractFileName = GetTempFileName(directory, entry);
                        entry.SetData(ExtractToFile(entry, extractFileName, false));
                    }
                    else
                    {
                        // extract to memory
                        entry.SetData(ExtractToMemoryCore(entry));
                    }
                }
            }
        }

        private static string GetTempFileName(string directory, ArchiveEntry entry)
        {
            var filename = $"{entry.Id:000000}{System.IO.Path.GetExtension(entry.EntryName)}";
            return System.IO.Path.Combine(directory, filename);
        }

        public static List<string> GetSupportedPluginList(string ext)
        {
            return SusiePluginManager.Current.GetSupportedPluginList(Susie.SusiePluginType.Archive, ext);
        }

        public static bool IsSupportedPlugin(string ext, string pluginName)
        {
            return SusiePluginManager.Current.IsSupportedPlugin(Susie.SusiePluginType.Archive, ext, pluginName);
        }
    }


    /// <summary>
    /// Susie アーカイブ一時出力用ディレクトリ
    /// </summary>
    public class SusieExtractDirectory : IDisposable
    {
        private readonly string _path;
        private bool _disposedValue;

        public SusieExtractDirectory()
        {
            _path = Temporary.Current.CreateCountedTempFileName("susie", "");
            Directory.CreateDirectory(_path);
        }

        public string Path => _path;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }

                if (Directory.Exists(_path))
                {
                    Directory.Delete(_path, true);
                }

                _disposedValue = true;
            }
        }

        ~SusieExtractDirectory()
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
