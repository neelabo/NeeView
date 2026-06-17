using CommunityToolkit.Mvvm.ComponentModel;
using NeeView.Collections.Generic;
using NeeView.Properties;
using NeeView.Windows.Property;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public static class BookmarkFolderTools
    {
        public static void ShowPropertyDialog(Window? owner, TreeListNode<IBookmarkEntry> node)
        {
            var context = new BookmarkFolderProperty(node);
            var document = new PropertyDocument(context);

            var dialog = new PropertyDialog();
            dialog.Title = TextResources.GetFormatString("PropertyDialog.Title", TextResources.GetString("Word.BookmarkFolder"));
            dialog.Document = document;
            dialog.Owner = owner;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();

            if (result == true)
            {
                context.Decide();
            }
        }
    }


    public partial class BookmarkFolderProperty : ObservableObject
    {
        private readonly TreeListNode<IBookmarkEntry> _node;
        private readonly BookmarkFolder _bookmarkFolder;
        private string _name;

        public BookmarkFolderProperty(TreeListNode<IBookmarkEntry> node)
        {
            Debug.Assert(node.Value is BookmarkFolder);

            _node = node;
            _bookmarkFolder = (BookmarkFolder)_node.Value;
            _name = _bookmarkFolder.Name ?? "";
            TagColor = _bookmarkFolder.Color ?? TagDefaultColorFactory.DefaultColor;
        }

        [PropertyMember(Name = "Word.Name")]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, _name = string.IsNullOrEmpty(value.Trim()) ? _name : value.Trim());
        }

        [ObservableProperty]
        [PropertyColor(true, DefaultColorFactory = typeof(TagDefaultColorFactory), Name = "Word.TagColor")]
        public partial Color TagColor { get; set; }

        public void Decide()
        {
            BookmarkCollectionService.Rename(_node, Name);

            Color? color = TagColor != TagDefaultColorFactory.DefaultColor ? TagColor : null;
            BookmarkCollectionService.SetColor(_node, color);
        }
    }


    public class TagDefaultColorFactory : IColorFactory
    {
        public Color CreateColor()
        {
            return DefaultColor;
        }

        public static Color DefaultColor => (App.Current.Resources["Tag.Background"] as SolidColorBrush)?.Color ?? default;
    }

}
