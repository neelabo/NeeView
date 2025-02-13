using NeeView.Collections;
using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NeeView
{
    public class BookmarkFolderNode : FolderTreeNodeBase
    {
        public BookmarkFolderNode(TreeListNode<IBookmarkEntry> source, FolderTreeNodeBase? parent)
        {
            Source = source;
            Parent = parent;
        }

        public TreeListNode<IBookmarkEntry> BookmarkSource => (TreeListNode<IBookmarkEntry>?)Source ?? throw new InvalidOperationException();

        public override string Name { get => BookmarkSource.Value.Name ?? ""; set { } }

        public override string DisplayName { get => Name; set { } }

        public override IImageSourceCollection Icon => FileIconCollection.Current.CreateDefaultFolderIcon();

        public string Path => Parent is BookmarkFolderNode parent ? LoosePath.Combine(parent.Path, Name) : Name;


        [NotNull]
        public override ObservableCollection<FolderTreeNodeBase>? Children
        {
            get
            {
                if (_children == null)
                {
                    _children = new ObservableCollection<FolderTreeNodeBase>(BookmarkSource.Children
                        .Where(e => e.Value is BookmarkFolder)
                        .OrderBy(e => e.Value, new HasNameComparer())
                        .Select(e => new BookmarkFolderNode(e, this)));
                }
                return _children;
            }
            set
            {
                SetProperty(ref _children, value);
            }
        }

        public override string GetRenameText()
        {
            return this.Name;
        }

        public override bool CanRename()
        {
            return true;
        }

        public override async Task<bool> RenameAsync(string name)
        {
            return await Task.FromResult(Rename(name));
        }

        public bool Rename(string name)
        {
            if (!CanRename()) return false;
            if (this.Name == name) return false;

            BookmarkCollectionService.Rename(this.BookmarkSource, name);
            return true;
        }
    }

}
