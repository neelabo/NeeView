using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace NeeView
{
    public class RootDirectoryNode : DirectoryNode
    {
        public RootDirectoryNode(FolderTreeNodeBase parent) : base("", parent)
        {
            Parent = parent;

            SystemDeviceWatcher.Current.DriveChanged += WindowMessage_DriveChanged;
            SystemDeviceWatcher.Current.MediaChanged += WindowMessage_MediaChanged;
            SystemDeviceWatcher.Current.DirectoryChanged += WindowMessage_DirectoryChanged;

            Icon = new SingleImageSourceCollection(ResourceTools.GetElementResource<ImageSource>(MainWindow.Current, "ic_desktop_windows_24px"));
        }


        public override string Name { get => QueryScheme.File.ToSchemeString(); set { } }

        public override string DisplayName { get => "PC"; set { } }

        public override IImageSourceCollection Icon { get; }



        public void Refresh()
        {
            this.CreateChildren(true);
            this.IsExpanded = true;
        }

        public void RefreshDriveChildren()
        {
            if (_children != null)
            {
                foreach (var child in _children)
                {
                    child.RefreshChildren();
                }
            }
        }

        public override void CreateChildren(bool isForce)
        {
            try
            {
                Children = new ObservableCollection<FolderTreeNodeBase>(DriveInfo.GetDrives()
                    .Select(e => new DriveDirectoryNode(e, this)));
            }
            catch (Exception ex)
            {
                ToastService.Current.Show("FolderList", new Toast(ex.Message, null, ToastIcon.Error));
            }
        }

        private void WindowMessage_DriveChanged(object? sender, DriveChangedEventArgs e)
        {
            if (_children == null) return;

            ////Debug.WriteLine($"DriveChange: {e.Name}, {e.IsAlive}");

            AppDispatcher.BeginInvoke(() =>
            {
                try
                {
                    var driveInfo = CreateDriveInfo(e.Name);

                    if (e.IsAlive)
                    {
                        if (driveInfo != null)
                        {
                            AddDrive(driveInfo);
                        }
                    }
                    else
                    {
                        var name = e.Name.TrimEnd(LoosePath.Separators);

                        var drive = _children.Cast<DriveDirectoryNode>().FirstOrDefault(d => d.Name == name);
                        if (drive != null)
                        {
                            if (driveInfo == null)
                            {
                                _children.Remove(drive);
                            }
                            else
                            {
                                drive.Refresh();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            });
        }

        private void AddDrive(DriveInfo driveInfo)
        {
            if (driveInfo == null) return;
            if (_children == null) return;

            var name = driveInfo.Name.TrimEnd(LoosePath.Separators);

            var drive = _children.Cast<DriveDirectoryNode>().FirstOrDefault(d => d.Name == name);
            if (drive != null)
            {
                drive.Refresh();
                return;
            }

            for (int index = 0; index < _children.Count; ++index)
            {
                if (string.Compare(name, _children[index].Name, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    _children.Insert(index, new DriveDirectoryNode(driveInfo, this));
                    break;
                }

                if (index == _children.Count - 1)
                {
                    _children.Add(new DriveDirectoryNode(driveInfo, this));
                    break;
                }
            }
        }

        private static DriveInfo? CreateDriveInfo(string name)
        {
            Debug.Assert(name.EndsWith("\\", StringComparison.Ordinal));

            if (System.IO.Directory.GetLogicalDrives().Contains(name))
            {
                return new DriveInfo(name);
            }

            return null;
        }

        private void WindowMessage_MediaChanged(object? sender, MediaChangedEventArgs e)
        {
            if (_children == null) return;

            ////Debug.WriteLine($"MediaChange: {e.Name}, {e.IsAlive}");

            AppDispatcher.BeginInvoke(() =>
            {
                try
                {
                    var name = e.Name.TrimEnd(LoosePath.Separators);

                    var drive = _children.Cast<DriveDirectoryNode>().FirstOrDefault(d => d.Name == name);
                    if (drive == null)
                    {
                        return;
                    }

                    drive.Refresh();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            });
        }

        private void WindowMessage_DirectoryChanged(object? sender, DirectoryChangedEventArgs e)
        {
            AppDispatcher.BeginInvoke(() =>
            {
                try
                {
                    switch (e.ChangeType)
                    {
                        case DirectoryChangeType.Created:
                            Directory_Created(e.FullPath);
                            break;
                        case DirectoryChangeType.Deleted:
                            Directory_Deleted(e.FullPath);
                            break;
                        case DirectoryChangeType.Renamed:
                            if (e.OldFullPath is null) throw new InvalidOperationException("e.OldFullPath must not be null at Renamed");
                            Directory_Renamed(e.OldFullPath, e.FullPath);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            });
        }

        private void Directory_Created(string fullPath)
        {
            ////Debug.WriteLine("Create: " + fullPath);

            var directory = LoosePath.GetDirectoryName(fullPath);

            var parent = GetDirectoryNode(directory);
            if (parent != null)
            {
                var name = LoosePath.GetFileName(fullPath);
                var node = new DirectoryNode(name, null);
                AppDispatcher.BeginInvoke(() => parent.Add(node));
            }
            else
            {
                ////Debug.WriteLine("Skip create");
            }
        }

        private void Directory_Deleted(string fullPath)
        {
            ////Debug.WriteLine("Delete: " + fullPath);

            var directory = LoosePath.GetDirectoryName(fullPath);

            var parent = GetDirectoryNode(directory);
            if (parent != null)
            {
                var name = LoosePath.GetFileName(fullPath);
                AppDispatcher.BeginInvoke(() => parent.Remove(name));
            }
            else
            {
                ////Debug.WriteLine("Skip delete");
            }
        }

        private void Directory_Renamed(string oldFullPath, string fullPath)
        {
            ////Debug.WriteLine("Rename: " + oldFullPath + " -> " + fullPath);

            var directory = LoosePath.GetDirectoryName(oldFullPath);

            var parent = GetDirectoryNode(directory);
            if (parent != null)
            {
                var oldName = LoosePath.GetFileName(oldFullPath);
                var name = LoosePath.GetFileName(fullPath);
                AppDispatcher.BeginInvoke(() => parent.Rename(oldName, name));
            }
            else
            {
                ////Debug.WriteLine("Skip rename");
            }
        }

        private DirectoryNode? GetDirectoryNode(string path)
        {
            return GetFolderTreeNode(path, false, false) as DirectoryNode;
        }
    }
}
