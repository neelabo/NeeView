using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using NeeView.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// HistoryListView.xaml の相互作用ロジック
    /// </summary>
    public partial class PlaylistView : UserControl
    {
        private readonly PlaylistViewModel _vm;


        public PlaylistView(PlaylistHub model)
        {
            InitializeComponent();
            InitializeCommand();

            _vm = new PlaylistViewModel(model);
            this.DockPanel.DataContext = _vm;

            _vm.RenameRequest += ViewModel_RenameRequest;

            this.PlaylistComboBox.DropDownOpened +=
                (s, e) => _vm.UpdatePlaylistCollection();

            Debug.WriteLine($"> Create: {nameof(PlaylistView)}");
        }


        public readonly static RoutedCommand RenameCommand = new(nameof(RenameCommand), typeof(PlaylistView), new InputGestureCollection() { new KeyGesture(Key.F2) });


        private void InitializeCommand()
        {
            this.PlaylistComboBox.CommandBindings.Add(new CommandBinding(RenameCommand, RenameCommand_Execute, RenameCommand_CanExecute));
        }

        private void RenameCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _vm.RenameCommand.CanExecute(e.Parameter);
        }

        private void RenameCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            _vm.RenameCommand.Execute(e.Parameter);
        }


        private async void ViewModel_RenameRequest(object? sender, EventArgs e)
        {
            var comboBox = this.PlaylistComboBox;
            comboBox.UpdateLayout();

            var textBlock = VisualTreeUtility.FindVisualChild<TextBlock>(comboBox, "FileNameTextBlock");
            if (textBlock is null) return;

            var rename = new PlaylistRenameControl(new RenameControlSource(comboBox, textBlock), Rename);
            await rename.ShowAsync();

            bool Rename(string name)
            {
                return _vm.Rename(name);
            }
        }
    }

    public class PlaylistRenameControl : RenameControl
    {
        private readonly Func<string, bool> _renameFunc;

        public PlaylistRenameControl(RenameControlSource source, Func<string, bool> renameFunc) : base(source)
        {
            _renameFunc = renameFunc;
            this.IsInvalidSeparatorChars = true;
            this.IsInvalidFileNameChars = true;
        }

        protected override async ValueTask<bool> OnRenameAsync(string oldValue, string newValue)
        {
            if (oldValue == newValue) return true;

            var result = _renameFunc(newValue);
            return await Task.FromResult(result);
        }
    }

}
