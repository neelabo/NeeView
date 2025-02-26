using System;
using System.Reflection.Metadata;
using System.Windows.Controls;

namespace NeeView
{
    public class CopyToFolderAsCommand : CommandElement
    {
        private readonly DestinationFolderParameterCommandParameterFactory _parameterFactory;

        public CopyToFolderAsCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.File");
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new CopyToFolderAsCommandParameter());

            _parameterFactory = new DestinationFolderParameterCommandParameterFactory(new CopyToDestinationFolderOption(this));
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            var parameter = e.Parameter.Cast<CopyToFolderAsCommandParameter>();
            var index = parameter.Index - 1;
            if (index >= 0)
            {
                var folders = Config.Current.System.DestinationFolderCollection;
                if (!folders.IsValidIndex(index)) return false;
                return BookOperation.Current.Control.CanCopyToFolder(folders[index], parameter.MultiPagePolicy);
            }
            else
            {
                return true;
            }
        }

        public override void Execute(object? sender, CommandContext e)
        {
            var parameter = e.Parameter.Cast<CopyToFolderAsCommandParameter>();
            var index = parameter.Index - 1;
            if (index >= 0)
            {
                var folders = Config.Current.System.DestinationFolderCollection;
                if (!folders.IsValidIndex(index)) return;
                BookOperation.Current.Control.CopyToFolder(folders[index], parameter.MultiPagePolicy);
            }
            else
            {
                MainViewComponent.Current.MainView.CommandMenu.OpenCopyToFolderMenu(_parameterFactory);
            }
        }

        public override MenuItem? CreateMenuItem(bool isDefault)
        {
            var parameter = GetCommandParameter();
            var index = parameter.Index - 1;
            if (isDefault || index < 0)
            {
                return MainViewCopyToFolderTools.CreateCopyToFolderItem(_parameterFactory);
            }
            else
            {
                return null;
            }
        }

        private CopyToFolderAsCommandParameter GetCommandParameter()
        {
            return (Parameter as CopyToFolderAsCommandParameter) ?? throw new InvalidOperationException();
        }
    }



    public class CopyToDestinationFolderOption : IDestinationFolderOption
    {
        private readonly CopyToFolderAsCommand _command;

        public CopyToDestinationFolderOption(CopyToFolderAsCommand command)
        {
            _command = command;
        }

        public MultiPagePolicy MultiPagePolicy => _command.Parameter.Cast<CopyToFolderAsCommandParameter>().MultiPagePolicy;
    }
}
