//#define LOCAL_DEBUG

using NeeLaboratory.Collections.Specialized;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.Properties;
using NeeView.Susie;
using NeeView.Susie.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace NeeView
{
    [LocalDebug]
    public partial class SusiePluginManager : BindableBase
    {
        private static readonly Lazy<SusiePluginManager> _current = new();
        public static SusiePluginManager Current => _current.Value;

        private bool _isInitialized;
        private SusiePluginClient? _client;
        private List<SusiePluginInfo> _unauthorizedPlugins;
        private ObservableCollection<SusiePluginInfo> _INPlugins;
        private ObservableCollection<SusiePluginInfo> _AMPlugins;
        private RecoveryState _recoveryState = new("", []);
        private static Lock _lock = new();

        private readonly Lazy<SusiePluginDatabaseCache> _susiePluginDatabase = new(() => new(Database.Current));

        public SusiePluginManager()
        {
            _unauthorizedPlugins = [];
            INPlugins = [];
            AMPlugins = [];
        }


        private SusiePluginDatabaseCache SusiePluginDatabase => _susiePluginDatabase.Value;

        public List<SusiePluginInfo> UnauthorizedPlugins
        {
            get { return _unauthorizedPlugins; }
            private set { _unauthorizedPlugins = value ?? new List<SusiePluginInfo>(); }
        }

        public ObservableCollection<SusiePluginInfo> INPlugins
        {
            get { return _INPlugins; }

            [MemberNotNull(nameof(_INPlugins))]
            private set
            {
                if (_INPlugins != value)
                {
                    _INPlugins?.CollectionChanged -= Plugins_CollectionChanged;
                    _INPlugins = value;
                    _INPlugins.CollectionChanged += Plugins_CollectionChanged;
                    RaisePropertyChanged(nameof(INPlugins));
                }
            }
        }

        public ObservableCollection<SusiePluginInfo> AMPlugins
        {
            get { return _AMPlugins; }

            [MemberNotNull(nameof(_AMPlugins))]
            private set
            {
                if (_AMPlugins != value)
                {
                    _AMPlugins?.CollectionChanged -= Plugins_CollectionChanged;
                    _AMPlugins = value;
                    _AMPlugins.CollectionChanged += Plugins_CollectionChanged;
                    RaisePropertyChanged(nameof(AMPlugins));
                }
            }
        }

        public IEnumerable<SusiePluginInfo> Plugins
        {
            get
            {
                foreach (var plugin in UnauthorizedPlugins) yield return plugin;
                foreach (var plugin in INPlugins) yield return plugin;
                foreach (var plugin in AMPlugins) yield return plugin;
            }
        }

        /// <summary>
        /// 対応画像ファイル拡張子
        /// </summary>
        public FileTypeCollection ImageExtensions { get; } = new();

        /// <summary>
        /// 対応圧縮ファイル拡張子
        /// </summary>
        public FileTypeCollection ArchiveExtensions { get; } = new();


        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            Config.Current.Susie.AddPropertyChanged(nameof(SusieConfig.IsEnabled), (s, e) =>
            {
                UpdateSusiePluginCollection();
            });

            Config.Current.Susie.AddPropertyChanging(nameof(SusieConfig.SusiePluginPath), (s, e) =>
            {
                CloseSusiePluginCollection();
            });

            Config.Current.Susie.AddPropertyChanged(nameof(SusieConfig.SusiePluginPath), (s, e) =>
            {
                UpdateSusiePluginCollection();
            });

            UpdateSusiePluginCollection();


            // NOTE: 開発用 Susie.IsEnable ON/OFF Loop
#if false
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(3000);
                    await AppDispatcher.BeginInvoke(() =>
                    {
                        Config.Current.Susie.IsEnabled = !Config.Current.Susie.IsEnabled;
                    });
                }
            });
