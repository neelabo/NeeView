using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using NeeView.Windows;

namespace NeeView
{
    public partial class BookmarkListView : UserControl, IHasFolderListBox
    {
        private readonly BookmarkListViewModel? _vm;


        public BookmarkListView(BookmarkFolderList model)
        {
            InitializeComponent();

            this.FolderTree.Model = new FolderTreeModel(model, FolderTreeCategory.BookmarkFolder);

            _vm = new BookmarkListViewModel(model);
            this.Root.DataContext = _vm;

            model.SearchBoxFocus += FolderList_SearchBoxFocus;
            model.FolderTreeFocus += FolderList_FolderTreeFocus;

            Debug.WriteLine($"> Create: {nameof(BookmarkListView)}");
        }


        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            if (_vm is null) return;

            base.OnDpiChanged(oldDpi, newDpi);
            _vm.SetDpiScale(newDpi);
        }

        /// <summary>
        /// フォルダーツリーへのフォーカス要求
        /// </summary>
        private void FolderList_FolderTreeFocus(object? sender, EventArgs e)
        {
            if (_vm is null) return;
            if (!_vm.Model.FolderListConfig.IsFolderTreeVisible) return;

            this.FolderTree.FocusSelectedItem();
        }

        /// <summary>
        /// 検索ボックスのフォーカス要求処理
        /// </summary>
        private void FolderList_SearchBoxFocus(object? sender, EventArgs e)
        {
            this.SearchBox.FocusAsync();
        }

        private void BookmarkListView_IsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
        }

        private void Root_KeyDown(object? sender, KeyEventArgs e)
        {
            if (_vm is null) return;

            // このパネルで使用するキーのイベントを止める
            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Up || e.Key == Key.Down || (_vm.IsLRKeyEnabled() && (e.Key == Key.Left || e.Key == Key.Right)) || e.Key == Key.Return || e.Key == Key.Delete)
                {
                    e.Handled = true;
                }
            }
        }

        private void Grid_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (_vm is null) return;

            if (e.WidthChanged)
            {
                _vm.Model.AreaWidth = e.NewSize.Width;
            }
            if (e.HeightChanged)
            {
                _vm.Model.AreaHeight = e.NewSize.Height;
            }
        }

        public void SetFolderListBoxContent(FolderListBox content)
        {
            this.ListBoxContent.Content = content;
        }

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var data = CreatePlaceDataObject();
                if (data == null)
                {
                    return;
                }
                Clipboard.SetDataObject(data);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void CopyAsTextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_vm is null) return;

            try
            {
                Clipboard.SetText(_vm.Model.Place?.SimplePath ?? "");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private DataObject? CreatePlaceDataObject()
        {
            if (_vm is null)
            {
                return null;
            }

            var place = _vm.Model.Place;
            if (place == null)
            {
                return null;
            }

            var data = new DataObject();
            data.SetQueryPathAndFile(place);
            return data;
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
