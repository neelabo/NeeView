using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class ToggleStretchModeCommand : CommandElement
    {
        public ToggleStretchModeCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ImageScale");
            this.ShortCutKey = new ShortcutKey("LeftButton+WheelDown");
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new ToggleStretchModeCommandParameter());
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return MainViewComponent.Current.ViewPropertyControl.GetToggleStretchMode(e.Parameter.Cast<ToggleStretchModeCommandParameter>()).ToAliasName();
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            Config.Current.View.StretchMode = MainViewComponent.Current.ViewPropertyControl.GetToggleStretchMode(e.Parameter.Cast<ToggleStretchModeCommandParameter>());
        }
    }

}
