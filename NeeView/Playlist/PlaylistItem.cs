using CommunityToolkit.Mvvm.ComponentModel;
using NeeView.Collections;
using NeeView.IO;
using System;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class PlaylistItem : ObservableObject, IHasPage, IHasName, IRenameable
    {
        private readonly PlaylistSourceItem _item;
        private string? _place;
        private string? _displayPlace;
        private Page? _archivePage;
        private ArchiveType? _archiveType;


        public PlaylistItem(string path) : this(path, null, false)
        {
        }

        public PlaylistItem(PlaylistSourceItem item) : this(item.Path, item.Name, item.Invalid)
        {
        }

        public PlaylistItem(string path, string? name, bool invalid)
        {
            _item = new PlaylistSourceItem(ValidPath(path), name, invalid);
        }


        public string Path
        {
            get { return _item.Path; }
            set
            {
                if (_item.Path != value)
                {
                    _item.Path = ValidPath(value);
                    _place = null;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Name));
                    OnPropertyChanged(nameof(Place));
                    OnPropertyChanged(nameof(DisplayPlace));
                    OnPropertyChanged(nameof(Detail));
                }
            }
        }

        public string Name
        {
            get { return _item.Name; }
            set
            {
                if (_item.Name != value)
                {
                    _item.Name = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsNameChanged));
                }
            }
        }

        public string? RawName => _item.NameRaw;

        public bool IsNameChanged => _item.IsNameChanged;

        public string Place
        {
            get
            {
                if (_place is null)
                {
                    if (FileIO.EntryExists(Path))
                    {
                        _place = LoosePath.GetDirectoryName(Path);
                    }
                    else
                    {
                        _place = ArchiveEntryUtility.GetExistEntryName(Path) ?? "";
                    }
                }
                return _place;
            }
        }

        public string? DisplayPlace
        {
            get
            {
                if (_displayPlace is null)
                {
                    Task.Run(() =>
                    {
                        _displayPlace = SidePanelProfile.GetDecoratePlaceName(Place);
                        OnPropertyChanged(nameof(DisplayPlace));
                        OnPropertyChanged(nameof(Detail));
                    });
                }
                return _displayPlace;
            }
        }

        public ArchiveType ArchiveType
        {
            get
            {
                if (_archiveType is null)
                {
                    Task.Run(() =>
                    {
                        var targetPath = Path;
                        if (FileShortcut.IsShortcut(Path))
                        {
                            targetPath = new FileShortcut(Path).TargetPath ?? Path;
                        }
                        _archiveType = ArchiveManager.Current.GetSupportedType(targetPath);
                        if (_archiveType == ArchiveType.None && FileIO.DirectoryExists(targetPath))
                        {
                            _archiveType = ArchiveType.FolderArchive;
                        }
                        OnPropertyChanged(nameof(ArchiveType));
                        OnPropertyChanged(nameof(IsArchiveIconVisible));
                    });
                }
                return _archiveType.HasValue ? _archiveType.Value : ArchiveType.None;
            }
        }

        public bool IsArchiveIconVisible
        {
            get
            {
                return ArchiveType != ArchiveType.None && !IsUnlinked;
            }
        }

        public Page ArchivePage
        {
            get
            {
                if (_archivePage == null)
                {
                    _archivePage = new Page(new ArchivePageContent(ArchiveEntryUtility.CreateTemporaryEntry(Path), null));
                    _archivePage.Thumbnail.IsCacheEnabled = true;
                    _archivePage.Thumbnail.Touched += Thumbnail_Touched;
                }
                return _archivePage;
            }
        }

        public string Detail
        {
            get
            {
                var s = new StringBuilder();
                s.AppendLine(Path);
                s.AppendLine(DisplayPlace);
                s.Append(Name);
                return s.ToString();
            }
        }

        public bool IsUnlinked
        {
            get { return _item.Invalid; }
            set
            {
                if (_item.Invalid != value)
                {
                    _item.Invalid = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsArchiveIconVisible));
                }
            }
        }


        private void Thumbnail_Touched(object? sender, EventArgs e)
        {
            if (sender is not Thumbnail thumbnail) return;

            BookThumbnailPool.Current.Add(thumbnail);
        }

        public void UpdateDisplayPlace()
        {
            OnPropertyChanged(nameof(DisplayPlace));
        }

        public Page GetPage()
        {
            return ArchivePage;
        }

        public void ClearArchivePage()
        {
            if (_archivePage != null)
            {
                _archivePage.Thumbnail.Touched -= Thumbnail_Touched;
                _archivePage.Dispose();
                _archivePage = null;
            }
        }

        private static string ValidPath(string path)
        {
            // 動画名が重複するパスを修正する
            if (ArchiveManager.Current.IsSupported(path, ArchiveType.MediaArchive))
            {
                var tokens = path.Split(LoosePath.Separators);
                var count = tokens.Length;
                if (count >= 2 && tokens[count - 1] == tokens[count - 2] && !FileIO.FileExists(path))
                {
                    return LoosePath.GetDirectoryName(path);
                }
            }
            return path;
        }

        public PlaylistSourceItem ToPlaylistItem()
        {
            return new PlaylistSourceItem(Path, Name, IsUnlinked);
        }

        public override string? ToString()
        {
            return Name;
        }

        public string GetRenameText()
        {
            return Name;
        }

        public bool CanRename()
        {
            return true;
        }

        public async Task<bool> RenameAsync(string name)
        {
            // TODO: この命令でリストの保存処理等の波及処理が実行されるようにする

            throw new NotImplementedException();
        }

        public bool EqualsTo(PlaylistSourceItem source)
        {
            return this.Path == source.Path && this.Name == source.Name;
        }
    }
}
