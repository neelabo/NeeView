using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NeeView
{
    [LocalDebug]
    public partial class BookmarkPopupEdit : BindableBase
    {
        private string _name;

        public BookmarkPopupEdit(string path, TreeListNode<IBookmarkEntry>? parentMaybe, TreeListNode<IBookmarkEntry>? node)
        {
            Path = path;
            Node = node ?? (parentMaybe is not null ? BookmarkCollectionService.FindChildBookmark(new QueryPath(path), parentMaybe) : null);
            _name = Node?.Value.Name ?? BookTools.PathToBookName(path);
        }

        public string Path { get; }

        public TreeListNode<IBookmarkEntry>? Node { get; }

        [MemberNotNullWhen(true, nameof(Node))]
        public bool IsEdit => Node is not null;

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, ValidateBookmarkName(value)); }
        }


        private string ValidateBookmarkName(string? s)
        {
            var name = BookmarkTools.GetValidateName(s);
            if (string.IsNullOrEmpty(name))
            {
                return BookTools.PathToBookName(Path);
            }

            return name;
        }

        public void Add(TreeListNode<IBookmarkEntry>? parent)
        {
            if (parent is null) return;

            LocalDebug.WriteLine($"{Path} to {parent.Value.Name}");
            var node = BookmarkCollectionService.Add(new QueryPath(Path), parent, Name, true);
            OpenBookmarkPlace(parent, node);
        }

        public void Edit(TreeListNode<IBookmarkEntry>? parent)
        {
            if (parent is null) return;
            if (Node is null) return;

            if (Node.Parent != parent)
            {
                LocalDebug.WriteLine($"{Node.Parent?.Value.Name} to {parent.Value.Name}");
                BookmarkCollectionService.MoveToChild(Node, parent);
            }

            BookmarkCollectionService.Rename(Node, Name);
            OpenBookmarkPlace(parent, Node);
        }

        public void Remove(TreeListNode<IBookmarkEntry>? parent)
        {
            if (parent is null) return;
            if (Node is null) return;

            LocalDebug.WriteLine($"{Node.Value.Name} from {Node.Parent?.Value.Name}");
            BookmarkCollectionService.Remove(Node);
        }

        private void OpenBookmarkPlace(TreeListNode<IBookmarkEntry> place, TreeListNode<IBookmarkEntry>? node)
        {
            if (place is null) return;
            if (node is null) return;

            var query = place.CreateQuery();
            if (BookmarkFolderList.Current.Place == query)
            {
                var position = new FolderItemPosition(node);
                BookmarkFolderList.Current.RequestPlace(query, position, FolderSetPlaceOption.UpdateHistory | FolderSetPlaceOption.ResetKeyword);
            }
        }
    }

}
