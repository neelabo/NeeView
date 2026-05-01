using NeeView.Properties;
using NeeView.Windows;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleFilmStripCommand : CommandElement
    {
        public ToggleVisibleFilmStripCommand()
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
            var state = CommandElementTools.GetState(e, Config.Current.FilmStrip.IsEnabled, MainWindow.Current.IsFilmStripVisible);
            return GetStateExecuteMessage(state);
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            var state = CommandElementTools.GetState(e, Config.Current.FilmStrip.IsEnabled, MainWindow.Current.IsFilmStripVisible);
            if (e.ByMenu)
            {
                Config.Current.FilmStrip.IsEnabled = state;
            }
            else
            {
                MainWindowModel.Current.SetFilmStripVisible(state.ToStateRequest());
            }
        }
    }
}
