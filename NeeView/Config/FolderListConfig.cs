using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    [Equatable(IgnoreInheritedMembers = true)]
    public partial class FolderListConfig : BindableBase, IHasPanelListItemStyle
    {
        private FolderTreeLayout _folderTreeLayout = FolderTreeLayout.Left;
        private bool _isFolderTreeVisible = false;
        private PanelListItemStyle _panelListItemStyle = PanelListItemStyle.Content;

        /// <summary>
        /// リスト項目のスタイル
        /// </summary>
        [PropertyMember]
        public PanelListItemStyle PanelListItemStyle
        {
            get { return _panelListItemStyle; }
            set { if (_panelListItemStyle != value) { _panelListItemStyle = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// フォルダーツリーレイアウト(上部or左部)
        /// </summary>
        [PropertyMember]
        public FolderTreeLayout FolderTreeLayout
        {
            get { return _folderTreeLayout; }
            set { SetProperty(ref _folderTreeLayout, value); }
        }

        /// <summary>
        /// フォルダーツリーの表示
        /// </summary>
        [PropertyMember]
        public bool IsFolderTreeVisible
        {
            get { return _isFolderTreeVisible; }
            set { SetProperty(ref _isFolderTreeVisible, value); }
        }


        #region HiddenParameters

        /// <summary>
        /// フォルダーツリーエリアの幅
        /// </summary>
        [PropertyMapIgnore]
        public double FolderTreeAreaWidth { get; set { field = AppMath.Round(value); } } = 128.0;

        /// <summary>
        /// フォルダーツリーエリアの高さ
        /// </summary>
        [PropertyMapIgnore]
        public double FolderTreeAreaHeight { get; set { field = AppMath.Round(value); } } = 72.0;

        #endregion
    }

}


