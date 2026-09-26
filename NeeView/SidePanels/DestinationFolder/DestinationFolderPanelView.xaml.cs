using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// Interaction logic for DestinationFolderPanelView.xaml.
    /// </summary>
    public partial class DestinationFolderPanelView : UserControl
    {
        private readonly DestinationFolderPanelViewModel _viewModel;

        /// <summary>
        /// Initialize the panel and bind its view model.
        /// </summary>
        public DestinationFolderPanelView()
        {
            InitializeComponent();
            _viewModel = new DestinationFolderPanelViewModel();

            // Item buttons resolve ClassifyCommand from the ancestor UserControl, so bind the view itself.
            DataContext = _viewModel;

            // 每次创建面板视图时恢复上次拖动保存的两组高度比例。
            var ratio = Config.Current.Panels.DestinationFolderSectionRatio;
            ManagedSectionRow.Height = new GridLength(ratio, GridUnitType.Star);
            CurrentFolderSectionRow.Height = new GridLength(1.0 - ratio, GridUnitType.Star);
        }

        /// <summary>
        /// 拖动结束后按实际行高保存比例，以便重新打开面板和下次启动时恢复。
        /// </summary>
        /// <param name="sender">两组列表之间的分隔控件</param>
        /// <param name="e">拖动完成事件参数</param>
        private void FolderSectionsSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            var totalHeight = ManagedSectionRow.ActualHeight + CurrentFolderSectionRow.ActualHeight;
            if (totalHeight <= 0.0 || !double.IsFinite(totalHeight)) return;

            // 只保存比例，不保存像素高度；窗口或右侧面板尺寸变化时仍能按比例布局。
            Config.Current.Panels.DestinationFolderSectionRatio = ManagedSectionRow.ActualHeight / totalHeight;
        }

        /// <summary>
        /// Refresh destination folders from the current configuration.
        /// </summary>
        public void Refresh()
        {
            _viewModel.Refresh();
        }

        /// <summary>
        /// Execute the same command as the create-folder button when Enter is pressed.
        /// </summary>
        /// <param name="sender">New-folder name text box.</param>
        /// <param name="e">Keyboard event arguments.</param>
        private void NewFolderNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Disable single-key shortcuts while focused so all text input reaches the text box first.
            KeyExGesture.AddFilter(KeyExGestureFilter.All);

            if (e.Key != Key.Enter || Keyboard.Modifiers != ModifierKeys.None) return;

            // Route both interactions through one command so validation and busy state stay consistent.
            if (_viewModel.CreateFolderCommand.CanExecute(null))
            {
                _viewModel.CreateFolderCommand.Execute(null);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Move keyboard focus into the panel.
        /// </summary>
        public void FocusAtOnce()
        {
            Focus();
        }
    }
}
