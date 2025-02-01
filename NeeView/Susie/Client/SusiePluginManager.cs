using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using NeeView.Susie;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NeeView.Susie.Client;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Interop;
using System.Threading;
using NeeLaboratory.Collections.Specialized;

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
        private static System.Threading.Lock _lock = new();

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

            var settings = Plugins.Select(e => e.ToSusiePluginSetting()).ToList();
            _client.Initialize(System.IO.Path.GetFullPath(Config.Current.Susie.SusiePluginPath), settings);

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
        public void WriteSusiePluginInfo(SusiePluginInfo pluginInfo)
        {
            lock (_lock)
            {
                if (_client is null) throw new InvalidOperationException("Client is null");

                var plugin = Plugins.FirstOrDefault(e => e.Name == pluginInfo.Name);
                if (plugin is null) return;

                plugin.Set(pluginInfo);
            }
        }

        public void FlushSusiePluginSetting(string name)
        {
            lock (_lock)
            {
                if (_client is null) throw new InvalidOperationException("Client is null");

                var settings = Plugins
                    .Where(e => e.Name == name)
                    .Select(e => e.ToSusiePluginSetting())
                    .ToList();

                FlushSusiePluginSetting(settings);
            }
        }

        public void FlushSusiePluginSetting(List<SusiePluginSetting> settings)
        {
            Debug.WriteLine($"FlushSusiePluginSetting: {string.Join(", ", settings.Select(e => e.Name).ToArray())}");

            if (settings.Count == 0) return;
            
            lock (_lock)
            {
                if (_client is null) throw new InvalidOperationException("Client is null");

                _client.SetPlugin(settings);
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
                    var plugin = collection.FirstOrDefault(e => e.Name == name);
                    if (plugin is not null)
                    {
                        var index = collection.IndexOf(plugin);
                        if (index >= 0)
                        {
                            collection[index].Set(plugins[0]);
                        }
                    }
                }
            }
        }

        // プラグイン設定の更新
        public void UpdatePlugin(SusiePluginInfo pluginInfo)
        {
            WriteSusiePluginInfo(pluginInfo);
            FlushSusiePluginSetting(pluginInfo.Name);
            UpdateSusiePlugin(pluginInfo.Name);
            UpdateExtensions(pluginInfo.PluginType);
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

        public SusieArchivePluginAccessor? GetArchivePluginAccessor(string fileName, byte[]? buff, bool isCheckExtension)
        {
            lock (_lock)
            {
                if (_client is null) throw new InvalidOperationException("Client is null");

                var plugin = _client.GetArchivePlugin(fileName, buff, isCheckExtension);
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

        #endregion Memento
    }

    public class SusiePluginCollection : Dictionary<string, SusiePluginMemento>
    {
    }

    [Memento]
    public class SusiePluginMemento
    {
        public bool IsEnabled { get; set; }

        public bool IsCacheEnabled { get; set; }

        public bool IsPreExtract { get; set; }

        public string? UserExtensions { get; set; }


        public static SusiePluginMemento FromSusiePluginInfo(SusiePluginInfo info)
        {
            var setting = new SusiePluginMemento();
            setting.IsEnabled = info.IsEnabled;
            setting.IsCacheEnabled = info.IsCacheEnabled;
            setting.IsPreExtract = info.IsPreExtract;
            setting.UserExtensions = info.UserExtension?.ToOneLine();
            return setting;
        }

        public static SusiePluginMemento FromSusiePluginSetting(Susie.SusiePluginSetting setting)
        {
            var memento = new SusiePluginMemento();
            memento.IsEnabled = setting.IsEnabled;
            memento.IsCacheEnabled = setting.IsCacheEnabled;
            memento.IsPreExtract = setting.IsPreExtract;
            memento.UserExtensions = setting.UserExtensions;
            return memento;
        }

        public SusiePluginInfo ToSusiePluginInfo(string name)
        {
            var info = new SusiePluginInfo(name);
            info.IsEnabled = IsEnabled;
            info.IsCacheEnabled = IsCacheEnabled;
            info.IsPreExtract = IsPreExtract;
            info.UserExtension = UserExtensions != null ? new FileExtensionCollection(UserExtensions) : null;
            return info;
        }
    }

}
