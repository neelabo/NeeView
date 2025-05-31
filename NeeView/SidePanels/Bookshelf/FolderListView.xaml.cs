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
using NeeView.Windows;
using System.Threading;
using NeeView.Data;
using System.Diagnostics.CodeAnalysis;

namespace NeeView
{
    public class FocusChangedEventArgs : EventArgs
    {
        public FocusChangedEventArgs(bool isFocused)
        {
            IsFocused = isFocused;
        }

        public bool IsFocused { get; set; }
    }


    /// <summary>
    /// FolderListControl.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderListView : UserControl, IHasFolderListBox
    {
        private readonly FolderListViewModel _vm;


        public FolderListView(BookshelfFolderList model)
        {
            InitializeComponent();

            this.FolderTree.Model = new FolderTreeModel(model, FolderTreeCategory.All);

            _vm = new FolderListViewModel(model);
            this.DockPanel.DataContext = _vm;

            model.SearchBoxFocus += FolderList_SearchBoxFocus;
            model.FolderTreeFocus += FolderList_FolderTreeFocus;

            this.SearchBox.IsKeyboardFocusedChanged +=
                (s, e) => SearchBoxFocusChanged?.Invoke(this, new FocusChangedEventArgs((bool)e.NewValue));

            Debug.WriteLine($"> Create: {nameof(FolderListView)}");
        }


        public event EventHandler<FocusChangedEventArgs>? SearchBoxFocusChanged;


        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);
            _vm.Dpi = newDpi.DpiScaleX;
        }

        /// <summary>
        /// フォルダーツリーへのフォーカス要求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderList_FolderTreeFocus(object? sender, EventArgs e)
        {
            if (!_vm.Model.FolderListConfig.IsFolderTreeVisible) return;

            this.FolderTree.FocusSelectedItem();
        }

        /// <summary>
        /// 検索ボックスのフォーカス要求処理
        /// </summary>
        private void FolderList_SearchBoxFocus(object? sender, EventArgs e)
        {
            this.SearchBox?.FocusAsync();
        }

        /// <summary>
        /// 履歴戻るボタンコンテキストメニュー開く 前処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderPrevButton_ContextMenuOpening(object? sender, ContextMenuEventArgs e)
        {
            var menu = (sender as FrameworkElement)?.ContextMenu;
            if (menu == null) return;
            menu.ItemsSource = _vm.GetPreviousHistory();
        }

        /// <summary>
        /// 履歴進むボタンコンテキストメニュー開く 前処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderNextButton_ContextMenuOpening(object? sender, ContextMenuEventArgs e)
        {
            var menu = (sender as FrameworkElement)?.ContextMenu;
            if (menu == null) return;
            menu.ItemsSource = _vm.GetNextHistory();
        }

        private void FolderListView_IsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
        }

        private void Grid_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                _vm.Model.AreaWidth = e.NewSize.Width;
            }
            if (e.HeightChanged)
            {
                _vm.Model.AreaHeight = e.NewSize.Height;
            }
        }

        #region DragDrop

        private readonly DragDropGhost _ghost = new();
        private bool _isButtonDown;
        private Point _buttonDownPos;

        private void PlaceIcon_MouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
        {
            var element = sender as UIElement;
            _buttonDownPos = e.GetPosition(element);
            _isButtonDown = true;
        }

        private void PlaceIcon_MouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
        {
            _isButtonDown = false;
        }

        private void PlaceIcon_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!_isButtonDown)
            {
                return;
            }

            if (e.LeftButton == MouseButtonState.Released)
            {
                _isButtonDown = false;
                return;
            }

            var element = sender as UIElement;

            var pos = e.GetPosition(element);
            if (DragDropHelper.IsDragDistance(pos, _buttonDownPos))
            {
                _isButtonDown = false;

                var data = CreatePlaceDataObject();
                if (data == null)
                {
                    return;
                }

                _ghost.Attach(this.PlaceBar, new Point(24, 24));
                DragDrop.DoDragDrop(element, data, DragDropEffects.Copy);
                _ghost.Detach();
            }
        }

        private DataObject? CreatePlaceDataObject()
        {
            var place = _vm.Model.Place;
            if (!CanDragPlace(place))
            {
                return null;
            }

            var data = new DataObject();
            data.SetQueryPathAndFile(place);
            return data;
        }

        private bool CanDragPlace([NotNullWhen(true)] QueryPath? place)
        {
            if (place == null)
            {
                return false;
            }
            if (place.Scheme != QueryScheme.File && place.Scheme != QueryScheme.Bookmark)
            {
                return false;
            }
            if (place.Scheme == QueryScheme.File && string.IsNullOrEmpty(place.Path))
            {
                return false;
            }
            return true;
        }

        private void PlaceBar_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (!CanDragPlace(_vm.Model.Place))
            {
                e.Handled = true;
            }
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
            try
            {
                Clipboard.SetText(_vm.Model.Place?.SimplePath ?? "");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void PlaceIcon_QueryContinueDrag(object? sender, QueryContinueDragEventArgs e)
        {
            _ghost.QueryContinueDrag(sender, e);
        }

        #endregion

        public void SetFolderListBoxContent(FolderListBox content)
        {
            this.ListBoxContent.Content = content;
        }

        // TODO: 共通化
        private void Root_KeyDown(object? sender, KeyEventArgs e)
        {
            // このパネルで使用するキーのイベントを止める
            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Up || e.Key == Key.Down || (_vm.IsLRKeyEnabled() && (e.Key == Key.Left || e.Key == Key.Right)) || e.Key == Key.Return || e.Key == Key.Delete)
                {
                    e.Handled = true;
                }
            }
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
