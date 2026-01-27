using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// FileInformationView.xaml の相互作用ロジック
    /// </summary>
    public partial class FileInformationContent : UserControl
    {
        private readonly FileInformationContentViewModel _vm;


        static FileInformationContent()
        {
            InitializeCommandStatic();
        }

        public FileInformationContent()
        {
            InitializeComponent();
            InitializeCommand();

            _vm = new FileInformationContentViewModel();
            this.Root.DataContext = _vm;

            // タッチスクロール操作の終端挙動抑制
            this.PropertyListBox.ManipulationBoundaryFeedback += SidePanelFrame.Current.ScrollViewer_ManipulationBoundaryFeedback;
        }


        #region Depenency properties

        public FileInformationSource Source
        {
            get { return (FileInformationSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(FileInformationSource), typeof(FileInformationContent), new PropertyMetadata(null, SourcePropertyChanged));


        private static void SourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileInformationContent control)
            {
                control._vm.Source = control.Source;
            }
        }

        #endregion Depenency properties


        #region Commands

        public readonly static RoutedCommand CopyCommand = new(nameof(CopyCommand), typeof(FileInformationContent));

        private static void InitializeCommandStatic()
        {
            CopyCommand.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Control));
        }

        private void InitializeCommand()
        {
            this.PropertyListBox.CommandBindings.Add(new CommandBinding(CopyCommand, CopyCommand_Execute, CopyCommand_CanExecute));
        }

        private void CopyCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.PropertyListBox.SelectedItem is FileInformationRecord record && record.Value?.ToString() != null;
        }

        private void CopyCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.PropertyListBox.SelectedItem is FileInformationRecord record)
            {
                var text = record.Value?.ToString();
                if (text != null)
                {
                    Clipboard.SetText(text);
                }
            }
        }

        #endregion Commands


        private void TextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox t && !t.IsFocused)
            {
                t.Focus();
                e.Handled = true;
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox t)
            {
                this.PropertyListBox.SelectedItem = null;
                t.SelectAll();
            }
        }

    }
}
