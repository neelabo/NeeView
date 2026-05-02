using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleContentsTreeCommand : CommandElement
    {
        public ToggleVisibleContentsTreeCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Panel");
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(PageListConfig.IsFolderTreeVisible)) { Source = Config.Current.PageList, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, PageListFolderTreeTools.IsVisiblePageListFolderTree);
            return GetStateExecuteMessage(state);
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                PageListFolderTreeTools.IsVisiblePageListFolderTree = Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture);
            }
            else
            {
                PageListFolderTreeTools.ToggleVisiblePageListFolderTree(e.Options.HasFlag(CommandOption.ByMenu));
            }
        }
    }


    public static class PageListFolderTreeTools
    {
        /// <summary>
        /// フォルダーツリー表示状態
        /// </summary>
        public static bool IsVisiblePageListFolderTree
        {
            get { return Config.Current.PageList.IsFolderTreeVisible && SidePanelFrame.Current.IsVisiblePageList; }
            set { SetVisiblePageListFolderTree(false, value); }
        }

        /// <summary>
        /// フォルダーツリー表示状態切替
        /// </summary>
        public static bool ToggleVisiblePageListFolderTree(bool byMenu)
        {
            return SetVisiblePageListFolderTree(byMenu, !IsVisiblePageListFolderTree || !SidePanelFrame.Current.IsVisiblePageList);
        }

        /// <summary>
        /// フォルダーツリー表示状設定
        /// </summary>
        private static bool SetVisiblePageListFolderTree(bool byMenu, bool isVisible)
        {
            //Debug.WriteLine($"{isVisible}, {SidePanelFrame.Current.IsVisiblePageList}");

            Config.Current.PageList.IsFolderTreeVisible = isVisible;

            SidePanelFrame.Current.SetVisiblePageList(true, true, true);

            if (!byMenu && isVisible)
            {
                // フォーカス要求
                var panel = (PageListPanel)CustomLayoutPanelManager.Current.GetPanel(nameof(PageListPanel));
                panel.Presenter.FocusContentTreeAtOnce();
            }

            return Config.Current.PageList.IsFolderTreeVisible;
        }
    }
}
