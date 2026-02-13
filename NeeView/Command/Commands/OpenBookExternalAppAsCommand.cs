using NeeView.Properties;
using System;
using System.Windows.Controls;

namespace NeeView
{
    public class OpenBookExternalAppAsCommand : CommandElement
    {
        private readonly Lazy<OpenBookExternalAppMenuFactory> _menuFactory;


        public OpenBookExternalAppAsCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new OpenBookExternalAppAsCommandParameter());

            var parameterFactory = new ExternalAppParameterCommandParameterFactory(new StaticExternalAppOption(MultiPagePolicy.Once));
            _menuFactory = new Lazy<OpenBookExternalAppMenuFactory>(() => new OpenBookExternalAppMenuFactory(parameterFactory));
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            var parameter = e.Parameter.Cast<OpenBookExternalAppAsCommandParameter>();
            var index = parameter.Index - 1;
            if (index >= 0)
            {
                var externalApps = Config.Current.System.ExternalAppCollection;
                if (!externalApps.IsValidIndex(index)) return false;
                return BookOperation.Current.BookControl.CanOpenExternalApp(externalApps[index]);
            }
            else
            {
                return true;
            }
        }

        public override void Execute(object? sender, CommandContext e)
        {
            var parameter = e.Parameter.Cast<OpenBookExternalAppAsCommandParameter>();
            var index = parameter.Index - 1;
            if (index >= 0)
            {
                var externalApps = Config.Current.System.ExternalAppCollection;
                if (!externalApps.IsValidIndex(index)) return;
                BookOperation.Current.BookControl.OpenExternalApp(externalApps[index]);
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

        private OpenBookExternalAppAsCommandParameter GetCommandParameter()
        {
            return (Parameter as OpenBookExternalAppAsCommandParameter) ?? throw new InvalidOperationException();
        }
    }
}
