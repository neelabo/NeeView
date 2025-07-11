using NeeLaboratory.ComponentModel;
using NeeView.Collections;
using NeeView.IO;
using System;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class PlaylistItem : BindableBase, IHasPage, IHasName, IRenameable
    {
        private readonly PlaylistSourceItem _item;
        private string? _place;
        private Page? _archivePage;
        private bool? _isArchive;
        private ArchiveType? _archiveType;

        public PlaylistItem(string path) : this(path, null)
        {
        }

        public PlaylistItem(PlaylistSourceItem item) : this(item.Path, item.Name)
        {
        }

        public PlaylistItem(string path, string? name)
        {
            _item = new PlaylistSourceItem(ValidPath(path), name);
        }


        public string Path
        {
            get { return _item.Path; }
            private set
            {
                if (_item.Path != value)
                {
                    _item.Path = ValidPath(value);
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(Name));
                    RaisePropertyChanged(nameof(Detail));
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
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(IsNameChanged));
                }
            }
        }

        public bool IsNameChanged => _item.IsNameChanged;

        public string Place
        {
            get
            {
                if (_place is null)
                {
                    if (FileIO.ExistsPath(Path))
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

        public string DisplayPlace
        {
            get { return SidePanelProfile.GetDecoratePlaceName(Place); }
        }

        public bool IsArchive
        {
            get
            {
                if (_isArchive is null)
                {
                    var targetPath = Path;
                    if (FileShortcut.IsShortcut(Path))
                    {
                        targetPath = new FileShortcut(Path).TargetPath ?? Path;
                    }
                    _isArchive = ArchiveManager.Current.IsSupported(targetPath) || System.IO.Directory.Exists(targetPath);
                }
                return _isArchive.Value;
            }
        }

        public ArchiveType ArchiveType
        {
            get
            {
                if (_archiveType is null)
                {
                    var targetPath = Path;
                    if (FileShortcut.IsShortcut(Path))
                    {
                        targetPath = new FileShortcut(Path).TargetPath ?? Path;
                    }
                    _archiveType = ArchiveManager.Current.GetSupportedType(targetPath);
                    if (_archiveType == ArchiveType.None && System.IO.Directory.Exists(targetPath))
                    {
                        _archiveType = ArchiveType.FolderArchive;
                    }
                }
                return _archiveType.Value;
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


        private void Thumbnail_Touched(object? sender, EventArgs e)
        {
            if (sender is not Thumbnail thumbnail) return;

            BookThumbnailPool.Current.Add(thumbnail);
        }

        public void UpdateDisplayPlace()
        {
            RaisePropertyChanged(nameof(DisplayPlace));
        }

        public Page GetPage()
        {
            return ArchivePage;
        }

        private static string ValidPath(string path)
        {
            // 動画名が重複するパスを修正する
            if (ArchiveManager.Current.IsSupported(path, ArchiveType.MediaArchive))
            {
                var tokens = path.Split(LoosePath.Separators);
                var count = tokens.Length;
                if (count >= 2 && tokens[count - 1] == tokens[count - 2] && !System.IO.File.Exists(path))
                {
                    return LoosePath.GetDirectoryName(path);
                }
            }
            return path;
        }

        public PlaylistSourceItem ToPlaylistItem()
        {
            return new PlaylistSourceItem(Path, Name);
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

        public async ValueTask<bool> RenameAsync(string name)
        {
            // TODO: この命令でリストの保存処理等の波及処理が実行されるようにする

            await Task.CompletedTask;
            throw new NotImplementedException();
        }
    }
}
