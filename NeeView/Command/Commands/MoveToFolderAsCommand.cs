using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Controls;

namespace NeeView
{
    public class MoveToFolderAsCommand : CommandElement
    {
        private readonly Lazy<MovePageToFolderMenuFactory> _menuFactory;
        private readonly bool _useDestinationFolderPanelMode;

        /// <summary>
        /// Initialize the regular destination-folder menu command.
        /// </summary>
        public MoveToFolderAsCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initialize a configurable numbered destination-folder command.
        /// </summary>
        /// <param name="index">One-based destination-folder index and default numeric shortcut.</param>
        public MoveToFolderAsCommand(int index)
            : this($"MoveToDestinationFolder{index}")
        {
            if (index is < 1 or > 9)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "Destination folder shortcut index must be between 1 and 9.");
            }

            // Only built-in number shortcuts follow panel mode; MoveToFolderAs remains a move command.
            _useDestinationFolderPanelMode = true;

            // Keep the folder index and shortcut in the native command configuration so users can edit both.
            Parameter = new MoveToFolderAsCommandParameter()
            {
                Index = index,
                MultiPagePolicy = MultiPagePolicy.Once,
            };
            ShortCutKey = new ShortcutKey(index.ToString(CultureInfo.InvariantCulture));
            Text = $"{Text} {index.ToString(CultureInfo.InvariantCulture)}";
            Menu = $"{Menu} {index.ToString(CultureInfo.InvariantCulture)}";
            IsCloneable = false;
        }

        /// <summary>
        /// Initialize the shared implementation with an optional stable command name.
        /// </summary>
        /// <param name="name">Command name, or null for the original menu command.</param>
        private MoveToFolderAsCommand(string? name)
            : base(name)
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new MoveToFolderAsCommandParameter());

            _menuFactory = new Lazy<MovePageToFolderMenuFactory>(() => new MovePageToFolderMenuFactory(
                parameterFactory: new DestinationFolderParameterCommandParameterFactory(new MoveToDestinationFolderOption(this)),
                option: new MoveToDestinationFolderOption(this)));
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
                if (!BookOperation.Current.Control.CanMoveToFolder(folders[index], parameter.MultiPagePolicy)) return false;
                return !_useDestinationFolderPanelMode
                    || !Config.Current.Panels.IsDestinationFolderCopyMode
                    || BookOperation.Current.Control.CanCopyToFolder(folders[index], parameter.MultiPagePolicy);
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
                // In copy mode, numbered shortcuts use the existing copy path and keep the source image.
                if (_useDestinationFolderPanelMode && Config.Current.Panels.IsDestinationFolderCopyMode)
                {
                    BookOperation.Current.Control.CopyToFolder(folders[index], parameter.MultiPagePolicy);
                }
                else
                {
                    BookOperation.Current.Control.MoveToFolder(folders[index], parameter.MultiPagePolicy);
                }
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
