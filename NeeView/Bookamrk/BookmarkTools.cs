using CommunityToolkit.Mvvm.ComponentModel;
using NeeView.Collections.Generic;
using NeeView.Properties;
using NeeView.Windows.Property;
using System.Diagnostics;
using System.Windows;

namespace NeeView
{
    public static class BookmarkTools
    {
        public static string GetValidateName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "";
            }

            return name.Trim().Replace('/', '_').Replace('\\', '_');
        }


        public static bool ShowPropertyDialog(Window? owner, TreeListNode<IBookmarkEntry> node)
        {
            var context = new BookmarkProperty(node);
            var document = new PropertyDocument(context);

            var dialog = new PropertyDialog();
            dialog.Title = TextResources.GetFormatString("PropertyDialog.Title", TextResources.GetString("Word.Bookmark"));
            dialog.Document = document;
            dialog.Owner = owner;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();

            if (result == true)
            {
                context.Decide();
                return true;
            }

            return false;
        }
    }


    public partial class BookmarkProperty : ObservableObject
    {
        private readonly TreeListNode<IBookmarkEntry> _node;
        private readonly Bookmark _bookmark;
        private string _name;

        public BookmarkProperty(TreeListNode<IBookmarkEntry> node)
        {
            Debug.Assert(node.Value is Bookmark);

            _node = node;
            _bookmark = (Bookmark)_node.Value;
            _name = _bookmark.Name ?? "";
        }

        [PropertyMember(Name = "Word.Name")]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, _name = string.IsNullOrEmpty(value.Trim()) ? _bookmark.Unit.Memento.Name : value.Trim());
        }

        public void Decide()
        {
            BookmarkCollectionService.Rename(_node, Name);
        }
    }

}
