using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace NeeView
{
    public partial class ContentsTreeView : UserControl
    {
        private readonly ContentsTreeViewModel _vm;

        public ContentsTreeView()
        {
            InitializeComponent();

            _vm = new ContentsTreeViewModel();
            this.Root.DataContext = _vm;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Return || e.Key == Key.Delete)
                {
                    e.Handled = true;
                    return;
                }
            }

            base.OnKeyDown(e);
        }

        public void FocusAtOnce()
        {
            Dispatcher.BeginInvoke(() => this.ContentsTree.Focus(), DispatcherPriority.Input);
        }

        private void ContentsTree_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool isVisible)
            {
                _vm.IsVisible = isVisible;
            }
        }

        private void ContentsTree_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.ContentsTree.SelectedItem is ContentsPageNode node)
            {
                node.IsSelected = false;
            }
        }

        private void ContentsTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is ContentsPageNode node)
            {
                _vm.SelectIndex(node);
            }
        }
    }
}