#endif
        }

        private void Plugins_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            LocalDebug.WriteLine($"ChangedAction={e.Action}");

            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                FlushSusiePluginOrder();
            }
        }

        private void UpdateRecoveryState()
        {
            LocalDebug.WriteLine($"UpdateRecoveryState()");

            var state = new RecoveryState(Path.GetFullPath(Config.Current.Susie.SusiePluginPath), Plugins.ToList());

            Volatile.Write(ref _recoveryState, state);
        }

        private void RecoverClientState()
        {
            if (_client is null) throw new InvalidOperationException("Client is null");

            var state = Volatile.Read(ref _recoveryState);
            _client.Initialize(state.PluginFolder, state.Plugins.ToList());
        }

        // PluginCollectionのOpen/Close
        private void UpdateSusiePluginCollection()
        {
            if (!_isInitialized) throw new InvalidOperationException("Not initialized");

            if (Config.Current.Susie.IsEnabled && FileIO.DirectoryExists(Config.Current.Susie.SusiePluginPath))
            {
                OpenSusiePluginCollection();
            }
            else
            {
                CloseSusiePluginCollection();
            }
        }


        private void OpenSusiePluginCollection()
        {
            lock (_lock)
            {
                CloseSusiePluginCollection();
                Debug.Assert(_client is null);

                _client = new SusiePluginClient();

                try
                {
                    LoadSusiePlugins();
                    _client.SetRecoveryAction(RecoverClientState);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    var errorLogFileName = App.ErrorLogFileName;
                    App.ExportErrorLog(errorLogFileName, ex);
                    ToastService.Current.Show(new Toast(TextResources.GetString("SusieConnectError.Message") + $"<br/><a href=\"{errorLogFileName}\">{errorLogFileName}</a>", null, ToastIcon.Error) { IsXHtml = true });

                    _client.Dispose();
                    _client = null;
                }
            }
        }

        private void LoadSusiePlugins()
        {
            lock (_lock)
            {
                if (_client is null) throw new InvalidOperationException("Client is null");

                RestoreBaseInfo(Plugins);

                _client.Initialize(System.IO.Path.GetFullPath(Config.Current.Susie.SusiePluginPath), Plugins.ToList());

                var plugins = _client.GetPlugin(null);
                UnauthorizedPlugins = new List<SusiePluginInfo>();
                INPlugins = new ObservableCollection<SusiePluginInfo>(plugins.Where(e => e.PluginType == SusiePluginType.Image));
                AMPlugins = new ObservableCollection<SusiePluginInfo>(plugins.Where(e => e.PluginType == SusiePluginType.Archive));

                UpdateRecoveryState();

                StoreBaseInfo(Plugins);

                UpdateImageExtensions();
                UpdateArchiveExtensions();
            }
        }


        private void RestoreBaseInfo(IEnumerable<SusiePluginInfo> plugins)
        {
            var paths = plugins.Select(e => GetPluginFullPath(e)).ToList();

            var cache = SusiePluginDatabase.Read(paths).ToDictionary(e => Path.GetFileName(e.Path));
            foreach (var plugin in plugins)
            {
                if (cache.TryGetValue(plugin.Name, out var info))
                {
                    info.WriteTo(plugin);
                }
            }
        }

        private void StoreBaseInfo(IEnumerable<SusiePluginInfo> plugins)
        {
            var items = plugins.Select(e => new SusiePluginBaseInfo(e, Config.Current.Susie.SusiePluginPath)).ToList();

            SusiePluginDatabase.Write(items);
        }

        private static string GetPluginFullPath(SusiePluginInfo plugin)
        {
            return Path.GetFullPath(Path.Combine(Config.Current.Susie.SusiePluginPath, plugin.Name));
        }

        private void CloseSusiePluginCollection()
        {
            lock (_lock)
            {
                if (_client == null) return;

                _client.Dispose();
                _client = null;

                UnauthorizedPlugins = Plugins.ToList();
                INPlugins = new ObservableCollection<SusiePluginInfo>();
                AMPlugins = new ObservableCollection<SusiePluginInfo>();

                UpdateRecoveryState();

                UpdateImageExtensions();
                UpdateArchiveExtensions();
            }
        }

        // Susie プラグインのサポート拡張子を更新
        public void UpdateExtensions(SusiePluginType pluginType)
        {
            if (pluginType == SusiePluginType.Image)
            {
                UpdateImageExtensions();
            }
            else
            {
                UpdateArchiveExtensions();
            }
        }

        // Susie画像プラグインのサポート拡張子を更新
        public void UpdateImageExtensions()
        {
            var extensions = INPlugins
                .Where(e => e.IsEnabled)
                .SelectMany(e => e.Extensions);

            ImageExtensions.Restore(extensions);

            Debug.WriteLine("SusieIN Support: " + string.Join(" ", this.ImageExtensions));
        }

        // Susie書庫プラグインのサポート拡張子を更新
        public void UpdateArchiveExtensions()
        {
            var extensions = AMPlugins
                .Where(e => e.IsEnabled)
                .SelectMany(e => e.Extensions);

            ArchiveExtensions.Restore(extensions);

            Debug.WriteLine("SusieAM Support: " + string.Join(" ", this.ArchiveExtensions));
        }

        // SusiePluginInfo を上書き
        public void WriteSusiePlugin(SusiePluginInfo plugin)
        {
            lock (_lock)
            {
                if (_client is null) throw new InvalidOperationException("Client is null");

                var target = Plugins.FirstOrDefault(e => e.Name == plugin.Name);
                if (target is null) return;

                plugin.CopyTo(target);
            }
        }

        public void FlushSusiePlugin(string name)
        {
            lock (_lock)
            {
                if (_client is null) throw new InvalidOperationException("Client is null");

                var settings = Plugins
                    .Where(e => e.Name == name)
                    .ToList();

                FlushSusiePlugin(settings);
            }
        }

        public void FlushSusiePlugin(List<SusiePluginInfo> plugins)
        {
            Debug.WriteLine($"FlushSusiePlugin: {string.Join(", ", plugins.Select(e => e.Name).ToArray())}");

            if (plugins.Count == 0) return;

            lock (_lock)
            {
                if (_client is null) throw new InvalidOperationException("Client is null");

                _client.SetPlugin(plugins);
            }
        }

        public void UpdateSusiePlugin(string name)
        {
            lock (_lock)
            {
                if (_client is null) throw new InvalidOperationException("Client is null");

                var plugins = _client.GetPlugin(new List<string>() { name });
                if (plugins != null && plugins.Count == 1)
                {
                    var collection = plugins[0].PluginType == SusiePluginType.Image ? INPlugins : AMPlugins;
                    var target = collection.FirstOrDefault(e => e.Name == name);
                    if (target is not null)
                    {
                        plugins[0].CopyTo(target);
                    }
                }
            }
        }

        // プラグイン設定の更新
        public void UpdatePlugin(SusiePluginInfo plugin)
        {
            WriteSusiePlugin(plugin);
            FlushSusiePlugin(plugin.Name);
            UpdateSusiePlugin(plugin.Name);
            UpdateExtensions(plugin.PluginType);
        }

        public void FlushSusiePluginOrder()
        {
            lock (_lock)
            {
                UpdateRecoveryState();

                _client?.SetPluginOrder(Plugins.Select(e => e.Name).ToList());
            }
        }

        public SusieImage? GetImage(string? pluginName, string fileName, byte[]? buff, bool isCheckExtension)
        {
            lock (_lock)
            {
                if (_client is null) throw new InvalidOperationException("Susie plugin client is not exists");
                return _client.GetImage(pluginName, fileName, buff, isCheckExtension);
            }
        }

        public List<SusieArchiveEntry> GetArchiveEntries(string pluginName, string fileName)
        {
            lock (_lock)
            {
                if (_client is null) throw new InvalidOperationException("Susie plugin client is not exists");
                return _client.GetArchiveEntries(pluginName, fileName);
            }
        }

        public byte[] ExtractArchiveEntry(string pluginName, string fileName, int position)
        {
            lock (_lock)
            {
                if (_client is null) throw new InvalidOperationException("Susie plugin client is not exists");
                return _client?.ExtractArchiveEntry(pluginName, fileName, position) ?? [];
            }
        }

        public void ExtractArchiveEntryToFolder(string pluginName, string fileName, int position, string extractFolder)
        {
            lock (_lock)
            {
                if (_client is null) throw new InvalidOperationException("Susie plugin client is not exists");
                _client.ExtractArchiveEntryToFolder(pluginName, fileName, position, extractFolder);
            }
        }

        public SusieImagePluginAccessor GetImagePluginAccessor()
        {
            lock (_lock)
            {
                if (_client is null) throw new InvalidOperationException("Client is null");

                return new SusieImagePluginAccessor(this, null);
            }
        }

        public SusieImagePluginAccessor? GetImagePluginAccessor(string fileName, byte[] buff, bool isCheckExtension)
        {
            lock (_lock)
            {
                if (_client is null) throw new InvalidOperationException("Client is null");

                var plugin = _client.GetImagePlugin(fileName, buff, isCheckExtension);
                if (plugin is null) return null;

                return new SusieImagePluginAccessor(this, plugin);
            }
        }

        public SusieArchivePluginAccessor? GetArchivePluginAccessor(string fileName, byte[]? buff, bool isCheckExtension, string? pluginName)
        {
            lock (_lock)
            {
                if (_client is null) throw new InvalidOperationException("Client is null");

                var plugin = _client.GetArchivePlugin(fileName, buff, isCheckExtension, pluginName);
                if (plugin is null) return null;

                return new SusieArchivePluginAccessor(this, plugin);
            }
        }

        public void ShowPluginConfigurationDialog(string pluginName, Window owner)
        {
            lock (_lock)
            {
                if (_client is null) throw new InvalidOperationException("Client is null");

                // Susie server process is 32-bit, so HWND is marshaled as Int32 over IPC.
                var handle = new WindowInteropHelper(owner).Handle;
                _client.ShowConfigurationDlg(pluginName, handle.ToInt32());
            }
        }

        public void ClearUnauthorizedPlugins()
        {
            UnauthorizedPlugins = new();
        }

        #region Memento

        public SusiePluginCollection CreateSusiePluginCollection()
        {
            var collection = new SusiePluginCollection();
            foreach (var item in this.Plugins)
            {
                collection.Add(item.Name, SusiePluginMemento.FromSusiePluginInfo(item));
            }
            return collection;
        }

        public void RestoreSusiePluginCollection(SusiePluginCollection? memento)
        {
            if (memento == null) return;

            CloseSusiePluginCollection();
            this.UnauthorizedPlugins = memento.Select(e => e.Value.ToSusiePluginInfo(e.Key)).ToList();

            if (_isInitialized)
            {
                UpdateSusiePluginCollection();
            }
        }

        public List<string> GetSupportedPluginList(SusiePluginType pluginType, string ext)
        {
            return GetPluginCollection(pluginType).Where(e => e.IsEnabled && e.Extensions.Contains(ext)).Select(e => e.Name).ToList();
        }

        public bool IsSupportedPlugin(SusiePluginType pluginType, string ext, string pluginName)
        {
            return GetPluginCollection(pluginType).FirstOrDefault(e => e.Name == pluginName)?.Extensions.Contains(ext) ?? false;
        }

        private ObservableCollection<SusiePluginInfo> GetPluginCollection(SusiePluginType pluginType)
        {
            return pluginType switch
            {
                SusiePluginType.Image => INPlugins,
                SusiePluginType.Archive => AMPlugins,
                _ => throw new ArgumentOutOfRangeException(nameof(pluginType)),
            };
        }

        #endregion Memento


        private sealed record RecoveryState(string PluginFolder, List<SusiePluginInfo> Plugins);
    }


    public class SusiePluginCollection : Dictionary<string, SusiePluginMemento>
    {
    }

    [Memento]
    public class SusiePluginMemento
    {
        [JsonIgnore]
        public string Name { get; set; } = "";

        public bool IsEnabled { get; set; } = true;

        public bool IsCacheEnabled { get; set; } = true;

        public bool IsPreExtract { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public FileExtensionCollection? UserExtensions { get; set; }


        public static SusiePluginMemento FromSusiePluginInfo(SusiePluginInfo plugin)
        {
            var setting = new SusiePluginMemento();
            setting.Name = plugin.Name;
            setting.IsEnabled = plugin.IsEnabled;
            setting.IsCacheEnabled = plugin.IsCacheEnabled;
            setting.IsPreExtract = plugin.IsPreExtract;
            setting.UserExtensions = plugin.UserExtensions?.Clone();
            return setting;
        }

        public SusiePluginInfo ToSusiePluginInfo(string name)
        {
            var info = new SusiePluginInfo(name);
            // info.Name は引数で指定する
            info.IsEnabled = IsEnabled;
            info.IsCacheEnabled = IsCacheEnabled;
            info.IsPreExtract = IsPreExtract;
            info.UserExtensions = UserExtensions?.Clone();
            return info;
        }
    }

}
