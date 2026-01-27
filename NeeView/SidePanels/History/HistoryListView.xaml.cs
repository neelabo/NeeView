using System;
using System.Diagnostics;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// HistoryListView.xaml の相互作用ロジック
    /// </summary>
    public partial class HistoryListView : UserControl
    {
        private readonly HistoryListViewModel _vm;


        public HistoryListView(HistoryList model)
        {
            InitializeComponent();

            _vm = new HistoryListViewModel(model);
            this.DockPanel.DataContext = _vm;

            model.SearchBoxFocus += HistoryList_SearchBoxFocus;

            Debug.WriteLine($"> Create: {nameof(HistoryListView)}");
        }


        /// <summary>
        /// 検索ボックスのフォーカス要求処理
        /// </summary>
        private void HistoryList_SearchBoxFocus(object? sender, EventArgs e)
        {
            this.SearchBox.FocusAsync();
        }

        #region UI Accessor

        public void SetSearchBoxText(string text)
        {
            this.SearchBox.SetCurrentValue(SearchBox.TextProperty, text);
        }

        public string GetSearchBoxText()
        {
            return this.SearchBox.Text;
        }

        #endregion UI Accessor
    }
}
