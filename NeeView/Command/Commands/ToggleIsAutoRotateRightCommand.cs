using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsAutoRotateRightCommand : CommandElement
    {
        public ToggleIsAutoRotateRightCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.ViewManipulation");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ViewPropertyControl.IsAutoRotateRight)) { Source = MainViewComponent.Current.ViewPropertyControl };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return MainViewComponent.Current.ViewPropertyControl.GetAutoRotateRight() ? Properties.TextResources.GetString("ToggleIsAutoRotateRightCommand.Off") : Properties.TextResources.GetString("ToggleIsAutoRotateRightCommand.On");
        }
        
        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        [MethodArgument("@ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                MainViewComponent.Current.ViewPropertyControl.SetAutoRotateRight(Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture));
            }
            else
            {
                MainViewComponent.Current.ViewPropertyControl.ToggleAutoRotateRight();
            }
        }
    }
}
