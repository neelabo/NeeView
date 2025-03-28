using NeeLaboratory.Collections.Specialized;
using NeeView.Susie;

namespace NeeView
{
    public class SusiePluginAccessor
    {
        private readonly SusiePluginInfo _plugin;

        public SusiePluginAccessor(SusiePluginInfo plugin)
        {
            _plugin = plugin;
        }

        [WordNodeMember]
        public string Name => _plugin.Name;

        [WordNodeMember]
        public string? Path => System.IO.Path.GetFullPath(System.IO.Path.Combine(Config.Current.Susie.SusiePluginPath, _plugin.Name));

        [WordNodeMember]
        public string? ApiVersion => _plugin.ApiVersion;

        [WordNodeMember]
        public string? PluginVersion => _plugin.PluginVersion;

        [WordNodeMember(DocumentType = typeof(SusiePluginType))]
        public string PluginType => _plugin.PluginType.ToString();

        [WordNodeMember]
        public string? DetailText => _plugin.DetailText;

        [WordNodeMember]
        public bool HasConfigDialog => _plugin.HasConfigurationDlg;

        [WordNodeMember]
        public bool IsEnabled
        {
            get => _plugin.IsEnabled;
            set
            {
                if (_plugin.IsEnabled != value)
                {
                    _plugin.IsEnabled = value;
                    Update();
                }
            }
        }

        [WordNodeMember]
        public bool IsCacheEnabled
        {
            get => _plugin.IsCacheEnabled;
            set
            {
                if (_plugin.IsCacheEnabled != value)
                {
                    _plugin.IsCacheEnabled = value;
                    Update();
                }
            }
        }

        [WordNodeMember]
        public bool IsPreExtract
        {
            get => _plugin.IsPreExtract;
            set
            {
                if (_plugin.IsPreExtract != value)
                {
                    _plugin.IsPreExtract = value;
                    Update();
                }
            }
        }

        [WordNodeMember]
        public string? Extensions
        {
            get => _plugin.Extensions.ToOneLine();
            set
            {
                var extensions = string.IsNullOrEmpty(value) ? null : new FileExtensionCollection(value);
                if (!_plugin.Extensions.Equals(extensions))
                {
                    _plugin.UserExtensions = extensions;
                    Update();
                }
            }
        }


        [WordNodeMember]
        public void ShowConfigDialog()
        {
            AppDispatcher.Invoke(() =>
            {
                SusiePluginManager.Current.ShowPluginConfigurationDialog(_plugin.Name, MainWindow.Current);
            });
        }

        private void Update()
        {
            AppDispatcher.Invoke(() => SusiePluginManager.Current.UpdatePlugin(_plugin));
        }
    }
}
