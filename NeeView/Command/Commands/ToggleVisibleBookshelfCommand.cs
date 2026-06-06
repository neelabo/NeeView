using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleBookshelfCommand : CommandElement
    {
        public ToggleVisibleBookshelfCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Panel");
            this.ShortCutKey = new ShortcutKey("B");
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new ToggleCommandParameter());
        }

        public override BindingBase CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanelFrame.IsVisibleFolderList)) { Source = SidePanelFrame.Current };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, SidePanelFrame.Current.GetVisibleFolderList(e.ByMenu));
            return GetStateExecuteMessage(state);
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            var toggleMode = CommandTools.GetToggleMode(e);

            if (toggleMode != ToggleMode.Toggle)
            {
                SidePanelFrame.Current.SetVisibleFolderList(toggleMode == ToggleMode.On, true, true);
            }
            else
            {
                SidePanelFrame.Current.ToggleVisibleFolderList(e.Options.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
