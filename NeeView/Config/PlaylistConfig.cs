using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System.Text.Json.Serialization;

namespace NeeView
{
    [Equatable(IgnoreInheritedMembers = true)]
    public partial class PlaylistConfig : BindableBase, IHasPanelListItemStyle
    {
        private PanelListItemStyle _panelListItemStyle;
        private bool _isGroupBy;
        private bool _isCurrentBookFilterEnabled;
        private bool _isFirstIn;

        [JsonInclude, JsonPropertyName(nameof(PlaylistFolder))]
        public string? _playlistFolder;

        [JsonInclude, JsonPropertyName(nameof(CurrentPlaylist))]
        public string? _currentPlaylist;


        [PropertyMember]
        public PanelListItemStyle PanelListItemStyle
        {
            get { return _panelListItemStyle; }
            set { SetProperty(ref _panelListItemStyle, value); }
        }

        [JsonIgnore]
        [PropertyPath(FileDialogType = Windows.Controls.FileDialogType.Directory)]
        public string PlaylistFolder
        {
            get { return ToFullPlaylistFolder(_playlistFolder); }
            set { SetProperty(ref _playlistFolder, ToShortPlaylistFolder(value)); }
        }

        [JsonIgnore]
        [PropertyMapIgnore]
        public string DefaultPlaylist => string.IsNullOrEmpty(PlaylistFolder) ? "" : System.IO.Path.Combine(PlaylistFolder, "Default.nvpls");

        [JsonIgnore]
        [PropertyMapIgnore]
        public string PagemarkPlaylist => string.IsNullOrEmpty(PlaylistFolder) ? "" : System.IO.Path.Combine(PlaylistFolder, "Pagemark.nvpls");

        [JsonIgnore]
        [PropertyPath(FileDialogType = FileDialogType.SaveFile, Filter = "NeeView Playlist|*.nvpls")]
        public string CurrentPlaylist
        {
            get { return ToFullPlaylistPath(_currentPlaylist); }
            set { SetProperty(ref _currentPlaylist, ToShortPlaylistPath(value)); }
        }

        [PropertyMember]
        public bool IsGroupBy
        {
            get { return _isGroupBy; }
            set { SetProperty(ref _isGroupBy, value); }
        }

        [PropertyMember]
        public bool IsCurrentBookFilterEnabled
        {
            get { return _isCurrentBookFilterEnabled; }
            set { SetProperty(ref _isCurrentBookFilterEnabled, value); }
        }

        [PropertyMember]
        public bool IsFirstIn
        {
            get { return _isFirstIn; }
            set { SetProperty(ref _isFirstIn, value); }
        }



        private string? ToShortPlaylistFolder(string path)
        {
            if (path is null)
            {
                return null;
            }

            path = LoosePath.NormalizeSeparator(path.Trim()).TrimEnd();
            if (string.IsNullOrWhiteSpace(path) || path == SaveDataProfile.DefaultPlaylistsFolder)
            {
                return null;
            }

            return path;
        }

        private string ToFullPlaylistFolder(string? path)
        {
            if (path is null)
            {
                return SaveDataProfile.DefaultPlaylistsFolder;
            }

            return path;
        }

        private string? ToShortPlaylistPath(string path)
        {
            if (path is null)
            {
                return null;
            }

            path = LoosePath.NormalizeSeparator(path.Trim());
            if (string.IsNullOrWhiteSpace(path) || path == DefaultPlaylist)
            {
                return null;
            }

            var folder = LoosePath.TrimDirectoryEnd(PlaylistFolder);
            if (path.StartsWith(folder, System.StringComparison.OrdinalIgnoreCase))
            {
                var shortPath = path.Substring(folder.Length);
                if (!shortPath.Contains(LoosePath.DefaultSeparator, System.StringComparison.Ordinal))
                {
                    return shortPath;
                }
            }

            return path;
        }

        private string ToFullPlaylistPath(string? path)
        {
            if (path is null)
            {
                return DefaultPlaylist;
            }

            if (System.IO.Path.IsPathFullyQualified(path))
            {
                return path;
            }

            return System.IO.Path.Combine(PlaylistFolder, path);
        }
    }
}

