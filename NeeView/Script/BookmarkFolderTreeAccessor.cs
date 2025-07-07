using System;

namespace NeeView
{
    [WordNodeMember]
    public class BookmarkFolderTreeAccessor
    {
        private readonly FolderTreeModel _model;

        public BookmarkFolderTreeAccessor()
        {
            _model = AppDispatcher.Invoke(() => BookmarkPanel.Current.FolderTreeModel);
            BookmarkNode = new BookmarkFolderNodeAccessor(_model, _model.RootBookmarkFolder ?? throw new InvalidOperationException());
        }


        [WordNodeMember]
        public BookmarkFolderNodeAccessor BookmarkNode { get; }

        [WordNodeMember]
        public NodeAccessor? SelectedItem
        {
            get { return _model.SelectedItem is not null ? FolderNodeAccessorFactory.Create(_model, _model.SelectedItem) : null; }
            set { AppDispatcher.Invoke(() => _model.SetSelectedItem(value?.Node)); }
        }
    }

}
