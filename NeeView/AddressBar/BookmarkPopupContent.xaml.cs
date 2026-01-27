using NeeLaboratory.Generators;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// BookmarkPopupContent.xaml の相互作用ロジック
    /// </summary>
    public partial class BookmarkPopupContent : UserControl
    {
        private readonly BookmarkPopupContentViewModel _vm;
        private BookmarkPopupResult _result = BookmarkPopupResult.None;

        public BookmarkPopupContent() : this("")
        {
        }

        public BookmarkPopupContent(string path)
        {
            InitializeComponent();

            _vm = new BookmarkPopupContentViewModel(path);
            _vm.FolderTreeViewSelectionChanged += ViewModel_FolderTreeViewSelectionChanged;
            this.Root.DataContext = _vm;

            this.Loaded += BookmarkPopupContent_Loaded;
            this.Unloaded += BookmarkPopupContent_Unloaded;
            this.SizeChanged += BookmarkPopupContent_SizeChanged;
            this.KeyDown += BookmarkPopupContent_KeyDown;
            this.FolderTreeView.SelectedItemChanged += FolderTreeView_SelectedItemChanged;
        }


        [Subscribable]
        public event EventHandler? SelfClosed;


        public Popup ParentPopup
        {
            get { return (Popup)GetValue(ParentPopupProperty); }
            set { SetValue(ParentPopupProperty, value); }
        }

        public static readonly DependencyProperty ParentPopupProperty =
            DependencyProperty.Register("ParentPopup", typeof(Popup), typeof(BookmarkPopupContent), new PropertyMetadata(null));


        public DpiScale DpiScale
        {
            get { return (DpiScale)GetValue(DpiScaleProperty); }
            set { SetValue(DpiScaleProperty, value); }
        }

        public static readonly DependencyProperty DpiScaleProperty =
            DependencyProperty.Register("DpiScale", typeof(DpiScale), typeof(BookmarkPopupContent), new PropertyMetadata(new DpiScale(1.0, 1.0)));


        private void ViewModel_FolderTreeViewSelectionChanged(object? sender, EventArgs e)
        {
            ScrollIntoView(this.FolderTreeView.SelectedItem as FolderTreeNodeBase);
        }

        private void BookmarkPopupContent_Loaded(object sender, RoutedEventArgs e)
        {
            this.OKButton.Focus();
            this.DpiScale = VisualTreeHelper.GetDpi(this);
        }

        private void BookmarkPopupContent_Unloaded(object sender, RoutedEventArgs e)
        {
            SelfClosed = null;
        }

        private void BookmarkPopupContent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                this.MinWidth = Math.Max(this.MinWidth, e.NewSize.Width);
            }
        }

        private void BookmarkPopupContent_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    if (Keyboard.Modifiers == ModifierKeys.None)
                    {
                        Close();
                        e.Handled = true;
                    }
                    break;

                case Key.Left:
                case Key.Up:
                case Key.Right:
                case Key.Down:
                    e.Handled = true;
                    break;
            }
        }

        private void FolderTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = this.FolderTreeView.SelectedItem as FolderTreeNodeBase;
            //Debug.WriteLine($"SelectedItemChanged: {e.OldValue} -> {e.NewValue}");
            _vm.SelectedItem = item;
        }

        private void FolderTreeView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                if (_vm.SelectedItem is null) return;

                _vm.SetTreeViewSelectedItem(_vm.SelectedItem, true);
                AppDispatcher.BeginInvoke(() => ScrollIntoView(_vm.SelectedItem));
            }
        }

        private void BookmarkNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textBox) return;

            AppDispatcher.BeginInvoke(() => textBox.SelectAll());
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            _result = _vm.IsEdit ? BookmarkPopupResult.Edit : BookmarkPopupResult.Add;
            Close();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            _result = BookmarkPopupResult.Add;
            Close();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            _result = BookmarkPopupResult.Remove;
            Close();
        }

        public void Decide()
        {
            if (_result == BookmarkPopupResult.None && !_vm.IsEdit)
            {
                _result = BookmarkPopupResult.Add;
            }
            _vm.ApplyBookmark(_result);
        }

        private void Close()
        {
            SelfClosed?.Invoke(this, EventArgs.Empty);

            if (ParentPopup != null)
            {
                ParentPopup.IsOpen = false;
            }
        }

        private void ScrollIntoView(FolderTreeNodeBase? item)
        {
            if (item is null) return;
            if (!this.FolderTreeView.IsVisible) return;

            this.FolderTreeView.ScrollIntoView(item);
        }
    }
}
