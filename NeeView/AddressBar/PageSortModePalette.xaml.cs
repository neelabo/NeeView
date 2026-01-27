using NeeLaboratory.Generators;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// PageSortModePalette.xaml の相互作用ロジック
    /// </summary>
    [NotifyPropertyChanged]
    public partial class PageSortModePalette : UserControl, INotifyPropertyChanged
    {
        private readonly PageSortModePaletteViewModel _vm;

        public PageSortModePalette()
        {
            InitializeComponent();

            _vm = new PageSortModePaletteViewModel();
            this.Root.DataContext = _vm;

            this.Loaded += (s, e) => this.Items.Focus();
        }

        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;

        [Subscribable]
        public event EventHandler? SelfClosed;

        public Popup ParentPopup
        {
            get { return (Popup)GetValue(ParentPopupProperty); }
            set { SetValue(ParentPopupProperty, value); }
        }

        public static readonly DependencyProperty ParentPopupProperty =
            DependencyProperty.Register("ParentPopup", typeof(Popup), typeof(PageSortModePalette), new PropertyMetadata(null));


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var select = (PageSortMode)((Button)sender).Tag;
            _vm.Decide(select);
            Close();
        }

        private void Root_PreviewKeyDown(object sender, KeyEventArgs e)
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
                    MoveFocus(FocusNavigationDirection.Left);
                    e.Handled = true;
                    break;

                case Key.Up:
                    MoveFocus(FocusNavigationDirection.Previous);
                    e.Handled = true;
                    break;

                case Key.Right:
                    MoveFocus(FocusNavigationDirection.Right);
                    e.Handled = true;
                    break;

                case Key.Down:
                    MoveFocus(FocusNavigationDirection.Next);
                    e.Handled = true;
                    break;
            }
        }

        private void Close()
        {
            SelfClosed?.Invoke(this, EventArgs.Empty);

            if (ParentPopup != null)
            {
                ParentPopup.IsOpen = false;
            }
        }

        private void MoveFocus(FocusNavigationDirection direction)
        {
            var element = FocusManager.GetFocusedElement(Window.GetWindow(this)) as UIElement ?? this.Items;
            element.MoveFocus(new TraversalRequest(direction));
        }
    }

}
