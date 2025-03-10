using NeeView.Windows;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// AddressBarView
    /// .xaml の相互作用ロジック
    /// </summary>
    public partial class AddressBarView : UserControl
    {
        public static readonly string DragDropFormat = $"{Environment.ProcessId}.BookAddress";


        private AddressBarViewModel? _vm;
        private UIElement? _popupClosedFocusElement;


        public AddressBarView()
        {
            InitializeComponent();

            this.AddressTextBox.PreviewMouseLeftButtonDown += AddressTextBox_PreviewMouseLeftButtonDown;
            this.AddressTextBox.GotFocus += AddressTextBox_GotFocus;
        }


        #region DependencyProperties

        public AddressBar Source
        {
            get { return (AddressBar)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(AddressBar), typeof(AddressBarView), new PropertyMetadata(null, Source_Changed));

        private static void Source_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as AddressBarView)?.Initialize();
        }

        #endregion

        public void Initialize()
        {
            _vm = new AddressBarViewModel(this.Source);
            this.Root.DataContext = _vm;
        }

        private void AddressTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not TextBox t) return;

            if (!t.IsFocused)
            {
                t.Focus();
                e.Handled = true;
            }
        }

        private void AddressTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textBox) return;

            textBox.SelectAll();
        }

        // アドレスバー入力
        private void AddressTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (_vm is null) return;

            if (e.Key == Key.Return)
            {
                _vm.Model.Address = this.AddressTextBox.Text;
            }

            // 単キーのショートカット無効
            KeyExGesture.AddFilter(KeyExGestureFilter.All);
            //e.Handled = true;
        }

        /// <summary>
        /// 履歴戻るボタンコンテキストメニュー開始前イベント処理
        /// </summary>
        private void PrevHistoryButton_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (_vm is null) return;

            var menu = (sender as FrameworkElement)?.ContextMenu;
            if (menu == null) return;
            menu.ItemsSource = _vm.GetHistory(-1, 10);
        }

        /// <summary>
        /// 履歴進むボタンコンテキストメニュー開始前イベント処理
        /// </summary>
        private void NextHistoryButton_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (_vm is null) return;

            var menu = (sender as FrameworkElement)?.ContextMenu;
            if (menu == null) return;
            menu.ItemsSource = _vm.GetHistory(+1, 10);
        }

        private void PageSortModeButton_Click(object sender, RoutedEventArgs e)
        {
            PageSortModePopup.IsOpen = true;
        }

        private void BookButton_Click(object sender, RoutedEventArgs e)
        {
            this.BookPopup.IsOpen = true;
        }
        private void Popup_Opened(object sender, EventArgs e)
        {
            PopupWatcher.SetPopupElement(sender, (UIElement)sender);
            _popupClosedFocusElement = null;
        }

        private void Popup_Closed(object sender, EventArgs e)
        {
            PopupWatcher.SetPopupElement(sender, null);
            _popupClosedFocusElement?.Focus();
        }

        private void PageSortModePopup_SelfClosed(object sender, EventArgs e)
        {
            _popupClosedFocusElement = this.PageSortModeButton;
        }

        private void BookPopup_SelfClosed(object sender, EventArgs e)
        {
            _popupClosedFocusElement = this.BookButton;
        }

        #region DragDrop

        private readonly DragDropGhost _ghost = new();
        private bool _isButtonDown;
        private Point _buttonDownPos;

        private void BookButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_vm is null) return;

            if (!_vm.Model.IsBookEnabled)
            {
                return;
            }

            var element = sender as UIElement;
            _buttonDownPos = e.GetPosition(element);
            _isButtonDown = true;
        }

        private void BookButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isButtonDown = false;
        }

        private void BookButton_MouseMove(object sender, MouseEventArgs e)
        {
            if (_vm is null) return;

            if (!_isButtonDown)
            {
                return;
            }

            if (e.LeftButton == MouseButtonState.Released)
            {
                _isButtonDown = false;
                return;
            }

            if (sender is not UIElement element) return;

            var pos = e.GetPosition(element);
            if (DragDropHelper.IsDragDistance(pos, _buttonDownPos))
            {
                _isButtonDown = false;

                if (!_vm.Model.IsBookEnabled)
                {
                    return;
                }

                var data = new DataObject();
                data.SetQueryPathAndFile(new QueryPath(_vm.Model.Address));

                _ghost.Attach(element, new Point(24, 24));
                DragDropWatcher.SetDragElement(sender, element);
                DragDrop.DoDragDrop(element, data, DragDropEffects.Copy);
                DragDropWatcher.SetDragElement(sender, null);
                _ghost.Detach();
            }
        }

        private void BookButton_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            _ghost.QueryContinueDrag(sender, e);
        }

        #endregion
    }
}
