using NeeView.Properties;
using System;

namespace NeeView
{
    public class ImportBackupCommand : CommandElement
    {
        public ImportBackupCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.Other");
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new ImportBackupCommandParameter());
        }
        
        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDisplayNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            ExportDataPresenter.Current.Import(e.Parameter.Cast<ImportBackupCommandParameter>());
        }
    }


}
