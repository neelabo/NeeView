using Microsoft.Win32;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class MediaArchiveConfig : BindableBase, IMediaContext
    {
        public static FileTypeCollection DefaultSupportFileTypes { get; } = new FileTypeCollection(".asf;.avi;.mp4;.mkv;.mov;.wmv");


        private bool _isEnabled = true;
        private bool _isMediaPageEnabled = false;
        private FileTypeCollection _supportFileTypes = (FileTypeCollection)DefaultSupportFileTypes.Clone();
        private double _pageSeconds = 10.0;
        private double _mediaStartDelaySeconds = 0.5;
        private bool _isMuted;
        private double _volume = 0.5;
        private bool _isRepeat;
        private bool _isLibVlcEnabled;
        private DefaultSubtitle _defaultSubtitle = DefaultSubtitle.Default;

        [JsonInclude, JsonPropertyName(nameof(LibVlcPath))]
        public string? _libVlcPath;


        /// <summary>
        /// 動画をブックとする
        /// </summary>
        [PropertyMember]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        /// <summary>
        /// 動画をページとする
        /// </summary>
        [PropertyMember]
        public bool IsMediaPageEnabled
        {
            get { return _isMediaPageEnabled; }
            set { SetProperty(ref _isMediaPageEnabled, value); }
        }

        [PropertyMember]
        public FileTypeCollection SupportFileTypes
        {
            get { return _supportFileTypes; }
            set { SetProperty(ref _supportFileTypes, value); }
        }

        [PropertyMember]
        public double PageSeconds
        {
            get { return _pageSeconds; }
            set { SetProperty(ref _pageSeconds, value); }
        }

        [PropertyMember]
        public double MediaStartDelaySeconds
        {
            get { return _mediaStartDelaySeconds; }
            set { SetProperty(ref _mediaStartDelaySeconds, value); }
        }

        [PropertyMember]
        public bool IsMuted
        {
            get { return _isMuted; }
            set { SetProperty(ref _isMuted, value); }
        }

        [PropertyMember]
        public double Volume
        {
            get { return _volume; }
            set { SetProperty(ref _volume, value); }
        }

        [PropertyMember]
        public bool IsRepeat
        {
            get { return _isRepeat; }
            set { SetProperty(ref _isRepeat, value); }
        }

        [PropertyMember]
        public bool IsLibVlcEnabled
        {
            get { return _isLibVlcEnabled; }
            set { SetProperty(ref _isLibVlcEnabled, value); }
        }

        [JsonIgnore]
        [PropertyPath(FileDialogType = Windows.Controls.FileDialogType.Directory)]
        public string LibVlcPath
        {
            get { return _libVlcPath ?? LibVlcProfile.DefaultLibVlcPath; }
            set { SetProperty(ref _libVlcPath, (string.IsNullOrWhiteSpace(value) || value.Trim() == LibVlcProfile.DefaultLibVlcPath) ? null : value.Trim()); }
        }

        [PropertyMember]
        public DefaultSubtitle DefaultSubtitle
        {
            get { return _defaultSubtitle; }
            set { SetProperty(ref _defaultSubtitle, value); }
        }
    }

    /// <summary>
    /// 既定の字幕
    /// </summary>
    public enum DefaultSubtitle
    {
        // 既定の字幕を使用する
        Default,

        // 字幕を無効にする
        Disable,
    }


    public static class LibVlcProfile
    {
        static LibVlcProfile()
        {
            try
            {
                // get VLC media player install folder
                var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\VLC media player");
                var installLocation = (string?)key?.GetValue("InstallLocation");
                DefaultLibVlcPath = installLocation ?? "";
            }
            catch
            {
            }
        }

        public static string DefaultLibVlcPath { get; private set; } = "";
    }
}
