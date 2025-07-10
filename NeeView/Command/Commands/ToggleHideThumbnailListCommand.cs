using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleHideThumbnailListCommand : CommandElement
    {
        public ToggleHideThumbnailListCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.FilmStrip");
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(FilmStripConfig.IsHideFilmStrip)) { Source = Config.Current.FilmStrip };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return Config.Current.FilmStrip.IsHideFilmStrip ? TextResources.GetString("ToggleHideThumbnailListCommand.Off") : TextResources.GetString("ToggleHideThumbnailListCommand.On");
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return Config.Current.FilmStrip.IsEnabled;
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                Config.Current.FilmStrip.IsHideFilmStrip = Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture);
            }
            else
            {
                ThumbnailList.Current.ToggleHideThumbnailList();
            }
        }
    }
}
