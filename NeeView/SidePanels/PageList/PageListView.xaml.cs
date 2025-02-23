using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// PageListControl.xaml の相互作用ロジック
    /// </summary>
    public partial class PageListView : UserControl
    {
        private readonly PageListViewModel _vm;


        public PageListView(PageList model)
        {
            InitializeComponent();

            _vm = new PageListViewModel(model);
            this.DockPanel.DataContext = _vm;

            model.SearchBoxFocus += PageList_SearchBoxFocus;

            ContentDropManager.Current.SetDragDropEvent(this);

            Debug.WriteLine($"> Create: {nameof(PageListView)}");
        }


        /// <summary>
        /// 検索ボックスのフォーカス要求処理
        /// </summary>
        private void PageList_SearchBoxFocus(object? sender, EventArgs e)
        {
            this.SearchBox.FocusAsync();
        }

        /// <summary>
        /// 履歴戻るボタンコンテキストメニュー開く 前処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrevButton_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var menu = (sender as FrameworkElement)?.ContextMenu;
            if (menu == null) return;
            menu.ItemsSource = _vm.GetHistory(-1, 10);
        }

        /// <summary>
        /// 履歴進むボタンコンテキストメニュー開く 前処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextButton_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var menu = (sender as FrameworkElement)?.ContextMenu;
            if (menu == null) return;
            menu.ItemsSource = _vm.GetHistory(+1, 10);
        }

        #region UI Accessor

        public PageSortMode GetSortMode()
        {
            return (this.PageSortComboBox.SelectedValue is PageSortMode sortMode) ? sortMode : default;
        }

        public void SetSortMode(PageSortMode sortMode)
        {
            this.PageSortComboBox.SetCurrentValue(ComboBox.SelectedValueProperty, sortMode);
        }

        public PageNameFormat GetFormat()
        {
            return (this.FormatComboBox.SelectedValue is PageNameFormat format) ? format : default;
        }

        public void SetFormat(PageNameFormat format)
        {
            this.FormatComboBox.SetCurrentValue(ComboBox.SelectedValueProperty, format);
        }

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
