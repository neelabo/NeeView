//#define LOCAL_DEBUG
using NeeLaboratory.Generators;
using NeeLaboratory.IO.Nodes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace NeeView
{
    [LocalDebug]
    public partial class FileItemTree : FileTree
    {
        public FileItemTree(string path, EnumerationOptions enumerationOptions) : base(path, enumerationOptions)
        {
        }


        public event EventHandler<FileTreeContentChangedEventArgs>? ContentAdded;
        public event EventHandler<FileTreeContentChangedEventArgs>? ContentRemoved;
        public event EventHandler<FileTreeContentChangedEventArgs>? ContentChanged;


        public class FileTreeContentChangedEventArgs : EventArgs
        {
            public FileTreeContentChangedEventArgs(FileItem fileItem) : this(fileItem, null)
            {
            }
            
            public FileTreeContentChangedEventArgs(FileItem fileItem, FileItem? oldFileItem)
            {
                FileItem = fileItem;
                OldFileItem = oldFileItem;
            }

            public FileItem FileItem { get; }
            public FileItem? OldFileItem { get; }
        }


        protected override void AttachContent(Node? node, FileSystemInfo file)
        {
            if (node == null) return;

            var fileItem = new FileItem(file);
            node.Content = fileItem;
            LocalDebug.WriteLine($"Add: {fileItem}");
            ContentAdded?.Invoke(this, new FileTreeContentChangedEventArgs(fileItem));
        }

        protected override void DetachContent(Node? node)
        {
            if (node == null) return;

            var fileItem = node.Content as FileItem;
            node.Content = null;
            if (fileItem is not null)
            {
                LocalDebug.WriteLine($"Remove: {fileItem}");
                ContentRemoved?.Invoke(this, new FileTreeContentChangedEventArgs(fileItem));
            }
        }

        protected override void UpdateContent(Node? node, bool isRecursive)
        {
            if (node is null) return;

            if (isRecursive)
            {
                foreach (var n in node.Walk())
                {
                    UpdateContent(n);
                }
            }
            else
            {
                UpdateContent(node);
            }
        }

        private void UpdateContent(Node? node)
        {
            if (node == null) return;

            var oldFileItem = node.Content as FileItem;
            var fileItem = new FileItem(CreateFileInfo(node.FullName));
            node.Content = fileItem;
            ContentChanged?.Invoke(this, new FileTreeContentChangedEventArgs(fileItem, oldFileItem));
        }

        public IEnumerable<FileItem> CollectFileItems()
        {
            return Trunk.WalkChildren().Select(e => GetFileItem(e));
        }

        private FileItem GetFileItem(Node node)
        {
            if (node.Content is FileItem fileItem)
            {
                return fileItem;
            }

            var info = CreateFileInfo(node.FullName);
            fileItem = new FileItem(info);
            node.Content = fileItem;
            return fileItem;
        }

    }
}
