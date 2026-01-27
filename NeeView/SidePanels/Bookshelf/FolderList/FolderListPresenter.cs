namespace NeeView
{
    public interface IHasFolderListBox
    {
        void SetFolderListBoxContent(FolderListBox content);
    }

    public class FolderListPresenter
    {
        private readonly FolderList _folderList;
        private readonly FolderListBoxViewModel _folderListBoxViewModel;
        private IHasFolderListBox? _folderListView;
        private LazyEx<FolderListBox> _folderListBox;


        public FolderListPresenter(FolderList folderList)
        {
            _folderList = folderList;
            _folderList.FolderListConfig.AddPropertyChanged(nameof(FolderListConfig.PanelListItemStyle), (s, e) => UpdateFolderListBox());

            _folderListBoxViewModel = new FolderListBoxViewModel(folderList);

            _folderListBox = new(() => new FolderListBox(_folderListBoxViewModel));
            _folderListBox.Created += (s, e) => AttachFolderListBox();
        }


        public FolderListBox? FolderListBox => _folderListBox.Value;


        public void InitializeView(IHasFolderListBox folderListView)
        {
            _folderListView = folderListView;
            UpdateFolderListBox(false);
        }

        public void UpdateFolderListBox(bool rebuild = true)
        {
            if (rebuild)
            {
                _folderListBox = new(() => new FolderListBox(_folderListBoxViewModel));
            }
            AttachFolderListBox();
        }

        private void AttachFolderListBox()
        {
            _folderListView?.SetFolderListBoxContent(_folderListBox.Value);
        }

        public void Refresh()
        {
            FolderListBox?.Refresh();
        }

        public void FocusAtOnce()
        {
            _folderList.FocusAtOnce();
            FolderListBox?.FocusSelectedItem(false);
        }
    }
}
