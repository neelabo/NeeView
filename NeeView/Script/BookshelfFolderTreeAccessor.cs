using System;

namespace NeeView
{
    [WordNodeMember]
    public class BookshelfFolderTreeAccessor
    {
        private readonly FolderTreeModel _model;

        public BookshelfFolderTreeAccessor()
        {
            _model = AppDispatcher.Invoke(() => FolderPanel.Current.FolderTreeModel);
            QuickAccessNode = new QuickAccessFolderNodeAccessor(_model, _model.RootQuickAccess ?? throw new InvalidOperationException());
            DirectoryNode = new DirectoryNodeAccessor(_model, _model.RootDirectory ?? throw new InvalidOperationException());
            BookmarkNode = new BookmarkFolderNodeAccessor(_model, _model.RootBookmarkFolder ?? throw new InvalidOperationException());
        }


        [WordNodeMember]
        public QuickAccessFolderNodeAccessor QuickAccessNode { get; }

        [WordNodeMember]
        public DirectoryNodeAccessor DirectoryNode { get; }

        [WordNodeMember]
        public BookmarkFolderNodeAccessor BookmarkNode { get; }

        [WordNodeMember]
        public NodeAccessor? SelectedItem
        {
            get { return _model.SelectedItem is not null ? FolderNodeAccessorFactory.Create(_model, _model.SelectedItem) : null; }
            set { AppDispatcher.Invoke(() => _model.SetSelectedItem(value?.Node)); }
        }


        [WordNodeMember]
        public void Expand(string path)
        {
            AppDispatcher.Invoke(() => _model.SyncDirectory(path, true));
        }
    }
}
