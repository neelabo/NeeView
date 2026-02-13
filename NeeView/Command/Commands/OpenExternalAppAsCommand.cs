using NeeView.Properties;
using System;
using System.Diagnostics;
using System.Windows.Controls;

namespace NeeView
{
    public class OpenExternalAppAsCommand : CommandElement
    {
        private readonly Lazy<OpenPageExternalAppMenuFactory> _menuFactory;

        public OpenExternalAppAsCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new OpenExternalAppAsCommandParameter());

            var parameterFactory = new ExternalAppParameterCommandParameterFactory(new ExternalAppOption(this));
            _menuFactory = new Lazy<OpenPageExternalAppMenuFactory>(() => new OpenPageExternalAppMenuFactory(parameterFactory));
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            var parameter = e.Parameter.Cast<OpenExternalAppAsCommandParameter>();
            var index = parameter.Index - 1;

            if (index >= 0)
            {
                var externalApps = Config.Current.System.ExternalAppCollection;
                if (!externalApps.IsValidIndex(index)) return false;
                return BookOperation.Current.Control.CanOpenApplication(externalApps[index], parameter.MultiPagePolicy);
            }
            else
            {
                return true;
            }
        }


        public override void Execute(object? sender, CommandContext e)
        {
            var parameter = e.Parameter.Cast<OpenExternalAppAsCommandParameter>();
            var index = parameter.Index - 1;
            if (index >= 0)
            {
                var externalApps = Config.Current.System.ExternalAppCollection;
                if (!externalApps.IsValidIndex(index)) throw new ArgumentOutOfRangeException(nameof(index));
                BookOperation.Current.Control.OpenApplication(externalApps[index], parameter.MultiPagePolicy);
            }
            else
            {
                MainViewComponent.Current.MainView.CommandMenu.OpenExternalAppMenu(_menuFactory.Value);
            }
        }

        public override MenuItem? CreateMenuItem(bool isDefault)
        {
            var parameter = GetCommandParameter();
            var index = parameter.Index - 1;
            if (isDefault || index < 0)
            {
                return _menuFactory.Value.CreateFolderMenu();
            }
            else
            {
                return null;
            }
        }

        private OpenExternalAppAsCommandParameter GetCommandParameter()
        {
            return (Parameter as OpenExternalAppAsCommandParameter) ?? throw new InvalidOperationException();
        }
    }


    public class ExternalAppOption : IExternalAppOption
    {
        private readonly CommandElement _command;

        public ExternalAppOption(CommandElement command)
        {
            Debug.Assert(command is OpenExternalAppAsCommand || command is OpenBookExternalAppAsCommand);
            _command = command;
        }

        public MultiPagePolicy MultiPagePolicy => _command.Parameter.Cast<OpenExternalAppAsCommandParameter>().MultiPagePolicy;
    }
}
