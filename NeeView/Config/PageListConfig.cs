using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class PageListConfig : BindableBase, IHasPanelListItemStyle
    {
        private PanelListItemStyle _panelListItemStyle = PanelListItemStyle.Content;
        private PageNameFormat _format = PageNameFormat.Smart;
        private bool _showBookTitle = true;
        private bool _focusMainView;
        private bool _isGroupBy;
        private bool _isVisibleItemsCount = true;
        private bool _isVisibleSearchBox = true;


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



