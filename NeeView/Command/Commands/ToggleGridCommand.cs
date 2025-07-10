using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleGridCommand : CommandElement
    {
        public ToggleGridCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Effect");
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ImageGridConfig.IsEnabled)) { Mode = BindingMode.OneWay, Source = Config.Current.ImageGrid };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return Config.Current.ImageGrid.IsEnabled ? TextResources.GetString("ToggleGridCommand.Off") : TextResources.GetString("ToggleGridCommand.On");
        }

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                Config.Current.ImageGrid.IsEnabled = Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture);
            }
            else
            {
                Config.Current.ImageGrid.IsEnabled = !Config.Current.ImageGrid.IsEnabled;
            }
        }
    }
}
