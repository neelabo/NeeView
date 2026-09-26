using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// DestinationFolderPanelView.xaml 的交互逻辑。
    /// </summary>
    public partial class DestinationFolderPanelView : UserControl
    {
        private readonly DestinationFolderPanelViewModel _viewModel;

        /// <summary>
        /// 初始化面板并绑定目标文件夹视图模型。
        /// </summary>
        public DestinationFolderPanelView()
        {
            InitializeComponent();
            _viewModel = new DestinationFolderPanelViewModel();

            // 目标按钮从祖先 UserControl 读取 ClassifyCommand，因此必须在视图本身设置 DataContext。
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
        /// 从当前配置刷新目标文件夹列表。
        /// </summary>
        public void Refresh()
        {
            _viewModel.Refresh();
        }

        /// <summary>
        /// 在新文件夹输入框按下回车时执行与“新增文件夹”按钮相同的命令。
        /// </summary>
        /// <param name="sender">新文件夹名称输入框</param>
        /// <param name="e">键盘事件参数</param>
        private void NewFolderNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // 输入框聚焦时禁用单键快捷键，确保英文、数字及其他字符优先进入文本框。
            KeyExGesture.AddFilter(KeyExGestureFilter.All);

            if (e.Key != Key.Enter || Keyboard.Modifiers != ModifierKeys.None) return;

            // 统一通过 ViewModel 命令执行，确保按钮和回车共用校验与忙碌状态。
            if (_viewModel.CreateFolderCommand.CanExecute(null))
            {
                _viewModel.CreateFolderCommand.Execute(null);
                e.Handled = true;
            }
        }

        /// <summary>
        /// 将键盘焦点移入面板。
        /// </summary>
        public void FocusAtOnce()
        {
            Focus();
        }
    }
}
