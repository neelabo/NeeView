using NeeView.Native;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// VersionWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class VersionWindow : Window
    {
        public readonly static RoutedCommand CopyCommand = new("CopyCommand", typeof(VersionWindowViewModel), new InputGestureCollection(new List<InputGesture>() { new KeyGesture(Key.C, ModifierKeys.Control) }));

        private readonly VersionWindowViewModel _vm;


        public VersionWindow()
        {
            NVInterop.NVFpReset();

            InitializeComponent();

            _vm = new VersionWindowViewModel();
            this.DataContext = _vm;

            this.CommandBindings.Add(new CommandBinding(CopyCommand, (s, e) => _vm.CopyVersionToClipboard(), (s, e) => e.CanExecute = true));
            this.CopyContextMenu.CommandBindings.Add(new CommandBinding(CopyCommand, (s, e) => _vm.CopyVersionToClipboard(), (s, e) => e.CanExecute = true));
        }


        // from http://gushwell.ldblog.jp/archives/52279481.html
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            var path = e.Uri.Scheme == "file" ? e.Uri.LocalPath : e.Uri.AbsoluteUri;
            ExternalProcess.Start(path);
            e.Handled = true;
        }
    }
}
