using CommunityToolkit.Mvvm.ComponentModel;
using NeeView.Properties;
using System;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// 可停靠的目标文件夹快速分类面板。
    /// </summary>
    public sealed class DestinationFolderPanel : ObservableObject, IPanel
    {
        private readonly Lazy<DestinationFolderPanelView> _view;

        /// <summary>
        /// 初始化面板懒加载视图和侧栏图标。
        /// </summary>
        public DestinationFolderPanel()
        {
            _view = new Lazy<DestinationFolderPanelView>(() => new DestinationFolderPanelView());
            Icon = App.Current.MainWindow.Resources["pic_bookshelf"] as ImageSource
                ?? throw new InvalidOperationException("Cannot found resource `pic_bookshelf`");
        }

#pragma warning disable CS0067
        /// <summary>
        /// 面板可见性锁状态变化事件；当前面板不锁定显示，因此不会主动触发。
        /// </summary>
        public event EventHandler? IsVisibleLockChanged;
#pragma warning restore CS0067

        /// <summary>
        /// 获取布局系统使用的稳定面板类型代码。
        /// </summary>
        public string TypeCode => nameof(DestinationFolderPanel);

        /// <summary>
        /// 获取侧栏图标。
        /// </summary>
        public ImageSource Icon { get; }

        /// <summary>
        /// 获取图标提示文本。
        /// </summary>
        public string IconTips => TextResources.GetString("DestinationFolderPanel.Title");

        /// <summary>
        /// 获取懒加载的面板视图。
        /// </summary>
        public Lazy<FrameworkElement> View => new Lazy<FrameworkElement>(() => _view.Value);

        /// <summary>
        /// 获取面板是否强制保持显示。
        /// </summary>
        public bool IsVisibleLock => false;

        /// <summary>
        /// 获取面板的默认停靠位置。
        /// </summary>
        public PanelPlace DefaultPlace => PanelPlace.Right;

        /// <summary>
        /// 在视图已经创建时刷新目标文件夹列表。
        /// </summary>
        public void Refresh()
        {
            if (_view.IsValueCreated)
            {
                _view.Value.Refresh();
            }
        }

        /// <summary>
        /// 创建视图并将键盘焦点移入面板。
        /// </summary>
        public void Focus()
        {
            _view.Value.FocusAtOnce();
        }
    }
}
