using NeeLaboratory.Collections.Specialized;
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace NeeView.Susie
{
    public class SusiePluginInfo : INotifyPropertyChanged, ISusiePluginInfo
    {
        #region INotifyPropertyChanged Support

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion


        private string _name;
        private string? _apiVersion;
        private string? _pluginVersion;
        private bool _hasConfigurationDlg;
        private bool _isEnabled;
        private bool _isCacheEnabled;
        private bool _isPreExtract;
        private FileExtensionCollection? _defaultExtension;
        private FileExtensionCollection? _userExtension;

        public SusiePluginInfo() : this("")
        {
        }

        public SusiePluginInfo(string name)
        {
            _name = name;
        }


        public bool IsValid => !string.IsNullOrEmpty(Name) && ApiVersion is not null;

        public string Name
        {
            get { return _name; }
            set
            {
                if (SetProperty(ref _name, value))
                {
                    RaisePropertyChanged(nameof(DetailText));
                }
            }
        }

        public string? ApiVersion
        {
            get { return _apiVersion; }
            set
            {
                if (SetProperty(ref _apiVersion, value))
                {
                    RaisePropertyChanged(nameof(PluginType));
                }
            }
        }

        public string? PluginVersion
        {
            get { return _pluginVersion; }
            set { SetProperty(ref _pluginVersion, value); }
        }

        public SusiePluginType PluginType => SusiePluginTypeExtensions.FromApiVersion(_apiVersion);

        public string DetailText { get { return $"{Name} ( {string.Join(" ", Extensions)} )"; } }

        public bool HasConfigurationDlg
        {
            get { return _hasConfigurationDlg; }
            set { SetProperty(ref _hasConfigurationDlg, value); }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        public bool IsCacheEnabled
        {
            get { return _isCacheEnabled; }
            set { SetProperty(ref _isCacheEnabled, value); }
        }

        public bool IsPreExtract
        {
            get { return _isPreExtract; }
            set { SetProperty(ref _isPreExtract, value); }
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public FileExtensionCollection? DefaultExtensions
        {
            get { return _defaultExtension; }
            set
            {
                if (SetProperty(ref _defaultExtension, value))
                {
                    RaisePropertyChanged(nameof(Extensions));
                    RaisePropertyChanged(nameof(DetailText));
                }
            }
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public FileExtensionCollection? UserExtensions
        {
            get { return _userExtension; }
            set
            {
                var extension = (value is null || value.Equals(_defaultExtension)) ? null : value;
                if (SetProperty(ref _userExtension, extension))
                {
                    RaisePropertyChanged(nameof(Extensions));
                    RaisePropertyChanged(nameof(DetailText));
                }
            }
        }

        public FileExtensionCollection Extensions => UserExtensions ?? DefaultExtensions ?? FileExtensionCollection.Empty;


        /// <summary>
        /// プラグイン情報を指定されたインスタンスに書き込みます。
        /// </summary>
        /// <param name="target"></param>
        public void CopyTo(SusiePluginInfo target)
        {
            target.Name = Name;
            target.ApiVersion = ApiVersion;
            target.PluginVersion = PluginVersion;
            target.HasConfigurationDlg = HasConfigurationDlg;
            target.IsEnabled = IsEnabled;
            target.IsCacheEnabled = IsCacheEnabled;
            target.IsPreExtract = IsPreExtract;
            target.DefaultExtensions = DefaultExtensions?.Clone();
            target.UserExtensions = UserExtensions?.Clone();
        }
    }
}
