using NeeView.Properties;
using System;
using System.Reflection.Metadata;
using System.Windows.Controls;

namespace NeeView
{
    public class MoveToFolderAsCommand : CommandElement
    {
        private readonly DestinationFolderParameterCommandParameterFactory _parameterFactory;
        private MoveableViewPageBindingSource? _bindingSource;

        public MoveToFolderAsCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new MoveToFolderAsCommandParameter());

            _parameterFactory = new DestinationFolderParameterCommandParameterFactory(new MoveToDestinationFolderOption(this));
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            if (!Config.Current.System.IsFileWriteAccessEnabled) return false;

            var parameter = e.Parameter.Cast<MoveToFolderAsCommandParameter>();
            var index = parameter.Index - 1;
            if (index >= 0)
            {
                var folders = Config.Current.System.DestinationFolderCollection;
                if (!folders.IsValidIndex(index)) return false;
                return BookOperation.Current.Control.CanMoveToFolder(folders[index], parameter.MultiPagePolicy);
            }
            else
            {
                return true;
            }
        }

        public override void Execute(object? sender, CommandContext e)
        {
            var parameter = e.Parameter.Cast<MoveToFolderAsCommandParameter>();
            var index = parameter.Index - 1;
            if (index >= 0)
            {
                var folders = Config.Current.System.DestinationFolderCollection;
                if (!folders.IsValidIndex(index)) return;
                BookOperation.Current.Control.MoveToFolder(folders[index], parameter.MultiPagePolicy);
            }
            else
            {
                MainViewComponent.Current.MainView.CommandMenu.OpenMoveToFolderMenu(_parameterFactory);
            }
        }

        public override MenuItem? CreateMenuItem(bool isDefault)
        {
            var parameter = GetCommandParameter();
            var index = parameter.Index - 1;
            if (isDefault || index < 0)
            {
                _bindingSource ??= new MoveableViewPageBindingSource(PageFrameBoxPresenter.Current, new MoveToDestinationFolderOption(this));
                return MainViewMoveToFolderTools.CreateMoveToFolderItem(_parameterFactory, _bindingSource);
            }
            else
            {
                return null;
            }
        }

        private MoveToFolderAsCommandParameter GetCommandParameter()
        {
            return (Parameter as MoveToFolderAsCommandParameter) ?? throw new InvalidOperationException();
        }
    }



    public class MoveToDestinationFolderOption : IDestinationFolderOption
    {
        private readonly MoveToFolderAsCommand _command;

        public MoveToDestinationFolderOption(MoveToFolderAsCommand command)
        {
            _command = command;
        }

        public MultiPagePolicy MultiPagePolicy => _command.Parameter.Cast<MoveToFolderAsCommandParameter>().MultiPagePolicy;
    }
}
