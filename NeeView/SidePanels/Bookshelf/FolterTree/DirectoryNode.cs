using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class DirectoryNode : FolderTreeNodeDelayBase
    {
        public DirectoryNode(string name, FolderTreeNodeBase? parent)
        {
            _name = name;
            Source = name;

            Parent = parent;
        }

        private string _name;
        public override string Name
        {
            get { return _name; }
            set
            {
                if (SetProperty(ref _name, value))
                {
                    Source = _name;
                    RaisePropertyChanged(nameof(DisplayName));
                }
            }
        }

        public override string DisplayName { get => Name; set { } }

        public override IImageSourceCollection Icon => FileIconCollection.Current.CreateFileIcon(Path, IO.FileIconType.Directory,  false, false);

        public virtual string Path => Parent is DirectoryNode parent ? LoosePath.Combine(parent.Path, Name) : Name;

        public DriveDirectoryNode? Drive => this is DriveDirectoryNode drive ? drive : Parent is DirectoryNode parent ? parent.Drive : null;

        // 遅延生成を親から継承する
        public bool IsDelayCreateInheritance { get; set; } = true;


        protected override void OnParentChanged(object sender, EventArgs e)
        {
            if (Parent == null) return;

            if (Parent is DirectoryNode parent && parent.IsDelayCreateInheritance)
            {
                IsDelayCreation = parent.IsDelayCreation;
            }
        }

        protected virtual void OnException(DirectoryNode sender, NotifyCrateDirectoryChildrenExcepionEventArgs e)
        {
            (Parent as DirectoryNode)?.OnException(sender, e);
        }

        protected virtual void OnChildrenChanged(DirectoryNode sender, EventArgs e)
        {
        }

        public override void CreateChildren(bool isForce)
        {
            var directory = new DirectoryInfo(Path);

            try
            {
                Children = new ObservableCollection<FolderTreeNodeBase>(directory.GetDirectories()
                    .Where(e => FileIOProfile.Current.IsFileValid(e.Attributes))
                    .OrderBy(e => e.Name, NaturalSort.Comparer)
                    .Select(e => new DirectoryNode(e.Name, this)));

                OnChildrenChanged(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                IsDelayCreation = true;

                if (isForce)
                {
                    var isDriveRefresh = !(ex is UnauthorizedAccessException || ex is System.Security.SecurityException);
                    OnException(this, new NotifyCrateDirectoryChildrenExcepionEventArgs(ex, isDriveRefresh));

                    Children = new ObservableCollection<FolderTreeNodeBase>();
                }
            }
        }


        public override FolderTreeNodeBase? FindChild(object? source)
        {
            return _children?.FirstOrDefault(e => e.Name.Equals(source));
        }

        public void Rename(string oldName, string name)
        {
            if (_children == null) return;

            ////Debug.WriteLine($"RENAME: " + oldName + " -> " + name);

            var directory = _children.Cast<DirectoryNode>().FirstOrDefault(e => e.Name == oldName);
            if (directory != null)
            {
                directory.Name = name;
                Sort(directory);
            }
        }

    }


    /// <summary>
    /// 子ノード生成時の例外引数
    /// </summary>
    public class NotifyCrateDirectoryChildrenExcepionEventArgs : EventArgs
    {
        public NotifyCrateDirectoryChildrenExcepionEventArgs(Exception exception, bool isRefresh)
        {
            Exception = exception;
            IsRefresh = isRefresh;
        }

        public Exception Exception { get; set; }
        public bool IsRefresh { get; set; }
    }

}
