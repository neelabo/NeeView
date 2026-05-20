using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeeView.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;

namespace NeeView
{
    public partial class TagColorDialogViewModel : ObservableObject
    {
        private readonly TreeListNode<IBookmarkEntry> _node;
        private readonly BookmarkFolder _bookmarkFolder;

        public TagColorDialogViewModel(TreeListNode<IBookmarkEntry> node)
        {
            Debug.Assert(node.Value is BookmarkFolder);

            _node = node;
            _bookmarkFolder = (BookmarkFolder)_node.Value;
            Color = _bookmarkFolder.Color ?? DefaultColor;
        }

        [ObservableProperty]
        public partial Color Color { get; set; }

        public Color DefaultColor => (App.Current.Resources["Tag.Background"] as SolidColorBrush)?.Color ?? default;


        [RelayCommand]
        public void Reset()
        {
            Color = DefaultColor;
        }

        [RelayCommand]
        public void Decide()
        {
            Color? color = Color != DefaultColor ? Color : null;
            BookmarkCollectionService.SetColor(_node, color);
        }
    }
}
