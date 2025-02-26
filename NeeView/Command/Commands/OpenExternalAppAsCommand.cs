using System;
using System.Windows.Controls;

namespace NeeView
{
    public class OpenExternalAppAsCommand : CommandElement
    {
        private readonly ExternalAppParameterCommandParameterFactory _parameterFactory;

        public OpenExternalAppAsCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.File");
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new OpenExternalAppAsCommandParameter());

            _parameterFactory = new ExternalAppParameterCommandParameterFactory(new ExternalAppOption(this));
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            var index = e.Parameter.Cast<OpenExternalAppAsCommandParameter>().Index - 1;
            if (index >= 0)
            {
                return MainViewExternalAppTools.CanOpenExternalApp(_parameterFactory, index);
            }
            else
            {
                return true;
            }
        }

        public override void Execute(object? sender, CommandContext e)
        {
            var index = e.Parameter.Cast<OpenExternalAppAsCommandParameter>().Index - 1;
            if (index >= 0)
            {
                MainViewExternalAppTools.OpenExternalApp(_parameterFactory, index);
            }
            else
            {
                MainViewComponent.Current.MainView.CommandMenu.OpenExternalAppMenu(_parameterFactory);
            }
        }

        public override MenuItem? CreateMenuItem()
        {
            return MainViewExternalAppTools.CreateExternalAppItem(_parameterFactory);
        }
    }


    public class ExternalAppOption : IExternalAppOption
    {
        private readonly OpenExternalAppAsCommand _command;

        public ExternalAppOption(OpenExternalAppAsCommand command)
        {
            _command = command;
        }

        public MultiPagePolicy MultiPagePolicy => _command.Parameter.Cast<OpenExternalAppAsCommandParameter>().MultiPagePolicy;
    }
}
