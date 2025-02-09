using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using NeeView.IO;

namespace NeeView
{
    public class FolderItemFactory
    {
        private readonly QueryPath _place;
        private readonly bool _isOverlayEnabled;


        public FolderItemFactory(QueryPath place, bool isOverlayEnabled)
        {
            _place = place;
            _isOverlayEnabled = isOverlayEnabled;
        }


        /// <summary>
        /// 空のFolderItemを作成
        /// </summary>
        public FolderItem CreateFolderItemEmpty()
        {
            return new ConstFolderItem(new ResourceThumbnail("ic_noentry", MainWindow.Current), _isOverlayEnabled)
            {
                Type = FolderItemType.Empty,
                Place = _place,
                Name = ".",
                TargetPath = _place.ReplacePath(LoosePath.Combine(_place.Path, ".")),
                DispName = Properties.TextResources.GetString("Notice.NoFiles"),
                Attributes = FolderItemAttribute.Empty,
            };
        }


        /// <summary>
        /// クエリからFolderItemを作成
        /// </summary>
        /// <param name="path">パス</param>
        /// <returns>FolderItem。生成できなかった場合はnull</returns>
        public FolderItem? CreateFolderItem(QueryPath path)
        {
            if (path.Scheme == QueryScheme.File)
            {
                if (path.Path is null) throw new InvalidOperationException($"path.Path must not be null");
                return CreateFolderItem(path.Path);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// パスからFolderItemを作成
        /// </summary>
        /// <param name="path">パス</param>
        /// <returns>FolderItem。生成できなかった場合はnull</returns>
        public FolderItem? CreateFolderItem(string path)
        {
            var directory = new DirectoryInfo(path);
            if (directory.Exists)
            {
                return CreateFolderItem(directory);
            }

            var file = new FileInfo(path);
            if (file.Exists)
            {
                return CreateFolderItem(file);
            }

            return null;
        }


        public FolderItem? CreateFolderItem(FileSystemInfo? e)
        {
            if (e == null || !e.Exists) return null;

            if ((e.Attributes & FileAttributes.Directory) != 0)
            {
                var directoryInfo = e as DirectoryInfo;
                return CreateFolderItem(directoryInfo);
            }
            else
            {
                var fileInfo = e as FileInfo;
                return CreateFolderItem(fileInfo);
            }
        }


        /// <summary>
        /// DriveInfoからFolderItem作成
        /// </summary>
        public FolderItem? CreateFolderItem(DriveInfo e)
        {
            if (e == null) return null;

            var item = new DriveFolderItem(e, _isOverlayEnabled)
            {
                Place = _place,
                Name = e.Name,
                TargetPath = new QueryPath(e.Name),
                DispName = string.Format(CultureInfo.InvariantCulture, "{0} ({1})", e.DriveType.ToDispString(), e.Name.TrimEnd('\\')),
                Attributes = FolderItemAttribute.Directory | FolderItemAttribute.Drive,
                IsReady = DriveReadyMap.IsDriveReady(e.Name),
            };

            // IsReadyの取得に時間がかかる場合があるため、非同期で状態を更新
            Task.Run(() =>
            {
                var isReady = e.IsReady;
                DriveReadyMap.SetDriveReady(e.Name, isReady);

                item.IsReady = isReady;

                var driveName = isReady && !string.IsNullOrWhiteSpace(e.VolumeLabel) ? e.VolumeLabel : e.DriveType.ToDispString();
                item.DispName = string.Format(CultureInfo.InvariantCulture, "{0} ({1})", driveName, e.Name.TrimEnd('\\'));
            });

            return item;
        }

        /// <summary>
        /// DirectoryInfoからFolderItem作成
        /// </summary>
        public FolderItem? CreateFolderItem(DirectoryInfo? e)
        {
            if (e == null || !e.Exists) return null;

            var item = new FileFolderItem(_isOverlayEnabled)
            {
                Type = FolderItemType.Directory,
                Place = _place,
                Name = e.Name,
                TargetPath = new QueryPath(e.FullName),
                LastWriteTime = e.LastWriteTime,
                CreationTime = e.CreationTime,
                Length = -1,
                Attributes = FolderItemAttribute.Directory,
                IsReady = true
            };

            if (e.Attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                item.Attributes |= FolderItemAttribute.ReparsePoint;
            }

            return item;
        }


        public FolderItem? CreateFolderItem(FileInfo? e)
        {
            if (e == null || !e.Exists) return null;

            if (FileShortcut.IsShortcut(e.FullName))
            {
                var shortcut = new FileShortcut(e);
                if (shortcut.IsValid)
                {
                    return CreateFolderItem(shortcut);
                }
                else
                {
                    return null;
                }
            }

            var archiveType = ArchiveManager.Current.GetSupportedType(e.FullName);
            if (archiveType != ArchiveType.None)
            {
                var item = new FileFolderItem(_isOverlayEnabled)
                {
                    Type = FolderItemType.File,
                    Place = _place,
                    Name = e.Name,
                    TargetPath = new QueryPath(e.FullName),
                    LastWriteTime = e.LastWriteTime,
                    CreationTime= e.CreationTime,
                    Length = e.Length,
                    IsReady = true
                };

                if (archiveType == ArchiveType.PlaylistArchive)
                {
                    item.Type = FolderItemType.Playlist;
                    item.Attributes = FolderItemAttribute.Playlist;
                    item.Length = -1;
                }

                if (e.Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    item.Attributes |= FolderItemAttribute.ReparsePoint;
                }

                return item;
            }

            return null;
        }


        /// <summary>
        /// FileShortcutからFolderItem作成
        /// </summary>
        public FolderItem? CreateFolderItem(FileShortcut e)
        {
            if (e == null || !e.IsValid)
            {
                return null;
            }

            var item = CreateFolderItem(e.Target);
            if (item == null)
            {
                return null;
            }

            item.Type = (item.Type == FolderItemType.Directory)
                ? FolderItemType.DirectoryShortcut
                : item.Type == FolderItemType.Playlist ? FolderItemType.PlaylistShortcut : FolderItemType.FileShortcut;

            item.Place = _place;
            item.Name = Path.GetFileName(e.SourcePath);
            item.TargetPath = new QueryPath(e.SourcePath);
            item.Attributes = item.Attributes | FolderItemAttribute.Shortcut;

            return item;
        }


        /// <summary>
        /// アーカイブエントリから項目作成
        /// </summary>
        public FolderItem CreateFolderItem(ArchiveEntry entry, string? prefix)
        {
            string name = entry.EntryLastName;
            if (prefix != null)
            {
                name = entry.EntryFullName[prefix.Length..].TrimStart(LoosePath.Separators);
            }

            return new FileFolderItem(_isOverlayEnabled)
            {
                Type = FolderItemType.File,
                Place = _place,
                Name = name,
                TargetPath = new QueryPath(entry.SystemPath),
                LastWriteTime = entry.LastWriteTime,
                CreationTime = entry.CreationTime,
                Length = entry.Length,
                Attributes = FolderItemAttribute.ArchiveEntry,
                IsReady = true
            };
        }
    }


    public static class DriveReadyMap
    {
        private static readonly Dictionary<string, bool> _driveReadyMap = new();

        public static bool IsDriveReady(string driveName)
        {
            if (_driveReadyMap.TryGetValue(driveName, out bool isReady))
            {
                return isReady;
            }

            return true;
        }

        public static void SetDriveReady(string driveName, bool isReady)
        {
            _driveReadyMap[driveName] = isReady;
        }
    }
}
