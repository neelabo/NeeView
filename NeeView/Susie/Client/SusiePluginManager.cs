using NeeLaboratory.Collections.Specialized;
using NeeLaboratory.ComponentModel;
using NeeView.Susie;
using NeeView.Susie.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace NeeView
{
    public class SusiePluginManager : BindableBase
    {
        static SusiePluginManager() => Current = new SusiePluginManager();
        public static SusiePluginManager Current { get; }

        private bool _isInitialized;
        private SusiePluginClient? _client;
        private List<SusiePluginInfo> _unauthorizedPlugins;
        private ObservableCollection<SusiePluginInfo> _INPlugins;
        private ObservableCollection<SusiePluginInfo> _AMPlugins;
        private static Lock _lock = new();

        private SusiePluginManager()
        {
            _unauthorizedPlugins = new List<SusiePluginInfo>();
            _INPlugins = new ObservableCollection<SusiePluginInfo>();
            _AMPlugins = new ObservableCollection<SusiePluginInfo>();
        }


        public List<SusiePluginInfo> UnauthorizedPlugins
        {
            get { return _unauthorizedPlugins; }
            private set { _unauthorizedPlugins = value ?? new List<SusiePluginInfo>(); }
        }

        public ObservableCollection<SusiePluginInfo> INPlugins
        {
            get { return _INPlugins; }
            private set { SetProperty(ref _INPlugins, value); }
        }

        public ObservableCollection<SusiePluginInfo> AMPlugins
        {
            get { return _AMPlugins; }
            private set { SetProperty(ref _AMPlugins, value); }
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
        public FileTypeCollection ImageExtensions = new();

        /// <summary>
        /// 対応圧縮ファイル拡張子
        /// </summary>
        public FileTypeCollection ArchiveExtensions = new();



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

#if false
            // [DEBUG] Susie.IsEnable ON/OFF Loop
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
            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                FlushSusiePluginOrder();
            }
        }


        // PluginCollectionのOpen/Close
        private void UpdateSusiePluginCollection()
        {
            if (!_isInitialized) throw new InvalidOperationException("Not initialized");

            if (Config.Current.Susie.IsEnabled && Directory.Exists(Config.Current.Susie.SusiePluginPath))
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
                    _client.SetRecoveryAction(LoadSusiePlugins);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    var errorLogFileName = App.ErrorLogFileName;
                    App.ExportErrorLog(errorLogFileName, ex);
                    ToastService.Current.Show(new Toast(Properties.TextResources.GetString("SusieConnectError.Message") + $"<br/><a href=\"{errorLogFileName}\">{errorLogFileName}</a>", null, ToastIcon.Error) { IsXHtml = true });
                }
            }
        }

        private void LoadSusiePlugins()
        {
            if (_client is null) throw new InvalidOperationException("Client is null");

            _client.Initialize(System.IO.Path.GetFullPath(Config.Current.Susie.SusiePluginPath), Plugins.ToList());

            var plugins = _client.GetPlugin(null);
            UnauthorizedPlugins = new List<SusiePluginInfo>();
            INPlugins = new ObservableCollection<SusiePluginInfo>(plugins.Where(e => e.PluginType == SusiePluginType.Image));
            INPlugins.CollectionChanged += Plugins_CollectionChanged;
            AMPlugins = new ObservableCollection<SusiePluginInfo>(plugins.Where(e => e.PluginType == SusiePluginType.Archive));
            AMPlugins.CollectionChanged += Plugins_CollectionChanged;

            UpdateImageExtensions();
            UpdateArchiveExtensions();
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
                _client?.SetPluginOrder(Plugins.Select(e => e.Name).ToList());
            }
        }

        public SusieImagePluginAccessor GetImagePluginAccessor()
        {
            lock (_lock)
            {
                if (_client is null) throw new InvalidOperationException("Client is null");

                return new SusieImagePluginAccessor(_client, null);
            }
        }

        public SusieImagePluginAccessor? GetImagePluginAccessor(string fileName, byte[] buff, bool isCheckExtension)
        {
            lock (_lock)
            {
                if (_client is null) throw new InvalidOperationException("Client is null");

                var plugin = _client.GetImagePlugin(fileName, buff, isCheckExtension);
                if (plugin is null) return null;

                return new SusieImagePluginAccessor(_client, plugin);
            }
        }

        public SusieArchivePluginAccessor? GetArchivePluginAccessor(string fileName, byte[]? buff, bool isCheckExtension, string? pluginName)
        {
            lock (_lock)
            {
                if (_client is null) throw new InvalidOperationException("Client is null");

                var plugin = _client.GetArchivePlugin(fileName, buff, isCheckExtension, pluginName);
                if (plugin is null) return null;

                return new SusieArchivePluginAccessor(_client, plugin);
            }
        }

        public void ShowPluginConfigurationDialog(string pluginName, Window owner)
        {
            lock (_lock)
            {
                if (_client is null) throw new InvalidOperationException("Client is null");

                var handle = new WindowInteropHelper(owner).Handle;
                _client.ShowConfigurationDlg(pluginName, handle.ToInt32());
            }
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
    }


    public class SusiePluginCollection : Dictionary<string, SusiePluginMemento>
    {
    }

    [Memento]
    public class SusiePluginMemento : ISusiePluginInfo
    {
        [JsonIgnore]
        public string Name { get; set; } = "";

        public bool IsEnabled { get; set; }
        public bool IsCacheEnabled { get; set; }
        public bool IsPreExtract { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ApiVersion { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? PluginVersion { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool HasConfigurationDlg { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public FileExtensionCollection? DefaultExtensions { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public FileExtensionCollection? UserExtensions { get; set; }


        public static SusiePluginMemento FromSusiePluginInfo(SusiePluginInfo plugin)
        {
            var setting = new SusiePluginMemento();
            setting.Name = plugin.Name;
            setting.IsEnabled = plugin.IsEnabled;
            setting.IsCacheEnabled = plugin.IsCacheEnabled;
            setting.IsPreExtract = plugin.IsPreExtract;
            setting.ApiVersion = plugin.ApiVersion;
            setting.PluginVersion = plugin.PluginVersion;
            setting.HasConfigurationDlg = plugin.HasConfigurationDlg;
            setting.DefaultExtensions = plugin.DefaultExtensions?.Clone();
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
            info.ApiVersion = ApiVersion;
            info.PluginVersion = PluginVersion;
            info.HasConfigurationDlg = HasConfigurationDlg;
            info.DefaultExtensions = DefaultExtensions?.Clone();
            info.UserExtensions = UserExtensions?.Clone();
            return info;
        }
    }

}
