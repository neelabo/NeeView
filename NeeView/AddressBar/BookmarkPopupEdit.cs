using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NeeView
{
    [LocalDebug]
    public partial class BookmarkPopupEdit : BindableBase
    {
        public BookmarkPopupEdit(string path, TreeListNode<IBookmarkEntry>? parentMaybe)
        {
            Path = path;
            Node = parentMaybe is not null ? BookmarkCollectionService.FindChildBookmark(new QueryPath(path), parentMaybe) : null;
        }

        public string Path { get; }

        public TreeListNode<IBookmarkEntry>? Node { get; }

        [MemberNotNullWhen(true, nameof(Node))]
        public bool IsEdit => Node is not null;


        /// <summary>
        /// 編集を適用
        /// </summary>
        /// <param name="parent">操作対象のブックマークフォルダ―</param>
        /// <param name="result">true:追加もしくは移動, false:削除, null:編集でなければ追加</param>
        public void Apply(TreeListNode<IBookmarkEntry>? parent, bool? result)
        {
            if (parent is null) return;

            if (IsEdit)
            {
                if (result == true)
                {
#if true
                    // 編集でも追加
                    var item = AddToChild(Path, parent);
                    OpenBookmarkPlace(parent);
#else
                    // 編集では移動
                    MoveToChild(Node, parent);
#endif
                }
                else if (result == false)
                {
                    if (parent == Node)
                    {
                        Remove(Node);
                    }
                    else
                    {
                        var node = BookmarkCollectionService.FindChildBookmark(new QueryPath(Path), parent);
                        if (node is not null)
                        {
                            Remove(Node);
                        }
                    }
                }
            }
            else
            {
                if (result != false)
                {
                    var item = AddToChild(Path, parent);
                    OpenBookmarkPlace(parent);
                }
            }
        }

        private void OpenBookmarkPlace(TreeListNode<IBookmarkEntry> place)
        {
            var position = new FolderItemPosition(new QueryPath(Path));
            BookmarkFolderList.Current.RequestPlace(place.CreateQuery(), position, FolderSetPlaceOption.UpdateHistory | FolderSetPlaceOption.ResetKeyword);
        }

        private TreeListNode<IBookmarkEntry>? AddToChild(string path, TreeListNode<IBookmarkEntry> parent)
        {
            LocalDebug.WriteLine($"{path} to {parent.Value.Name}");
            return BookmarkCollectionService.Add(new QueryPath(Path), parent);
        }

        private void MoveToChild(TreeListNode<IBookmarkEntry> node, TreeListNode<IBookmarkEntry> parent)
        {
            if (parent is null) return;

            LocalDebug.WriteLine($"{node.Parent?.Value.Name} to {parent.Value.Name}");
            BookmarkCollectionService.MoveToChild(node, parent);
        }

        private void Remove(TreeListNode<IBookmarkEntry>? node)
        {
            if (node is null) return;

            LocalDebug.WriteLine($"{node.Value.Name} from {node.Parent?.Value.Name}");
            BookmarkCollectionService.Remove(node);
        }
    }

}
