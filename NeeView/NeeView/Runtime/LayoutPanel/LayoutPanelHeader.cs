using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView.Runtime.LayoutPanel
{
    public class LayoutPanelHeader : Control
    {
        public readonly static RoutedCommand CloseCommand = new(nameof(CloseCommand), typeof(LayoutPanelHeader));


        static LayoutPanelHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutPanelHeader), new FrameworkPropertyMetadata(typeof(LayoutPanelHeader)));
        }

        public LayoutPanelHeader()
        {
            InitializeCommand();
        }


        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(LayoutPanelHeader), new PropertyMetadata(null));


        public ICommand ClosePanelCommand
        {
            get { return (ICommand)GetValue(ClosePanelCommandProperty); }
            set { SetValue(ClosePanelCommandProperty, value); }
        }

        public static readonly DependencyProperty ClosePanelCommandProperty =
            DependencyProperty.Register("ClosePanelCommand", typeof(ICommand), typeof(LayoutPanelHeader), new PropertyMetadata(null));




        private void InitializeCommand()
        {
            this.CommandBindings.Add(new CommandBinding(CloseCommand, CloseCommand_Execute, CloseCommand_CanExecute));
        }

        private void CloseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CloseCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            ClosePanelCommand?.Execute(null);
        }
    }
}
