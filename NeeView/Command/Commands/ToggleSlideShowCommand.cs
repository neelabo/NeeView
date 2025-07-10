using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleSlideShowCommand : CommandElement
    {
        public ToggleSlideShowCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.ShortCutKey = new ShortcutKey("F5");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SlideShow.IsPlayingSlideShow)) { Source = SlideShow.Current };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return SlideShow.Current.IsPlayingSlideShow ? TextResources.GetString("ToggleSlideShowCommand.Off") : TextResources.GetString("ToggleSlideShowCommand.On");
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                SlideShow.Current.IsPlayingSlideShow = Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture);
            }
            else
            {
                SlideShow.Current.TogglePlayingSlideShow();
            }
        }
    }
}
