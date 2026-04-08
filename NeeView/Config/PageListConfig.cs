using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class PageListConfig : BindableBase, IHasPanelListItemStyle
    {
        [DefaultEquality] private PanelListItemStyle _panelListItemStyle = PanelListItemStyle.Content;
        [DefaultEquality] private PageNameFormat _format = PageNameFormat.Smart;
        [DefaultEquality] private bool _showBookTitle = true;
        [DefaultEquality] private bool _focusMainView;
        [DefaultEquality] private bool _isGroupBy;
        [DefaultEquality] private bool _isVisibleItemsCount = true;
        [DefaultEquality] private bool _isVisibleSearchBox = true;


        /// <summary>
        /// ページリストのリスト項目表示形式
        /// </summary>
        [PropertyMember]
        public PanelListItemStyle PanelListItemStyle
        {
            get { return _panelListItemStyle; }
            set { SetProperty(ref _panelListItemStyle, value); }
        }

        /// <summary>
        /// ページ名表示形式
        /// </summary>
        [PropertyMember]
        public PageNameFormat Format
        {
            get { return _format; }
            set { SetProperty(ref _format, value); }
        }

        /// <summary>
        /// ブック名表示
        /// </summary>
        [PropertyMember]
        public bool ShowBookTitle
        {
            get { return _showBookTitle; }
            set { SetProperty(ref _showBookTitle, value); }
        }

        /// <summary>
        /// ページ選択でメインビューにフォーカスを移す 
        /// </summary>
        [PropertyMember]
        public bool FocusMainView
        {
            get { return _focusMainView; }
            set { SetProperty(ref _focusMainView, value); }
        }

        /// <summary>
        /// グループ表示
        /// </summary>
        [PropertyMember]
        public bool IsGroupBy
        {
            get { return _isGroupBy; }
            set { SetProperty(ref _isGroupBy, value); }
        }

        /// <summary>
        /// コレクションアイテム数の表示
        /// </summary>
        [PropertyMember]
        public bool IsVisibleItemsCount
        {
            get { return _isVisibleItemsCount; }
            set { SetProperty(ref _isVisibleItemsCount, value); }
        }

        /// <summary>
        /// 検索ボックスを表示
        /// </summary>
        public bool IsVisibleSearchBox
        {
            get { return _isVisibleSearchBox; }
            set { SetProperty(ref _isVisibleSearchBox, value); }
        }

    }

}



