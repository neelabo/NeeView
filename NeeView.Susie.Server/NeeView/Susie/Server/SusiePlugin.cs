using NeeLaboratory.Collections.Specialized;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Susie.Server
{
    /// <summary>
    /// Susie Plugin Accessor
    /// </summary>
    public class SusiePlugin : IDisposable
    {
        private readonly System.Threading.Lock _lock = new();
        private readonly SusiePluginApiCache _apiCache;
        private bool _isCacheEnabled = true;
        private FileExtensionCollection? _userExtensions;
        private bool _propertiesInitialized;
        private bool _isDisposed = false;


        public SusiePlugin(string fileName)
        {
            FileName = fileName;

            _apiCache = new SusiePluginApiCache(this);
            _apiCache.ModuleLoaded += (sender, e) => ReadProperties(e.Module);
        }

        // 有効/無効
        public bool IsEnabled { get; set; } = true;

        // 事前展開。AMプラグインのみ有効
        public bool IsPreExtract { get; set; }

        // プラグインファイルのパス
        public string FileName { get; private set; }

        // プラグイン名
        public string Name { get { return Path.GetFileName(FileName); } }

        // APIバージョン
        public string? ApiVersion { get; private set; }

        // プラグインバージョン
        public string? PluginVersion { get; private set; }

        // 設定ダイアログの有無
        public bool HasConfigurationDlg { get; private set; }

        // プラグインの種類
        public SusiePluginType PluginType => SusiePluginTypeExtensions.FromApiVersion(this.ApiVersion);

        // プラグインDLLをキャッシュする?
        public bool IsCacheEnabled
        {
            get { return _isCacheEnabled; }
            set
            {
                _isCacheEnabled = value;
                if (!_isCacheEnabled)
                {
                    _apiCache.UnloadModule();
                }
            }
        }

        // 標準拡張子
        public FileExtensionCollection? DefaultExtensions { get; set; }

        // ユーザー拡張子
        public FileExtensionCollection? UserExtensions
        {
            get { return _userExtensions; }
            set
            {
                if (value != null && !value.IsEmpty() && !value.Equals(DefaultExtensions))
                {
                    _userExtensions = value;
                }
                else
                {
                    _userExtensions = null;
                }
            }
        }

        // 対応拡張子
        public FileExtensionCollection Extensions
        {
            get { return UserExtensions ?? DefaultExtensions ?? FileExtensionCollection.Empty; }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _apiCache.Dispose();
                }
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // 文字列変換
        public override string ToString()
        {
            return Name ?? "(none)";
        }

        // プロパティ情報が有効か
        private bool IsValidProperties() => ApiVersion is not null;

        /// <summary>
        /// プラグインアクセサ作成
        /// </summary>
        /// <param name="fileName">プラグインファイルのパス</param>
        /// <returns>プラグイン。失敗したら nullを返す</returns>
        public static SusiePlugin? Create(string fileName, SusiePluginInfo? setting)
        {
            var spi = new SusiePlugin(fileName);

            // 設定がある場合はそれを適用、ない場合はプラグインを読み込んで初期化する
            if (setting is not null)
            {
                Debug.Assert(setting.Name == Path.GetFileName(fileName));
                spi.Restore(setting);
                if (spi.IsValidProperties())
                {
                    return spi;
                }
            }

            return spi.Initialize() ? spi : null;
        }

        /// <summary>
        /// 対応拡張子を標準に戻す
        /// </summary>
        public void ResetExtensions()
        {
            UserExtensions = null;
        }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="fileName">プラグインファイルのパス</param>
        /// <returns>成功したら true</returns>
        public bool Initialize()
        {
            using var api = _apiCache.Lock();
            return ReadProperties(api);
        }

        /// <summary>
        /// プラグインプロパティを読み込む
        /// </summary>
        /// <param name="api"></param>
        private bool ReadProperties(ISusiePluginApi api)
        {
            if (_propertiesInitialized) return true;
            _propertiesInitialized = true;

            Trace.WriteLine($"SusiePlugin.ReadProperties: FileName={FileName}");

            try
            {
                ApiVersion = api.GetPluginInfo(0);
                PluginVersion = api.GetPluginInfo(1);

                if (string.IsNullOrEmpty(PluginVersion))
                {
                    PluginVersion = Path.GetFileName(FileName);
                }

                UpdateDefaultExtensions(api);

                HasConfigurationDlg = api.IsExistFunction("ConfigurationDlg");

                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"SusiePlugin.Initialize: FileName={FileName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 標準拡張子の更新
        /// </summary>
        /// <param name="api"></param>
        private void UpdateDefaultExtensions(ISusiePluginApi api)
        {
            var extensions = new List<string>();
            for (int index = 2; ; index += 2)
            {
                // 拡張子フィルター
                var filter = api.GetPluginInfo(index + 0);
                // 拡張子フィルターの説明（未使用）
                var extensionsNote = api.GetPluginInfo(index + 1);

                if (filter == null) break;

                // ifjpeg2k.spi用に区切り記号に","を追加
                // ワイルド拡張子は無効
                extensions.AddRange(filter.Split(';', ',').Select(e => e.TrimStart('*').ToLowerInvariant().Trim()).Where(e => e != ".*"));
            }

            DefaultExtensions = new FileExtensionCollection(extensions);
        }

        /// <summary>
        /// 情報ダイアログを開く
        /// </summary>
        /// <param name="parent">親ウィンドウ</param>
        /// <returns>成功した場合は0</returns>
        public int AboutDlg(IntPtr hwnd)
        {
            if (FileName == null) throw new InvalidOperationException();

            lock (_lock)
            {
                try
                {
                    using var api = _apiCache.Lock();
                    return api.ConfigurationDlg(hwnd, 0);
                }
                finally
                {
                    // 設定変更を確実時反映させるために再読み込みさせる
                    _apiCache.UnloadModule();
                }
            }
        }

        /// <summary>
        /// 設定ダイアログを開く
        /// </summary>
        /// <param name="parent">親ウィンドウ</param>
        /// <returns>成功した場合は0</returns>
        public int ConfigurationDlg(IntPtr hwnd)
        {
            if (FileName == null) throw new InvalidOperationException();

            lock (_lock)
            {
                try
                {
                    using var api = _apiCache.Lock();
                    var result = api.ConfigurationDlg(hwnd, 1);
                    UpdateDefaultExtensions(api);
                    return result;
                }
                finally
                {
                    // 設定変更を確実時反映させるために再読み込みさせる
                    _apiCache.UnloadModule();
                }
            }
        }

        /// <summary>
        /// 設定ダイアログもしくは情報ダイアログを開く
        /// </summary>
        public void OpenConfigurationDialog(IntPtr hWnd)
        {
            int result;
            try
            {
                result = ConfigurationDlg(hWnd);
            }
            catch
            {
                result = -1;
            }

            // 設定ウィンドウが呼び出せなかった場合は情報画面でお茶を濁す
            if (result < 0)
            {
                try
                {
                    AboutDlg(hWnd);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// プラグイン対応判定
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <param name="head">ヘッダ(2KB)</param>
        /// <returns>プラグインが対応していれば true</returns>
        public bool IsSupported(string fileName, byte[] head, bool isCheckExtension)
        {
            if (FileName == null) throw new InvalidOperationException();
            if (!IsEnabled) return false;

            // サポート拡張子チェック
            if (isCheckExtension && !Extensions.Contains(GetExtension(fileName))) return false;

            lock (_lock)
            {
                using var api = _apiCache.Lock();
                string shortPath = GetLegacyPathName(fileName);
                return api.IsSupported(shortPath, head);
            }
        }

        /// <summary>
        /// アーカイブ情報取得
        /// </summary>
        /// <param name="fileName">アーカイブファイル名</param>
        /// <returns></returns>
        public ArchiveEntryCollection GetArchiveEntryCollection(string fileName)
        {
            lock (_lock)
            {
                using var api = _apiCache.Lock();
                string shortPath = GetLegacyPathName(fileName);
                var entries = api.GetArchiveInfo(shortPath);
                if (entries == null) throw new SusieException($"{this.Name}: Failed to read archive information.");
                return new ArchiveEntryCollection(this, fileName, entries);
            }
        }

        /// <summary>
        /// アーカイブ情報取得(IsSupport判定有)
        /// </summary>
        /// <param name="fileName">アーカイブファイル名</param>
        /// <returns>アーカイブ情報。失敗した場合は null</returns>
        public ArchiveEntryCollection? GetArchiveEntryCollection(string fileName, byte[] head)
        {
            if (FileName == null) throw new InvalidOperationException();
            if (!IsEnabled) return null;

            // サポート拡張子チェック
            if (!Extensions.Contains(GetExtension(fileName))) return null;

            lock (_lock)
            {
                using var api = _apiCache.Lock();
                string shortPath = GetLegacyPathName(fileName);
                if (!api.IsSupported(shortPath, head)) return null;
                var entries = api.GetArchiveInfo(shortPath);
                if (entries == null) throw new SusieException($"{this.Name}: Failed to read archive information.");
                return new ArchiveEntryCollection(this, fileName, entries);
            }
        }

        /// <summary>
        /// 画像取得(メモリ版)
        /// </summary>
        /// <param name="fileName">画像ファイル名(サポート判定用)</param>
        /// <param name="buff">画像データ</param>
        /// <param name="isCheckExtension">拡張子をチェックする</param>
        /// <returns>Bitmap。失敗した場合は null</returns>
        public byte[]? GetPicture(string fileName, byte[] buff, bool isCheckExtension)
        {
            if (FileName == null) throw new InvalidOperationException();
            if (!IsEnabled) return null;

            // サポート拡張子チェック
            if (isCheckExtension && !Extensions.Contains(GetExtension(fileName))) return null;

            lock (_lock)
            {
                using var api = _apiCache.Lock();
                // string shortPath = GetLegacyPathName(fileName);
                if (!api.IsSupported(fileName, buff)) return null;
                return api.GetPicture(buff);
            }
        }

        /// <summary>
        /// 画像取得(ファイル版)
        /// </summary>
        /// <param name="fileName">画像ファイルパス</param>
        /// <param name="fileName">ファイルヘッダ2KB</param>
        /// <param name="isCheckExtension">拡張子をチェックする</param>
        /// <returns>Bitmap。失敗した場合は null</returns>
        public byte[]? GetPictureFromFile(string fileName, byte[] head, bool isCheckExtension)
        {
            if (FileName == null) throw new InvalidOperationException();
            if (!IsEnabled) return null;

            // サポート拡張子チェック
            if (isCheckExtension && !Extensions.Contains(GetExtension(fileName))) return null;

            lock (_lock)
            {
                using var api = _apiCache.Lock();
                string shortPath = GetLegacyPathName(fileName);
                if (!api.IsSupported(shortPath, head)) return null;
                return api.GetPicture(shortPath);
            }
        }

        private static string GetExtension(string s)
        {
            return "." + s.Split('.').Last().ToLowerInvariant();
        }

        /// <summary>
        /// アーカイブエントリをメモリにロード
        /// </summary>
        public byte[] LoadArchiveEntry(string archiveFileName, ArchiveFileInfoRaw info)
        {
            return LoadArchiveEntry(archiveFileName, (int)info.position);
        }

        public byte[] LoadArchiveEntry(string archiveFileName, int position)
        {
            if (_isDisposed)
            {
                throw new SusieException("Susie plugin already disposed", this.Name);
            }

            lock (_lock)
            {
                using var api = _apiCache.Lock();
                var buff = api.GetFile(archiveFileName, position);
                if (buff == null) throw new SusieException("Susie extraction failed (Type.M)", this.Name);
                return buff;
            }
        }

        /// <summary>
        /// アーカイブエントリをフォルダーに出力
        /// </summary>
        /// <param name="extractFolder"></param>
        public void ExtractArchiveEntryToFolder(string archiveFileName, ArchiveFileInfoRaw info, string extractFolder)
        {
            ExtractArchiveEntryToFolder(archiveFileName, (int)info.position, extractFolder);
        }

        public void ExtractArchiveEntryToFolder(string archiveFileName, int position, string extractFolder)
        {
            if (_isDisposed)
            {
                throw new SusieException("Susie plugin already disposed", this.Name);
            }

            lock (_lock)
            {
                using var api = _apiCache.Lock();
                int ret = api.GetFile(archiveFileName, position, extractFolder);
                if (ret != 0) throw new SusieException("Susie extraction failed (Type.F)", this.Name);
            }
        }

        public void Restore(SusiePluginInfo info)
        {
            if (info == null) return;

            if (!_propertiesInitialized)
            {
                this.ApiVersion = info.ApiVersion;
                this.PluginVersion = info.PluginVersion;
                this.HasConfigurationDlg = info.HasConfigurationDlg;
                this.DefaultExtensions = info.DefaultExtensions?.Clone();
            }

            this.IsEnabled = info.IsEnabled;
            this.IsCacheEnabled = info.IsCacheEnabled;
            this.IsPreExtract = info.IsPreExtract;
            this.UserExtensions = info.UserExtensions?.Clone();
        }

        public SusiePluginInfo ToSusiePluginInfo()
        {
            var info = new SusiePluginInfo(this.Name);
            info.ApiVersion = this.ApiVersion;
            info.PluginVersion = this.PluginVersion;
            info.HasConfigurationDlg = this.HasConfigurationDlg;
            info.IsEnabled = this.IsEnabled;
            info.IsCacheEnabled = this.IsCacheEnabled;
            info.IsPreExtract = this.IsPreExtract;
            info.DefaultExtensions = this.DefaultExtensions?.Clone();
            info.UserExtensions = this.UserExtensions?.Clone();

            return info;
        }

        /// <summary>
        /// Susieプラグインがアクセス可能な形のパスに変換
        /// </summary>
        private static string GetLegacyPathName(string source)
        {
            var path = source;

            // WoW64回避
            if (Environment.Is64BitOperatingSystem)
            {
                var winDir = Environment.GetEnvironmentVariable("WINDIR") ?? @"C:\Windows";
                var system32dir = Path.Combine(winDir, "System32") + "\\";

                if (source.StartsWith(system32dir, StringComparison.OrdinalIgnoreCase))
                {
                    path = string.Concat(Path.Combine(winDir, "Sysnative"), "\\", path.AsSpan(system32dir.Length));
                    return path;

                    // NOTE: System32、Sysnative は特殊なフォルダーのためか GetShortPathName が正常に動作しない
                }
            }

            // ショートパス名に変換
            // NOTE: ドライブの設定によっては非対応
            path = NativeMethods.GetShortPathName(path);

            return path;
        }

        /// <summary>
        /// 判定用にファイル先頭を読み込む
        /// </summary>
        public static byte[] LoadHead(string fileName)
        {
            var path = GetLegacyPathName(fileName);

            var buff = new byte[2048];
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                _ = fs.Read(buff, 0, 2048);
            }
            return buff;
        }
    }

}
