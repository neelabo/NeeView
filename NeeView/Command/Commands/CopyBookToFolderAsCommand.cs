using NeeView.Properties;
using System;
using System.Windows.Controls;

namespace NeeView
{
    public class CopyBookToFolderAsCommand : CommandElement
    {
        private readonly Lazy<CopyBookToFolderMenuFactory> _menuFactory;

        public CopyBookToFolderAsCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new CopyBookToFolderAsCommandParameter());

            _menuFactory = new Lazy<CopyBookToFolderMenuFactory>(() => new CopyBookToFolderMenuFactory(new DestinationFolderParameterCommandParameterFactory(new StaticDestinationFolderOption(MultiPagePolicy.Once))));
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            var parameter = e.Parameter.Cast<CopyBookToFolderAsCommandParameter>();
            var index = parameter.Index - 1;
            if (index >= 0)
            {
                var folders = Config.Current.System.DestinationFolderCollection;
                if (!folders.IsValidIndex(index)) return false;
                return BookOperation.Current.BookControl.CanCopyBookToFolder(folders[index]);
            }
            else
            {
                return true;
            }
        }

        public override void Execute(object? sender, CommandContext e)
        {
            var parameter = e.Parameter.Cast<CopyBookToFolderAsCommandParameter>();
            var index = parameter.Index - 1;
            if (index >= 0)
            {
                var folders = Config.Current.System.DestinationFolderCollection;
                if (!folders.IsValidIndex(index)) return;
                BookOperation.Current.BookControl.CopyBookToFolder(folders[index]);
            }
            else
            {
                MainViewComponent.Current.MainView.CommandMenu.OpenDestinationFolderMenu(_menuFactory.Value);
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

        private CopyBookToFolderAsCommandParameter GetCommandParameter()
        {
            return (Parameter as CopyBookToFolderAsCommandParameter) ?? throw new InvalidOperationException();
        }
    }
}
