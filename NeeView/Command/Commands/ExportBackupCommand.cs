namespace NeeView
{
    public class ExportBackupCommand : CommandElement
    {
        public ExportBackupCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Other");
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new ExportBackupCommandParameter());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            ExportDataPresenter.Current.Export(e.Parameter.Cast<ExportBackupCommandParameter>());
        }
    }
}
