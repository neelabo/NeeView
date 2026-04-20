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
            return Config.Current.PageList.IsFolderTreeVisible ? TextResources.GetString("ToggleVisibleContentsTreeCommand.Off") : TextResources.GetString("ToggleVisibleContentsTreeCommand.On");
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            var focus = !e.Options.HasFlag(CommandOption.ByMenu);

            if (e.Args.Length > 0)
            {
                SetContentsTreeVisible(Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture), focus);
            }
            else
            {
                ToggleContentsTreeVisible(focus);
            }
        }


        private void SetContentsTreeVisible(bool isVisible, bool focus)
        {
            Config.Current.PageList.IsFolderTreeVisible = isVisible;

            if (isVisible)
            {
                // パネル表示
                SidePanelFrame.Current.SetVisiblePageList(true, true, false);

                if (focus)
                {
                    // フォーカス
                    var panel = (PageListPanel)CustomLayoutPanelManager.Current.GetPanel(nameof(PageListPanel));
                    panel.Presenter.FocusContentTreeAtOnce();
                }
            }
        }

        private void ToggleContentsTreeVisible(bool focus)
        {
            SetContentsTreeVisible(!Config.Current.PageList.IsFolderTreeVisible, focus);
        }
    }
}
