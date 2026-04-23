using NeeView.Properties;
using NeeView.Windows;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleThumbnailListCommand : CommandElement
    {
        public ToggleVisibleThumbnailListCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.FilmStrip");
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(FilmStripConfig.IsEnabled)) { Source = Config.Current.FilmStrip };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return ThumbnailList.Current.IsVisible ? TextResources.GetString("ToggleVisibleThumbnailListCommand.Off") : TextResources.GetString("ToggleVisibleThumbnailListCommand.On");
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Options.HasFlag(CommandOption.ByMenu))
            {
                Config.Current.FilmStrip.IsEnabled = e.Args.Length > 0
                    ? Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture)
                    : !Config.Current.FilmStrip.IsEnabled;
            }
            else
            {
                if (e.Args.Length > 0)
                {
                    var isVisible = Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture);
                    MainWindowModel.Current.SetFilmStripVisible(isVisible.ToVisibilityRequest());
                }
                else
                {
                    MainWindowModel.Current.SetFilmStripVisible(VisibilityRequest.Toggle);
                }
            }
        }
    }
}
