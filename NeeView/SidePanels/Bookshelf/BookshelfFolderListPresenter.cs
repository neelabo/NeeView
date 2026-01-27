namespace NeeView
{
    public class BookshelfFolderListPresenter : FolderListPresenter
    {
        private readonly LazyEx<FolderListView> _folderListView;
        private readonly BookshelfFolderList _folderList;


        public BookshelfFolderListPresenter(LazyEx<FolderListView> folderListView, BookshelfFolderList folderList) : base(folderList)
        {
            _folderListView = folderListView;
            _folderList = folderList;

            if (_folderListView.IsValueCreated)
            {
                InitializeView(_folderListView.Value);
            }
            else
            {
                _folderListView.Created += (s, e) => InitializeView(e.Value);
            }
        }


        public FolderListView FolderListView => _folderListView.Value;

        public BookshelfFolderList FolderList => _folderList;


        private void InitializeView(FolderListView view)
        {
            base.InitializeView(view);
            view.SearchBoxFocusChanged += FolderListView_SearchBoxFocusChanged;
        }

        private void FolderListView_SearchBoxFocusChanged(object? sender, FocusChangedEventArgs e)
        {
            if (FolderListBox is null) return;

            // リストのフォーカス更新を停止
            FolderListBox.IsFocusEnabled = !e.IsFocused;
        }

    }
}
