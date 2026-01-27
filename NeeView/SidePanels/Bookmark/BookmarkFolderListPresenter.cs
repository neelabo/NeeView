namespace NeeView
{
    public class BookmarkFolderListPresenter : FolderListPresenter
    {
        private readonly LazyEx<BookmarkListView> _folderListView;
        private readonly BookmarkFolderList _folderList;

        public BookmarkFolderListPresenter(LazyEx<BookmarkListView> folderListView, BookmarkFolderList folderList) : base(folderList)
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


        public BookmarkListView BookmarkListView => _folderListView.Value;

        public BookmarkFolderList BookmarkFolderList => _folderList;


        private void InitializeView(BookmarkListView view)
        {
            base.InitializeView(view);
            //view.SearchBoxFocusChanged += BookmarkListView_SearchBoxFocusChanged;
        }
    }
}
